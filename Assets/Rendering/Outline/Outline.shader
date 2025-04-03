Shader "Effect/Outline"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off ZTest Always

        HLSLINCLUDE 
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        // Basically we're reducing the space we use to store our position so we can have
        // our NULL POS of -1.

        // Shoutout to GOAT: Ben Golus
        #define SNORM16_MAX_FLOAT_MINUS_EPSILON ((float)(32768-2) / (float)(32768-1))
        #define FLOOD_ENCODE_OFFSET float2(1.0, SNORM16_MAX_FLOAT_MINUS_EPSILON)
        #define FLOOD_ENCODE_SCALE float2(2.0, 1.0 + SNORM16_MAX_FLOAT_MINUS_EPSILON)

        #define FLOOD_NULL_POS -1.0
        #define FLOOD_NULL_POS_FLOAT2 float2(FLOOD_NULL_POS, FLOOD_NULL_POS)

        ENDHLSL

        // 0
        Pass 
        {
            Name "Silhouette"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            float4 Vert(float4 vertex : POSITION) : SV_POSITION
            {
                float4 clipSpacePosition = TransformObjectToHClip(vertex.xyz);
                #ifdef UNITY_UV_STARTS_AT_TOP
                // vertexOutput.positionCS.y = -vertexOutput.positionCS.y;
                #endif
                return clipSpacePosition;
            }

            half Frag() { return 1.0; }

            ENDHLSL
        }
        
        // 1
        Pass
        {
            Name "JumpFloodInit"
            HLSLPROGRAM

            // The Blit.hlsl file provides the vertex shader (Vert),
            // the input structure (Attributes), and the output structure (Varyings)
            // But the Blit texture loaded (_BlitTexture) is a Texture2D<float4> seemingly 
            // always and my graphics format is 2 channel
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            float2 Frag(Varyings input) : SV_Target
            {
                int2 uvInt = input.positionCS.xy;

                if(_BlitTexture.Load(uvInt.x, uvInt.y) > 0.5)
                {
                    return input.positionCS.xy * abs(_BlitTexture_TexelSize.xy) * FLOOD_ENCODE_SCALE - FLOOD_ENCODE_OFFSET;
                }
                else 
                {
                    return FLOOD_NULL_POS_FLOAT2;
                }
            }

            ENDHLSL
        }

        // 2
        Pass 
        {
            Name "JumpFloodPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #pragma vertex Vert
            #pragma fragment JumpFlood

            int _StepWidth;
            
            float4 Vert(Varyings input) : SV_POSITION
            {
                int2 uvInt = input.positionCS.xy;

                float nearestDist = 1.#INF;
                float2 nearestPixel;

                UNITY_UNROLL
                for (int i = -1; i <= 1; ++i)
                {
                    UNITY_UNROLL
                    for (int j = -1; j <= 1; ++j)
                    {
                        int2 offset = uvInt + int2(i,j) * _StepWidth;
                        offset = clamp(offset, int2(0,0), (int2)_BlitTexture_TexelSize.zw - 1);

                        float2 offsetPos = (_BlitTexture.Load(int3(offset, 0)).rg + FLOOD_ENCODE_OFFSET) * _BlitTexture_TexelSize.zw / FLOOD_ENCODE_SCALE;
                        float2 disp = input.positionCS.xy - offsetPos;

                        float dist = dot(disp, disp);

                        if (offsetPos.y != FLOOD_NULL_POS && dist < bestDist)
                        {
                            nearestDist = dist;
                            nearestCoord = offsetPos;
                        }
                    }
                }
                return isinf(bestDist) ? FLOOD_NULL_POS_FLOAT2 : bestCoord * _BlitTexture_TexelSize.xy * FLOOD_ENCODE_SCALE - FLOOD_ENCODE_OFFSET;
            }
            ENDHLSL
        }

        // 3
        Pass
        {
            Name "ColorOutline"

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            float _OutlineThickness;
            float4 _OutlineColor;

            TEXTURE2D(_SilhouetteTex);

            half4 Frag(Varyings input) : SV_Target
            {
                int2 uvInt = int2(input.positionCS.xy);

                // load encoded position
                float2 encodedPos = _BlitTexture.Load(int3(uvInt, 0)).rg;

                // early out if null position
                if (encodedPos.y == FLOOD_NULL_POS)
                    return half4(0,0,0,0);

                // decode closest position
                // NOTE: _ScreenParams is of form (width, height, 1/width, 1/height) and it comes from
                // Core.hlsl in the global import (specifically Core.hlsl < Input.hlsl < UnityInput.hlsl)
                float2 nearestPos = (encodedPos + FLOOD_ENCODE_OFFSET) * abs(_ScreenParams.xy) / FLOOD_ENCODE_SCALE;

                // current pixel position
                float2 currentPos = i.pos.xy;

                // distance in pixels to closest position
                half dist = length(nearestPos - currentPos);

                // Outline should only be drawn on pixels within _OutlineThickness
                half outline = saturate(_OutlineThickness - dist + 0.5);

                // We don't have stencil, don't drawn outline on sprite pixels.
                half spriteMask = (1.0 - _SilhouetteTex.Load(int3(uvInt, 0)).r);

                // apply outline to alpha
                half4 col = _OutlineColor;
                col.a *= outline * spriteMask;

                return col;
            }
            ENDHLSL
        }

    }
}
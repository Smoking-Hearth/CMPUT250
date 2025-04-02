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
        #define SNORM16_MAX_FLOAT_MINUS_EPSILON ((float)(32768-2) / (float)(32768-1))
        #define FLOOD_ENCODE_OFFSET float2(1.0, SNORM16_MAX_FLOAT_MINUS_EPSILON)
        #define FLOOD_ENCODE_SCALE float2(2.0, 1.0 + SNORM16_MAX_FLOAT_MINUS_EPSILON)

        #define FLOOD_NULL_POS -1.0
        #define FLOOD_NULL_POS_FLOAT2 float2(FLOOD_NULL_POS, FLOOD_NULL_POS)
        ENDHLSL

        // How can I do a stencil pass adapted for sprites which sets the stencil bit
        // based on the alpha value of the sprites texture? 
        // 0
        Pass 
        {
            Name "InnerStencil"
            Stencil
            {
                Ref 1
                ReadMask 1
                WriteMask 1
                Comp NotEqual
                Pass Replace
            }

            ColorMask 0
            Blend Zero One

            HLSLPROGRAM
            #pragma vertex Vert
            
            // I Hope this is correct.
            float4 Vert(float4 vertex : POSITION) : SV_POSITION
            {
                return TransformObjectToHClip(vertex.xyz);
            }

            // Why is this empty?
            ENDHLSL
        }

        // 1
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
                clipSpacePosition.y = -clipSpacePosition.y;
                #endif
                return clipSpacePosition;
            }

            half Frag() { return 1.0; }

            ENDHLSL
        }
        
        // 2 
        Pass
        {
            Name "JumpFloodInit"
            HLSLPROGRAM
            // The Blit.hlsl file provides the vertex shader (Vert),
            // the input structure (Attributes), and the output structure (Varyings)
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            Texture2D _MainTex;

            float2 Frag(Varyings input) : SV_Target
            {
                int2 uvInt = input.positionCS.xy;

                if(_MainTex.Load(uvInt.x, uvInt.y) > 0.5)
                {
                    return input.positionCS.xy * abs(_MainTex_TexelSize.xy) * FLOOD_ENCODE_SCALE - FLOOD_ENCODE_OFFSET;
                }
                else 
                {
                    return FLOOD_NULL_POS_FLOAT2;
                }
            }

            ENDHLSL
        }

        // 3
        Pass 
        {
            Name "JumpFloodPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #pragma vertex Vert
            #pragma fragment JumpFlood

            float _OutlineThickness;
            float4 _OutlineColor;
            int _StepWidth;
            
            Texture2D _MainTex;
            
            float4 Vert(Varyings input) : SV_POSITION
            {
                int2 uvInt = input.positionCS.xy;

                float shortestDist = 1.#INF;
                float2 theCoord;

                UNITY_UNROLL
                for (int i = -1; i <= 1; ++i)
                {
                    UNITY_UNROLL
                    for (int j = -1; j <= 1; ++j)
                    {
                        int2 offset = uvInt + int2(i,j) * _StepWidth;
                        offset = clamp(offset, int2(0,0), (int2)_MainTex_TexelSize.zw - 1);

                        float2 offsetPos = (_MainTex.Load(int3(offset, 0)).rg + FLOOD_ENCODE_OFFSET) * _MainTex_TexelSize.zw / FLOOD_ENCODE_SCALE;
                        float2 disp = input.positionCS.xy - offsetPos;

                        float dist = dot(disp, disp);

                        if (offsetPos.y != FLOOD_NULL_POS && dist < bestDist)
                        {
                            bestDist = dist;
                            bestCoord = offsetPos;
                        }
                    }
                }
                return isinf(bestDist) ? FLOOD_NULL_POS_FLOAT2 : bestCoord * _MainTex_TexelSize.xy * FLOOD_ENCODE_SCALE - FLOOD_ENCODE_OFFSET;
            }
            ENDHLSL
        }

        // 4
        Pass
        {
            Name "ColorOutline"

            Stencil {
                Ref 1
                ReadMask 1
                WriteMask 1
                Comp NotEqual
                Pass Zero
                Fail Zero
            }

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            float4 Vert(Varyings input) : SV_POSITION
            {
                int2 uvInt = int2(input.positionCS.xy);

                // load encoded position
                float2 encodedPos = _MainTex.Load(int3(uvInt, 0)).rg;

                // early out if null position
                if (encodedPos.y == -1)
                    return half4(0,0,0,0);

                // decode closest position
                float2 nearestPos = (encodedPos + FLOOD_ENCODE_OFFSET) * abs(_ScreenParams.xy) / FLOOD_ENCODE_SCALE;

                // current pixel position
                float2 currentPos = i.pos.xy;

                // distance in pixels to closest position
                half dist = length(nearestPos - currentPos);

                half outline = saturate(_OutlineWidth - dist + 0.5);

                // apply outline to alpha
                half4 col = _OutlineColor;
                col.a *= outline;

                return col;

            }
            ENDHLSL
        }

    }
}
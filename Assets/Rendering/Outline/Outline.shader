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
            #pragma fragment Frag
            
            // I Hope this is correct.
            float4 Vert(float4 vertex : POSITION) : SV_POSITION
            {
                return TransformObjectToHClip(vertex.xyz);
            }

            // Why is this empty?
            float4 Frag () {}
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
            float4 _MainTex_TexelSize;

            float2 Frag(Varyings input) : SV_Target
            {
                int2 uvInt = input.positionCS.xy;


                // How to I to this if based on the result of the stencil buffer test?
                if()
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

            int _OutlineThickness;
            


            ENDHLSL
        }

        // 4
        Pass
        {
            Name "ColorOutline"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            ENDHLSL
        }

    }
}
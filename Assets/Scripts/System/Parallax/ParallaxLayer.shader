Shader "Unlit/ParallaxLayer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // Note: The RenderPipeline tag ensures the shader is used with URP.
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            Name "ForwardLit"
            HLSLPROGRAM
            // Define the vertex and fragment entry points
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            // Include URP core and fog libraries (the paths are relative to the package)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Define input attributes for the vertex shader
            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            // Define data passed from the vertex to the fragment shader.
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // Declare the texture using new macros.
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                // Transform vertex position from object space to homogeneous clip space using URP's function.
                OUT.vertex = TransformObjectToHClip(IN.vertex);
                // Transform UV coordinates (applies _MainTex_ST automatically).
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Sample the texture using the URP macros.
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
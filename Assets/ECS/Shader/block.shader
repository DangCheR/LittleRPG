Shader "Custom/WorldTiling_DOTS"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _TextureSize ("Texture Size (Meters)", Float) = 5.0 
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            // ==========================================
            // 【终极补丁：拔升 Shader Model 级别】
            // 告诉编译器：“别管那些破手机了，我这个 Shader 是跑在支持 SSBO 的现代硬件上的！”
            // ==========================================
            #pragma target 4.5
            
            #pragma vertex vert
            #pragma fragment frag

            // 开启 DOTS 实例化变体
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;  
                float _TextureSize;  
            CBUFFER_END

            Varyings vert (Attributes v)
            {
                Varyings o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float2 uv = i.worldPos.xz / _TextureSize;

                return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
            }

            ENDHLSL
        }
    }
}
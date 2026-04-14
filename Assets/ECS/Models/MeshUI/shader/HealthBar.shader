Shader "Custom/HealthBar"
{
    Properties
    {
        // 这里的名字要和 C# 里的字符串一模一样
        _HealthPct ("Health Percentage", Float) = 1.0
        _ColorHealth ("Health Color", Color) = (0, 1, 0, 1)
        _ColorBG ("Background Color", Color) = (1, 0, 0, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // 允许透明
            ZWrite Off // 血条一般不写深度

            HLSLPROGRAM
            #pragma target 4.5

            #pragma vertex vert
            #pragma fragment frag
            // 这一行必须加，为了支持 DOTS 的合批
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // 这里的 ID 必须写
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // 这里的 ID 也要写
            };

            // --- 关键点：DOTS 数据缓冲区 ---
            CBUFFER_START(UnityPerMaterial)
                float _HealthPct;
                float4 _ColorHealth;
                float4 _ColorBG;
            CBUFFER_END

            Varyings vert (Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i); // 重点：根据实例 ID 取自己的血量
                
                // 逻辑开始
                // i.uv.x 是横向坐标 (0~1)
                // step(A, B): 如果 A < B 返回 1，否则返回 0
                float isHealth = step(i.uv.x, _HealthPct);
                
                // 混合颜色：isHealth 为 1 显绿色，为 0 显背景红
                return lerp(_ColorBG, _ColorHealth, isHealth);
            }
            ENDHLSL
        }
    }
}
Shader "Custom/DarkenAndGaussianBlurByDepth2D"
{
    Properties {
        _MainTex("Sprite", 2D) = "white" {}
        _MaxZLevel("Max Z Level (full effect)", Float) = 10
        _BlurLevel("Blur Level", Range(0, 24)) = 6
        _Darkness("Darkness", Range(0, 1)) = 0.6
    }

    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float worldZ : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;

            float _MaxZLevel;
            float _BlurLevel;
            float _Darkness;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.pos = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;

                float3 worldPos = TransformObjectToWorld(v.vertex).xyz;
                o.worldZ = worldPos.z;

                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float maxZ = max(abs(_MaxZLevel), 1e-5);
                float t = saturate(abs(i.worldZ) / maxZ);
                float blurRadius = _BlurLevel * t * t;
                float2 blurStep1 = _MainTex_TexelSize.xy * blurRadius;
                float2 blurStep2 = blurStep1 * 2.0;

                float4 colNear = 0.0;
                float4 colFar = 0.0;

                // 3x3 Gaussian kernel:
                // [1 2 1]
                // [2 4 2] / 16
                // [1 2 1]
                colNear += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-blurStep1.x, -blurStep1.y)) * 1.0;
                colNear += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0.0,        -blurStep1.y)) * 2.0;
                colNear += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( blurStep1.x, -blurStep1.y)) * 1.0;

                colNear += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-blurStep1.x, 0.0)) * 2.0;
                colNear += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * 4.0;
                colNear += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( blurStep1.x, 0.0)) * 2.0;

                colNear += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-blurStep1.x,  blurStep1.y)) * 1.0;
                colNear += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0.0,         blurStep1.y)) * 2.0;
                colNear += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( blurStep1.x,  blurStep1.y)) * 1.0;
                colNear *= (1.0 / 16.0);

                colFar += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-blurStep2.x, -blurStep2.y)) * 1.0;
                colFar += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0.0,        -blurStep2.y)) * 2.0;
                colFar += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( blurStep2.x, -blurStep2.y)) * 1.0;

                colFar += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-blurStep2.x, 0.0)) * 2.0;
                colFar += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * 4.0;
                colFar += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( blurStep2.x, 0.0)) * 2.0;

                colFar += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-blurStep2.x,  blurStep2.y)) * 1.0;
                colFar += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0.0,         blurStep2.y)) * 2.0;
                colFar += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( blurStep2.x,  blurStep2.y)) * 1.0;
                colFar *= (1.0 / 16.0);

                float4 col = lerp(colNear, colFar, t);

                float darknessFactor = lerp(1.0, 1.0 - _Darkness, t);
                col.rgb *= darknessFactor;

                return col;
            }

            ENDHLSL
        }
    }
}

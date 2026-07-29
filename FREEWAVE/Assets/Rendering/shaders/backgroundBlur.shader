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
                float2 blurStep = _MainTex_TexelSize.xy * blurRadius;
                float2 blurStepFar = blurStep * 1.8;

                // Smoother 13-tap kernel with fractional offsets to reduce blocky artifacts.
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * 0.16;

                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( blurStep.x, 0.0)) * 0.09;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-blurStep.x, 0.0)) * 0.09;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0.0,  blurStep.y)) * 0.09;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0.0, -blurStep.y)) * 0.09;

                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( 0.7071 * blurStep.x,  0.7071 * blurStep.y)) * 0.06;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-0.7071 * blurStep.x,  0.7071 * blurStep.y)) * 0.06;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( 0.7071 * blurStep.x, -0.7071 * blurStep.y)) * 0.06;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-0.7071 * blurStep.x, -0.7071 * blurStep.y)) * 0.06;

                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2( blurStepFar.x, 0.0)) * 0.07;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-blurStepFar.x, 0.0)) * 0.07;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0.0,  blurStepFar.y)) * 0.07;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0.0, -blurStepFar.y)) * 0.07;

                float darknessFactor = lerp(1.0, 1.0 - _Darkness, t);
                col.rgb *= darknessFactor;

                return col;
            }

            ENDHLSL
        }
    }
}

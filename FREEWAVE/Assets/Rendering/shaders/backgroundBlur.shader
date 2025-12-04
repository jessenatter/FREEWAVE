Shader "Custom/DarkenByDepth2D"
{
    Properties {
        _MainTex("Sprite", 2D) = "white" {}
        _ZMin("Brightest Z", Float) = 0
        _ZMax("Darkest Z", Float) = -10
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

            float _ZMin;
            float _ZMax;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.pos = TransformObjectToHClip(v.vertex);

                float3 worldPos = TransformObjectToWorld(v.vertex).xyz;
                o.worldZ = worldPos.z;

                o.uv = v.uv;
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                float t = saturate( (i.worldZ - _ZMin) / (_ZMax - _ZMin) );

                col.rgb *= (1.0 - t); // darker in background

                return col;
            }

            ENDHLSL
        }
    }
}

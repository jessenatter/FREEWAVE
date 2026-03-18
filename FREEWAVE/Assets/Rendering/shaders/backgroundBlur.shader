Shader "Custom/DarkenAndBlurByDepth2D_Mip"
{
    Properties {
        _MainTex("Sprite", 2D) = "white" {}
        _ZMin("Brightest Z", Float) = 0
        _ZMax("Darkest Z", Float) = -10
        _MaxMip("Max Blur Mip Level", Float) = 5
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
            float _MaxMip;

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
                float t = saturate((i.worldZ - _ZMin)/(_ZMax - _ZMin));

                float mipLevel = t * _MaxMip;

                float4 col = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv, mipLevel);

                float2 offset = 1.0 / float2(2048, 2048);
                col += SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv + float2(offset.x, 0), mipLevel);
                col += SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv - float2(offset.x, 0), mipLevel);
                col += SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv + float2(0, offset.y), mipLevel);
                col += SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, i.uv - float2(0, offset.y), mipLevel);
                col /= 5.0;

                col.rgb *= (1.0 - t);

                return col;
            }

            ENDHLSL
        }
    }
}

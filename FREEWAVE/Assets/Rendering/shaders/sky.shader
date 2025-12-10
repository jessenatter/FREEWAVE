Shader "Custom/DarkenAndBlurByDepth2D_Mip"
{
    Properties {
        _MainTex("Sprite", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump"{}
        _NormalStrength("Normal Strength", Float) = 0.05
        _Speed("Speed", Float) = 0.5
        _Scale("Scale",Float) = 0.5
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
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            float _NormalStrength;  
            float _Speed;
            float _Scale;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.pos = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;

                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float time = _Time.y * (.1 + (.5 * _Speed));
                float2 uv = i.uv;
                float2 normalUV = uv;
                normalUV.y += _Time.y * 0.03;
                float3 _normal = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, normalUV * (1 + _Scale)).rgb;
                _normal = _normal * 2.0 - 1.0;

                float2 warp = float2(_normal.r, _normal.g) * _NormalStrength;

                float wave = (sin(time) + 1.0) * 0.5;
                float _base = 0.4;
                float intensity = _base + wave * (1-_base); 
                warp *= intensity;

                float2 warpedUV = uv + warp;

                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, warpedUV);

                return col;
            }

            ENDHLSL
        }
    }
}

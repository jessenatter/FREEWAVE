Shader "Custom/DarkenAndBlurByDepth2D_Mip"
{
    Properties 
    {
        _MainTex("Sprite", 2D) = "white" {}
        _CameraTex("Mirror Texture", 2D) = "white" {}
        _MaskTex("Mask Texture", 2D) = "white" {}
        _Distortion("Distortion", Float) = 0.5
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
                float3 worldPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_CameraTex);
            SAMPLER(sampler_CameraTex);

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            
            float _Distortion;
            float4x4 _CameraVP;


            Varyings vert(Attributes v)
            {
                Varyings o;
                o.pos = TransformObjectToHClip(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;

                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float4 sprite = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);

                float _mask = mask.a; 
                
                float4 camPos = mul(_CameraVP, float4(i.worldPos,1));
                float2 uv = camPos.xy / camPos.w * 0.5 + 0.5;
                uv = saturate(uv); 

                float4 reflection = SAMPLE_TEXTURE2D(_CameraTex, sampler_CameraTex, uv);


                float4 col = lerp(sprite, reflection, _mask);
                col = lerp(col,sprite,0.2);

                return col;
            }


            ENDHLSL
        }
    }
}

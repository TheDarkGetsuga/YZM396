Shader "Universal Render Pipeline/2D/BlurLit2D"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Lit"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _USE_SHAPE_LIGHT _USE_POINT_LIGHT _USE_VOLUME_LIGHT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Lighting2D.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _BlurSize;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = float4(1,1,1,1);
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float2 offset = _MainTex_TexelSize.xy * _BlurSize;

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * 0.36;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset.xy) * 0.16;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - offset.xy) * 0.16;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(offset.x, -offset.y)) * 0.16;
                col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + float2(-offset.x, offset.y)) * 0.16;

                // Apply lighting from URP 2D
                col.rgb *= Lighting2D(col.rgb, input.uv);

                return col;
            }
            ENDHLSL
        }
    }
}

Shader "Custom/UnlitVibrantCartoonURP"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0.5, 0.5, 0.5, 1)
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.5
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.01
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _Glossiness ("Glossiness", Range(0, 1)) = 0.5
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimAmount ("Rim Amount", Range(0, 1)) = 0.716
        _RimThreshold ("Rim Threshold", Range(0, 1)) = 0.1
        _LightDirection ("Fake Light Direction", Vector) = (0.5, 0.5, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _ShadowColor;
            float _ShadowThreshold;
            half4 _OutlineColor;
            float _OutlineWidth;
            half4 _SpecularColor;
            float _Glossiness;
            half4 _RimColor;
            float _RimAmount;
            float _RimThreshold;
            float3 _LightDirection;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            float3 viewDirWS : TEXCOORD2;
        };
        ENDHLSL

        // Main Pass
        Pass
        {
            Name "ForwardUnlit"
            Tags
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(TransformObjectToWorld(input.positionOS.xyz));
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

                float3 normalWS = normalize(input.normalWS);
                float3 lightDir = normalize(_LightDirection.xyz);
                float NdotL = dot(normalWS, lightDir);

                // Cel shading
                float celShadow = step(_ShadowThreshold, NdotL);
                half3 celColor = lerp(_ShadowColor.rgb, baseColor.rgb, celShadow);

                // Fake Specular
                float3 viewDir = normalize(input.viewDirWS);
                float3 halfVector = normalize(lightDir + viewDir);
                float NdotH = dot(normalWS, halfVector);
                float specularIntensity = pow(NdotH * celShadow, _Glossiness * 100);
                float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
                half3 specular = specularIntensitySmooth * _SpecularColor.rgb;

                // Fake Rim lighting
                float rimDot = 1 - dot(viewDir, normalWS);
                float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
                rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
                half3 rim = rimIntensity * _RimColor.rgb;

                half3 finalColor = celColor + specular + rim;
                return half4(finalColor, baseColor.a);
            }
            ENDHLSL
        }

        // Outline Pass
        Pass
        {
            Name "Outline"
            Tags
            {
                "LightMode"="SRPDefaultUnlit"
            }
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 normalOS = input.normalOS;
                float3 posOS = input.positionOS.xyz + normalOS * _OutlineWidth;
                output.positionCS = TransformObjectToHClip(posOS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
Shader "Custom/SuperChargeButton"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Color ("Base Color", Color) = (1,1,1,1)
        _ChargeColor ("Charge Color", Color) = (1,0.5,0,1)
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _ChargeAmount ("Charge Amount", Range(0,1)) = 0
        _PulseSpeed ("Pulse Speed", Float) = 1
        _PulseAmount ("Pulse Amount", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent"
        }
        LOD 100

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 tangentWS : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _ChargeColor;
                float4 _EmissionColor;
                float _ChargeAmount;
                float _PulseSpeed;
                float _PulseAmount;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.normalWS = normalInputs.normalWS;
                OUT.tangentWS = float4(normalInputs.tangentWS, IN.tangentOS.w);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                float3 normalMap = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv));

                // Calculate radial UV for circular progress in clock-wise direction
                float2 center = float2(0.5, 0.5);
                float2 uvCentered = uv - center;

                // Calculate progress mask
                float progressMask = 1 - saturate(length(uvCentered) * 2);

                // Calculate edge glow
                float edgeGlow = saturate(1 - length(uvCentered) * 2);


                // Pulsing effect when fully charged
                float pulse = 0;
                if (_ChargeAmount >= 0.99)
                {
                    pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                }

                // Combine effects
                float3 finalColor = lerp(_Color.rgb, _ChargeColor.rgb, progressMask);
                finalColor += edgeGlow * _ChargeColor.rgb;
                finalColor += pulse * _PulseAmount * _EmissionColor.rgb;

                // Apply normal mapping
                float3 normalWS = normalize(IN.normalWS);
                float3 tangentWS = normalize(IN.tangentWS.xyz);
                float3 bitangentWS = normalize(cross(normalWS, tangentWS) * IN.tangentWS.w);
                float3x3 tangentToWorld = float3x3(tangentWS, bitangentWS, normalWS);
                float3 normalTS = normalMap;
                float3 normal = normalize(mul(normalTS, tangentToWorld));

                // Simple lighting
                float3 lightDir = normalize(_MainLightPosition.xyz);
                float NdotL = max(0, dot(normal, lightDir));
                float3 diffuse = NdotL * _MainLightColor.rgb;

                finalColor *= mainTex.rgb * (diffuse + unity_AmbientSky.rgb);

                return float4(finalColor, mainTex.a);
            }
            ENDHLSL
        }
    }
}
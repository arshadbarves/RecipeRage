Shader "Custom/StripedWall"
{
    Properties
    {
        _Color1 ("Stripe Color 1", Color) = (0.68, 0.81, 0.92, 1) // Light Blue
        _Color2 ("Stripe Color 2", Color) = (0.99, 0.99, 0.58, 1) // Cream/White
        _StripeWidth ("Stripe Width", Float) = 0.5
        _StripeAngle ("Stripe Angle", Range(0, 360)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        _NoiseTex ("Dirt Noise", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 1.0
        _DirtIntensity ("Dirt Intensity", Range(0, 1)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" "Queue"="Geometry" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile _ LIGHTMAP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 2);
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color1;
                float4 _Color2;
                float _StripeWidth;
                float _StripeAngle;
                float _Smoothness;
                float4 _NoiseTex_ST;
                float _NoiseScale;
                float _DirtIntensity;
            CBUFFER_END

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            float2 RotateUV(float2 uv, float angle)
            {
                float s = sin(angle * PI / 180.0);
                float c = cos(angle * PI / 180.0);
                float2x2 rotationMatrix = float2x2(c, -s, s, c);
                return mul(uv - 0.5, rotationMatrix) + 0.5;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                
                OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
                OUTPUT_SH(output.normalWS, output.vertexSH);
                
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Rotate UV for stripe angle
                float2 rotatedUV = RotateUV(input.uv, _StripeAngle);

                // Stripe Logic
                // Use frac to create repeating pattern
                float stripePattern = frac(rotatedUV.x * _StripeWidth);
                
                // Select color based on threshold (0.5 for equal width stripes)
                half3 albedo = stripePattern < 0.5 ? _Color1.rgb : _Color2.rgb;

                // Dirt/Noise
                float2 noiseUV = input.uv * _NoiseScale;
                half4 noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV);
                albedo = lerp(albedo, albedo * noise.rgb, _DirtIntensity);

                // Lighting
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, input.normalWS);
                
                half3 lightColor = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                // half3 ambient = SampleSH(input.normalWS); // Replaced by bakedGI
                
                half NdotL = saturate(dot(input.normalWS, mainLight.direction));
                // Add bakedGI
                half3 diffuse = albedo * (lightColor * NdotL + bakedGI);

                return half4(diffuse, 1.0);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Meta"
            Tags { "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM
            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMeta

            #pragma shader_feature_local_fragment _SPECULAR_SETUP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _ _DETAIL_MULX2 _DETAIL_SCALED

            #pragma shader_feature_local_fragment _SPECGLOSSMAP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaPass.hlsl"

            ENDHLSL
        }
    }
}

Shader "Custom/RandomTileFloor"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0.68, 0.77, 0.81, 1) // Pastel Blue
        _Color2 ("Color 2", Color) = (1.0, 0.71, 0.70, 1) // Pastel Pink
        _Color3 ("Color 3", Color) = (0.99, 0.99, 0.58, 1) // Cream/White
        _Color4 ("Color 4", Color) = (0.70, 0.62, 0.71, 1) // Grey/Purple
        _GridColor ("Grid Color", Color) = (0.5, 0.5, 0.5, 1)
        _GridScale ("Grid Scale", Float) = 5.0
        _GridThickness ("Grid Thickness", Range(0.001, 0.1)) = 0.02
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        _NoiseTex ("Dirt Noise", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 1.0
        _DirtIntensity ("Dirt Intensity", Range(0, 1)) = 0.2
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
                float4 _Color3;
                float4 _Color4;
                float4 _GridColor;
                float _GridScale;
                float _GridThickness;
                float _Smoothness;
                float4 _NoiseTex_ST;
                float _NoiseScale;
                float _DirtIntensity;
            CBUFFER_END

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
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
                // Grid Logic
                float2 scaledUV = input.uv * _GridScale;
                float2 tileID = floor(scaledUV);
                float2 f = frac(scaledUV);

                // Random Color Selection
                float rnd = random(tileID);
                half3 tileColor;
                
                if (rnd < 0.25) tileColor = _Color1.rgb;
                else if (rnd < 0.5) tileColor = _Color2.rgb;
                else if (rnd < 0.75) tileColor = _Color3.rgb;
                else tileColor = _Color4.rgb;

                // Dirt/Noise
                float2 noiseUV = input.uv * _NoiseScale;
                half4 noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV);
                tileColor = lerp(tileColor, tileColor * noise.rgb, _DirtIntensity);

                // Grid Lines
                float2 df = min(f, 1.0 - f);
                float distToEdge = min(df.x, df.y);
                float lineAa = fwidth(distToEdge);
                float gridFactor = smoothstep(_GridThickness, _GridThickness + lineAa, distToEdge);

                half3 albedo = lerp(_GridColor.rgb, tileColor, gridFactor);

                // Lighting
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, input.normalWS);
                
                half3 lightColor = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                // half3 ambient = SampleSH(input.normalWS); // Replaced by bakedGI
                
                half NdotL = saturate(dot(input.normalWS, mainLight.direction));
                // Add bakedGI to the lighting equation
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

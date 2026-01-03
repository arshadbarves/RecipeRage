Shader "Custom/VoxelFloor"
{
    Properties
    {
        _BaseColor ("Sand Color", Color) = (0.85, 0.55, 0.45, 1) // #D98E73 approx
        _GridColor ("Grid Color", Color) = (0.71, 0.38, 0.27, 1) // #B56046 approx
        _GridScale ("Grid Scale", Float) = 1.0
        _GridThickness ("Grid Thickness", Range(0.001, 0.1)) = 0.02
        _Smoothness ("Smoothness", Range(0, 1)) = 0.1
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1; // Used for world space calculations
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _GridColor;
                float _GridScale;
                float _GridThickness;
                float _Smoothness;
            CBUFFER_END

            // Random function for cell variation
            float2 noise_random(float2 p) {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Grid Logic
                float2 scaledUV = input.uv * _GridScale;
                float2 cellID = floor(scaledUV); // Integer ID for each cell
                float2 f = frac(scaledUV);
                
                // Random variation per cell
                float2 noiseVal = noise_random(cellID); // Use same random function
                float randomVariation = noiseVal.x * 0.1; // 10% variation range
                
                // Color with variation
                half3 cellColor = _BaseColor.rgb + (randomVariation * 0.1); 
                // Alternatively, tint slightly:
                // half3 cellColor = lerp(_BaseColor.rgb, _BaseColor.rgb * 0.9, noiseVal.x);

                float2 df = min(f, 1.0 - f);
                float distToEdge = min(df.x, df.y);
                
                float lineAa = fwidth(distToEdge);
                float gridFactor = smoothstep(_GridThickness, _GridThickness + lineAa, distToEdge);
                
                half3 albedo = lerp(_GridColor.rgb, cellColor, gridFactor);

                // Lighting
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 lightColor = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                half3 ambient = SampleSH(input.normalWS);
                
                half NdotL = saturate(dot(input.normalWS, mainLight.direction));
                half3 diffuse = albedo * (lightColor * NdotL + ambient);

                return half4(diffuse, 1.0);
            }
            ENDHLSL
        }
        
        // ShadowCaster Pass
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
    }
}

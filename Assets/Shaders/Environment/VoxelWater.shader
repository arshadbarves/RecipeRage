Shader "Custom/VoxelWater"
{
    Properties
    {
        _DeepColor ("Deep Color", Color) = (0.3, 0.75, 0.85, 0.8) // #4EC0D9 approx
        _ShallowColor ("Shallow Color", Color) = (0.4, 0.85, 0.95, 0.5)
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveScale ("Wave Scale", Float) = 5.0
        
        _FoamThreshold ("Foam Threshold", Range(0, 2)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline" "Queue"="Transparent" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _DeepColor;
                float4 _ShallowColor;
                float4 _FoamColor;
                float _WaveSpeed;
                float _WaveScale;
                float _FoamThreshold;
            CBUFFER_END

            // Random function for Voronoi
            float2 noise_random(float2 p) {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }

            // Simple noise
            float simple_noise(float2 p) {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f*f*(3.0-2.0*f);
                return lerp( lerp( dot( noise_random(i + float2(0.0,0.0) ), f - float2(0.0,0.0) ),
                                   dot( noise_random(i + float2(1.0,0.0) ), f - float2(1.0,0.0) ), u.x),
                             lerp( dot( noise_random(i + float2(0.0,1.0) ), f - float2(0.0,1.0) ),
                                   dot( noise_random(i + float2(1.0,1.0) ), f - float2(1.0,1.0) ), u.x), u.y);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Depth Logic
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float sceneDepth = SampleSceneDepth(screenUV);
                float sceneDepthLinear = LinearEyeDepth(sceneDepth, _ZBufferParams);
                float surfaceDepth = LinearEyeDepth(input.positionCS.z, _ZBufferParams); // Approximate
                // Better: float surfaceDepth = input.screenPos.w; (LinearEyeDepth requires raw depth usually, but w is linear view depth)
                // Let's use LinearEyeDepth correct usage: 
                // SampleSceneDepth returns value in [0,1] (or reverse Z). LinearEyeDepth converts to view space units.
                // input.positionCS.w is the view space depth.
                
                float depthDifference = sceneDepthLinear - input.positionCS.w;
                
                // World Generated Logic: Use World Position XZ instead of mesh UV
                float2 worldUV = input.positionWS.xz;
                
                // Voronoi / Noise for water pattern
                float2 waveUV = worldUV * _WaveScale + _Time.y * _WaveSpeed * 0.1;
                
                // Use Voronoi-like pattern (Simple noise is okay, but let's make it more cellular)
                float noiseVal = simple_noise(waveUV);
                
                // Distort lines to look more like liquid
                float dist = sin(waveUV.y * 10.0 + _Time.y) * 0.05;
                noiseVal += dist;

                // Step noise to make "cells"
                float cellMask = step(0.4, noiseVal); // Adjusted threshold

                // Foam Logic
                // Foam at edges (depth diff small) OR where noise is high (optional)
                float foamFactor = 1.0 - saturate(depthDifference / _FoamThreshold);
                // Add noise to foam edge
                foamFactor += (noiseVal * 0.2); 
                foamFactor = smoothstep(0.5, 0.6, foamFactor);
                
                // Add "Wave Crests" - bright spots in the open water
                float waveCrest = smoothstep(0.8, 0.9, noiseVal);
                
                half4 waterColor = lerp(_DeepColor, _ShallowColor, foamFactor * 0.5 + cellMask * 0.1); 
                half4 finalColor = lerp(waterColor, _FoamColor, saturate(foamFactor + waveCrest));
                
                // Opacity
                finalColor.a = lerp(_DeepColor.a, 1.0, foamFactor); // More opaque at foam

                // Lighting (Specular only for water usually looks nice, or simple diffuse)
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half3 lightColor = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation;
                half NdotL = saturate(dot(input.normalWS, mainLight.direction));
                
                finalColor.rgb *= (lightColor * NdotL + SampleSH(input.normalWS));

                return finalColor;
            }
            ENDHLSL
        }
    }
}

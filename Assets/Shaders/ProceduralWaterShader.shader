Shader "Roystan/Toon/Water Tut URP"
{
    Properties
    {
        _DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)
        _DepthMaxDistance("Depth Maximum Distance", Float) = 1
        _FoamColor("Foam Color", Color) = (1,1,1,1)
        _SurfaceNoise("Surface Noise", 2D) = "white" {}
        _SurfaceNoiseScroll("Surface Noise Scroll Amount", Vector) = (0.03, 0.03, 0, 0)
        _SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0, 1)) = 0.777
        _SurfaceDistortion("Surface Distortion", 2D) = "white" {}
        _SurfaceDistortionAmount("Surface Distortion Amount", Range(0, 1)) = 0.27
        _FoamMaxDistance("Foam Maximum Distance", Float) = 0.4
        _FoamMinDistance("Foam Minimum Distance", Float) = 0.04
    }
    
    SubShader
    {
        Tags 
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "WaterPass"
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"
            
            #define SMOOTHSTEP_AA 0.01
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 noiseUV : TEXCOORD0;
                float2 distortUV : TEXCOORD1;
                float4 screenPosition : TEXCOORD2;
                float3 viewNormal : NORMAL;
                float3 viewVectorWS : TEXCOORD3;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _DepthGradientShallow;
                float4 _DepthGradientDeep;
                float4 _FoamColor;
                float _DepthMaxDistance;
                float _FoamMaxDistance;
                float _FoamMinDistance;
                float _SurfaceNoiseCutoff;
                float _SurfaceDistortionAmount;
                float2 _SurfaceNoiseScroll;
                float4 _SurfaceNoise_ST;
                float4 _SurfaceDistortion_ST;
            CBUFFER_END
            
            TEXTURE2D(_SurfaceNoise);
            SAMPLER(sampler_SurfaceNoise);
            
            TEXTURE2D(_SurfaceDistortion);
            SAMPLER(sampler_SurfaceDistortion);
            
            // Blends two colors using normal blending
            float4 AlphaBlend(float4 top, float4 bottom)
            {
                float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
                float alpha = top.a + bottom.a * (1 - top.a);
                
                return float4(color, alpha);
            }
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.screenPosition = ComputeScreenPos(OUT.positionCS);
                
                OUT.distortUV = TRANSFORM_TEX(IN.uv, _SurfaceDistortion);
                OUT.noiseUV = TRANSFORM_TEX(IN.uv, _SurfaceNoise);
                
                // Calculate view-space normal
                OUT.viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, IN.normalOS));
                
                // For depth calculation in URP
                OUT.viewVectorWS = GetWorldSpaceViewDir(TransformObjectToWorld(IN.positionOS.xyz));
                
                return OUT;
            }
            
            float4 frag(Varyings IN) : SV_Target
            {
                // Get screen position for depth sampling
                float2 screenUV = IN.screenPosition.xy / IN.screenPosition.w;
                
                // Sample scene depth
                #if UNITY_REVERSED_Z
                    float sceneDepth = SampleSceneDepth(screenUV);
                #else
                    float sceneDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(screenUV));
                #endif
                
                // Convert to linear depth
                float sceneLinerEyeDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);
                float surfaceLinearEyeDepth = IN.screenPosition.w;
                float depthDifference = sceneLinerEyeDepth - surfaceLinearEyeDepth;
                
                // Calculate water color based on depth
                float waterDepthDifference01 = saturate(depthDifference / _DepthMaxDistance);
                float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepthDifference01);
                
                // Sample scene normals
                float3 existingNormal = SampleSceneNormals(screenUV);
                
                // Calculate foam based on normal differences
                float3 normalDot = saturate(dot(existingNormal, IN.viewNormal));
                float foamDistance = lerp(_FoamMaxDistance, _FoamMinDistance, normalDot);
                float foamDepthDifference01 = saturate(depthDifference / foamDistance);
                
                float surfaceNoiseCutoff = foamDepthDifference01 * _SurfaceNoiseCutoff;
                
                // Sample and apply distortion
                float2 distortSample = (SAMPLE_TEXTURE2D(_SurfaceDistortion, sampler_SurfaceDistortion, IN.distortUV).xy * 2 - 1) * _SurfaceDistortionAmount;
                
                // Apply distortion and time scrolling to noise UV
                float2 noiseUV = float2(
                    (IN.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x) + distortSample.x,
                    (IN.noiseUV.y + _Time.y * _SurfaceNoiseScroll.y) + distortSample.y
                );
                
                float surfaceNoiseSample = SAMPLE_TEXTURE2D(_SurfaceNoise, sampler_SurfaceNoise, noiseUV).r;
                
                // Apply smoothstep for anti-aliased foam edges
                float surfaceNoise = smoothstep(surfaceNoiseCutoff - SMOOTHSTEP_AA, surfaceNoiseCutoff + SMOOTHSTEP_AA, surfaceNoiseSample);
                
                float4 surfaceNoiseColor = _FoamColor;
                surfaceNoiseColor.a *= surfaceNoise;
                
                // Combine foam and water color
                return AlphaBlend(surfaceNoiseColor, waterColor);
            }
            ENDHLSL
        }
    }
}
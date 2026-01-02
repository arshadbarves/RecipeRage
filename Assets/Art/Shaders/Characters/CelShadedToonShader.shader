// Renamed from BrawlStarsStyleShader to CelShadedToonShader 

Shader "Custom/CelShadedToonShader"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        
        [Space(10)]
        [Header(Toon Shading)]
        [Toggle(_USE_TOON_SHADING)] _UseToonShading("Enable Toon Shading", Float) = 1
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmoothness("Shadow Smoothness", Range(0, 0.5)) = 0.05
        _ShadowColor("Shadow Color", Color) = (0.7, 0.7, 0.8, 1)
        
        [Space(10)]
        [Header(Outline)]
        [Toggle(_USE_OUTLINE)] _UseOutline("Enable Outline", Float) = 1
        _OutlineWidth("Outline Width", Range(0, 5)) = 1.5
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineColorFar("Far Outline Color", Color) = (0.2, 0.2, 0.2, 1)
        _OutlineFarDistance("Outline Far Distance", Range(1, 10)) = 5
        
        [Space(10)]
        [Header(Rim Light)]
        [Toggle(_USE_RIM_LIGHT)] _UseRimLight("Enable Rim Light", Float) = 1
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(0, 10)) = 3
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.2
        
        [Space(10)]
        [Header(Color Grading)]
        [Toggle(_USE_COLOR_GRADING)] _UseColorGrading("Enable Color Grading", Float) = 1
        _Saturation("Saturation", Range(0, 2)) = 1.2
        _Brightness("Brightness", Range(0, 2)) = 1.1
        _Contrast("Contrast", Range(0, 2)) = 1.1
        
        [Space(10)]
        [Header(Specular)]
        [Toggle(_USE_SPECULAR)] _UseSpecular("Enable Specular", Float) = 1
        _SpecularColor("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularSmoothness("Specular Smoothness", Range(0, 1)) = 0.5
        _SpecularThreshold("Specular Threshold", Range(0, 1)) = 0.8
        
        [Space(10)]
        [Header(Emission)]
        [Toggle(_USE_EMISSION)] _UseEmission("Enable Emission", Float) = 0
        [HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 1)
        _EmissionMap("Emission Map", 2D) = "white" {}
        
        [Space(10)]
        [Header(Special Effects)]
        [Toggle(_USE_PULSE_EFFECT)] _UsePulseEffect("Enable Pulse Effect", Float) = 0
        _PulseColor("Pulse Color", Color) = (1, 0.5, 0, 1)
        _PulseSpeed("Pulse Speed", Range(0, 10)) = 2
        _PulseIntensity("Pulse Intensity", Range(0, 1)) = 0.5
        
        [Space(10)]
        [Header(Mobile Optimization)]
        [Toggle(_MOBILE_MODE)] _MobileMode("Mobile Optimization", Float) = 0
        [PowerSlider(3)] _QualityLevel("Quality Level", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        // Base Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // Feature toggles
            #pragma shader_feature_local _USE_TOON_SHADING
            #pragma shader_feature_local _USE_RIM_LIGHT
            #pragma shader_feature_local _USE_SPECULAR
            #pragma shader_feature_local _USE_COLOR_GRADING
            #pragma shader_feature_local _USE_EMISSION
            #pragma shader_feature_local _USE_PULSE_EFFECT
            #pragma shader_feature_local _MOBILE_MODE
            
            // Mobile optimizations
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            
            // Unity includes
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
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
            };

            // Texture samplers
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            // Material properties
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _ShadowThreshold;
                float _ShadowSmoothness;
                float4 _ShadowColor;
                float4 _RimColor;
                float _RimPower;
                float _RimThreshold;
                float4 _SpecularColor;
                float _SpecularSmoothness;
                float _SpecularThreshold;
                float _Saturation;
                float _Brightness;
                float _Contrast;
                float4 _EmissionColor;
                float _OutlineWidth;
                float4 _OutlineColor;
                float4 _OutlineColorFar;
                float _OutlineFarDistance;
                float _UsePulseEffect;
                float4 _PulseColor;
                float _PulseSpeed;
                float _PulseIntensity;
                float _QualityLevel;
            CBUFFER_END

            float3 CalculateRimLight(float3 normal, float3 viewDir)
            {
                #if defined(_USE_RIM_LIGHT)
                    float rimDot = 1 - saturate(dot(viewDir, normal));
                    float rimIntensity = smoothstep(_RimThreshold, _RimThreshold + 0.1, pow(rimDot, _RimPower));
                    return _RimColor.rgb * rimIntensity;
                #else
                    return float3(0, 0, 0);
                #endif
            }

            float3 CalculateSpecular(float3 normal, float3 lightDir, float3 viewDir)
            {
                #if defined(_USE_SPECULAR)
                    float3 halfDir = normalize(lightDir + viewDir);
                    float NdotH = dot(normal, halfDir);
                    
                    #if defined(_MOBILE_MODE)
                        // Simplified specular for mobile
                        float specular = pow(max(0, NdotH), _SpecularSmoothness * 64.0 * _QualityLevel);
                        return _SpecularColor.rgb * step(_SpecularThreshold, specular) * _QualityLevel;
                    #else
                        // Brawl Stars style specular - sharper and more defined
                        float specular = pow(max(0, NdotH), _SpecularSmoothness * 128.0);
                        return _SpecularColor.rgb * step(_SpecularThreshold, specular);
                    #endif
                #else
                    return float3(0, 0, 0);
                #endif
            }

            // Apply color grading (saturation, brightness, contrast)
            float3 ApplyColorGrading(float3 color)
            {
                #if defined(_USE_COLOR_GRADING)
                    // Saturation
                    float luminance = dot(color, float3(0.299, 0.587, 0.114));
                    float3 saturatedColor = lerp(float3(luminance, luminance, luminance), color, _Saturation);
                    
                    // Brightness
                    float3 brightColor = saturatedColor * _Brightness;
                    
                    // Contrast
                    float3 avgColor = float3(0.5, 0.5, 0.5);
                    float3 finalColor = lerp(avgColor, brightColor, _Contrast);
                    
                    return finalColor;
                #else
                    return color;
                #endif
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(positionWS);
                output.positionWS = positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Sample base texture
                float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                float4 finalColor = baseMap * _BaseColor;
                
                // Get lighting data
                Light mainLight = GetMainLight();
                float3 normalWS = normalize(input.normalWS);
                float3 lightDir = normalize(mainLight.direction);
                float3 viewDir = normalize(input.viewDirWS);
                
                // Calculate lighting intensity for use in multiple effects
                float NdotL = dot(normalWS, lightDir);
                float lightIntensity = NdotL * 0.5 + 0.5;
                
                // Calculate toon shading
                #if defined(_USE_TOON_SHADING)
                    float3 toonColor = float3(1,1,1);
                    
                    #if defined(_MOBILE_MODE)
                        // Simplified toon shading for mobile
                        float step1 = step(_ShadowThreshold, lightIntensity);
                        toonColor = lerp(_ShadowColor.rgb, float3(1,1,1), step1);
                    #else
                        // Brawl Stars style toon shading - sharper transition
                        float step1 = smoothstep(_ShadowThreshold - _ShadowSmoothness,
                                              _ShadowThreshold + _ShadowSmoothness,
                                              lightIntensity);
                        toonColor = lerp(_ShadowColor.rgb, float3(1,1,1), step1);
                    #endif
                    
                    finalColor.rgb *= toonColor;
                #endif
                
                // Add rim lighting
                finalColor.rgb += CalculateRimLight(normalWS, viewDir);
                
                // Add specular
                finalColor.rgb += CalculateSpecular(normalWS, lightDir, viewDir);
                
                // Apply color grading
                finalColor.rgb = ApplyColorGrading(finalColor.rgb);
                
                // Add emission
                #if defined(_USE_EMISSION)
                    float4 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv) * _EmissionColor;
                    finalColor.rgb += emission.rgb;
                #endif

                // Apply pulse effect (common in Brawl Stars for special abilities/effects)
                #if defined(_USE_PULSE_EFFECT)
                    float pulseFactor = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                    finalColor.rgb = lerp(finalColor.rgb, _PulseColor.rgb, pulseFactor * _PulseIntensity);
                #endif
                
                return finalColor;
            }
            ENDHLSL
        }

        // Outline Pass
        Pass
        {
            Name "Outline"
            Tags { }
            Cull Front

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma shader_feature_local _USE_OUTLINE
            #pragma shader_feature_local _MOBILE_MODE
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            float _OutlineWidth;
            float4 _OutlineColor;
            float4 _OutlineColorFar;
            float _OutlineFarDistance;
            float _QualityLevel;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                #if defined(_USE_OUTLINE)
                    float outlineWidth = _OutlineWidth;
                    
                    #if defined(_MOBILE_MODE)
                        outlineWidth *= _QualityLevel;
                    #endif
                    
                    // Brawl Stars style outlines - consistent width regardless of distance
                    float3 normalOS = normalize(input.normalOS);
                    float3 posOS = input.positionOS.xyz + normalOS * (outlineWidth * 0.001);
                    
                    float4 worldPos = mul(UNITY_MATRIX_M, float4(posOS, 1.0));
                    output.positionCS = mul(UNITY_MATRIX_VP, worldPos);
                #else
                    // No outline, just pass through the vertex
                    float4 worldPos = mul(UNITY_MATRIX_M, float4(input.positionOS.xyz, 1.0));
                    output.positionCS = mul(UNITY_MATRIX_VP, worldPos);
                #endif
                
                output.screenPos = ComputeScreenPos(output.positionCS);
                
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                #if defined(_USE_OUTLINE)
                    float depth = input.positionCS.z / input.positionCS.w;
                    float distanceFactor = saturate(depth / _OutlineFarDistance);
                    return lerp(_OutlineColor, _OutlineColorFar, distanceFactor);
                #else
                    // If outline is disabled, discard the fragment
                    discard;
                    return float4(0, 0, 0, 0);
                #endif
            }
            ENDHLSL
        }
        
        // Shadow casting pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            float4 _BaseMap_ST;
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformWorldToHClip(input.positionOS.xyz);
                return output;
            }

            float4 frag(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
        
        // Depth pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            float4 _BaseMap_ST;
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            float4 frag(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }
} 
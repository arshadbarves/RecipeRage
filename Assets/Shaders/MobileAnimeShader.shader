Shader "Custom/MobileAnimeShader"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        
        [Space(10)]
        [Header(State Effects)]
        [Toggle(_USE_STATE_EFFECTS)] _UseStateEffects("Enable State Effects", Float) = 1
        [Toggle] _IsHighlighted("Is Highlighted", Float) = 0
        _HighlightColor("Highlight Color", Color) = (1, 0.8, 0, 1)
        _HighlightIntensity("Highlight Intensity", Range(0, 1)) = 0.5
        _HighlightPulseSpeed("Highlight Pulse Speed", Range(0, 10)) = 2
        [Toggle] _IsDisabled("Is Disabled", Float) = 0
        _DisabledColor("Disabled Color", Color) = (0.5, 0.5, 0.5, 1)
        _DisabledSaturation("Disabled Saturation", Range(0, 1)) = 0.5
        
        [Space(10)]
        [Header(Advanced State Effects)]
        [Toggle] _IsProcessing("Is Processing", Float) = 0
        _ProcessingColor("Processing Color", Color) = (0, 0.7, 1, 1)
        _ProcessingPulseSpeed("Processing Pulse Speed", Range(0, 10)) = 3
        [Toggle] _IsSuccess("Is Success", Float) = 0
        _SuccessColor("Success Color", Color) = (0, 1, 0.2, 1)
        _SuccessDuration("Success Duration", Range(0, 5)) = 1
        [Toggle] _IsError("Is Error", Float) = 0
        _ErrorColor("Error Color", Color) = (1, 0, 0, 1)
        _ErrorPulseSpeed("Error Pulse Speed", Range(0, 10)) = 5
        
        [Space(10)]
        [Header(Toon Shading)]
        [Toggle(_USE_TOON_SHADING)] _UseToonShading("Enable Toon Shading", Float) = 1
        _ShadowSteps("Shadow Steps", Range(1, 3)) = 2
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSmoothness("Shadow Smoothness", Range(0, 0.5)) = 0.05
        _ShadowColor("Shadow Color", Color) = (0.7, 0.7, 0.7, 1)
        _ShadowColorSecond("Second Shadow Color", Color) = (0.5, 0.5, 0.5, 1)
        
        [Space(10)]
        [Header(Rim Light)]
        [Toggle(_USE_RIM_LIGHT)] _UseRimLight("Enable Rim Light", Float) = 1
        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower("Rim Power", Range(0, 10)) = 5
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.5
        _RimSmoothness("Rim Smoothness", Range(0, 0.5)) = 0.1
        
        [Space(10)]
        [Header(Outline)]
        [Toggle(_USE_OUTLINE)] _UseOutline("Enable Outline", Float) = 1
        _OutlineWidth("Outline Width", Range(0, 5)) = 1
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineColorFar("Far Outline Color", Color) = (0.2, 0.2, 0.2, 1)
        _OutlineFarDistance("Outline Far Distance", Range(1, 10)) = 5
        
        [Space(10)]
        [Header(Specular)]
        [Toggle(_USE_SPECULAR)] _UseSpecular("Enable Specular", Float) = 1
        _SpecularColor("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularSmoothness("Specular Smoothness", Range(0, 1)) = 0.5
        _SpecularThreshold("Specular Threshold", Range(0, 1)) = 0.8
        _SpecularSize("Specular Size", Range(0, 1)) = 0.1
        [Toggle] _AnimeSpecular("Anime Specular", Float) = 1
        
        [Space(10)]
        [Header(MatCap)]
        [Toggle(_USE_MATCAP)] _UseMatCap("Enable MatCap", Float) = 0
        [NoScaleOffset] _MatCap("MatCap Texture", 2D) = "black" {}
        _MatCapStrength("MatCap Strength", Range(0, 1)) = 1
        
        [Space(10)]
        [Header(Color Shifts)]
        [Toggle(_USE_COLOR_SHIFT)] _UseColorShift("Enable Color Shift", Float) = 1
        _BrightColor("Bright Color Shift", Color) = (1, 1, 1, 1)
        _DarkColor("Dark Color Shift", Color) = (0.9, 0.9, 1, 1)
        _ColorShiftStrength("Color Shift Strength", Range(0, 1)) = 0.3
        
        [Space(10)]
        [Header(Emission)]
        [Toggle(_USE_EMISSION)] _UseEmission("Enable Emission", Float) = 0
        [HDR] _EmissionColor("Emission Color", Color) = (0, 0, 0, 1)
        _EmissionMap("Emission Map", 2D) = "white" {}
        [Toggle] _PulsingEmission("Pulsing Emission", Float) = 0
        _PulseSpeed("Pulse Speed", Range(0, 10)) = 1
        
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
            #pragma shader_feature_local _USE_STATE_EFFECTS
            #pragma shader_feature_local _USE_TOON_SHADING
            #pragma shader_feature_local _USE_RIM_LIGHT
            #pragma shader_feature_local _USE_SPECULAR
            #pragma shader_feature_local _USE_MATCAP
            #pragma shader_feature_local _USE_COLOR_SHIFT
            #pragma shader_feature_local _USE_EMISSION
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
                float4 screenPos : TEXCOORD4;
            };

            // Texture samplers
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_MatCap);
            SAMPLER(sampler_MatCap);

            // Material properties
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _ShadowSteps;
                float _ShadowThreshold;
                float _ShadowSmoothness;
                float4 _ShadowColor;
                float4 _ShadowColorSecond;
                float4 _RimColor;
                float _RimPower;
                float _RimThreshold;
                float _RimSmoothness;
                float4 _SpecularColor;
                float _SpecularSmoothness;
                float _SpecularThreshold;
                float _SpecularSize;
                float _AnimeSpecular;
                float _MatCapStrength;
                float4 _BrightColor;
                float4 _DarkColor;
                float _ColorShiftStrength;
                float4 _EmissionColor;
                float _PulsingEmission;
                float _PulseSpeed;
                float _IsHighlighted;
                float4 _HighlightColor;
                float _HighlightIntensity;
                float _HighlightPulseSpeed;
                float _IsDisabled;
                float4 _DisabledColor;
                float _DisabledSaturation;
                float _IsProcessing;
                float4 _ProcessingColor;
                float _ProcessingPulseSpeed;
                float _IsSuccess;
                float4 _SuccessColor;
                float _SuccessDuration;
                float _IsError;
                float4 _ErrorColor;
                float _ErrorPulseSpeed;
                float _QualityLevel;
            CBUFFER_END

            float3 CalculateRimLight(float3 normal, float3 viewDir)
            {
                #if defined(_USE_RIM_LIGHT) && !defined(_MOBILE_MODE)
                    float rimDot = 1 - saturate(dot(viewDir, normal));
                    float rimIntensity = smoothstep(_RimThreshold - _RimSmoothness, 
                                                 _RimThreshold + _RimSmoothness, 
                                                 pow(rimDot, _RimPower));
                    return _RimColor.rgb * rimIntensity;
                #elif defined(_USE_RIM_LIGHT) && defined(_MOBILE_MODE)
                    // Simplified rim light for mobile
                    float rimDot = 1 - saturate(dot(viewDir, normal));
                    float rimIntensity = step(_RimThreshold, pow(rimDot, _RimPower));
                    return _RimColor.rgb * rimIntensity * _QualityLevel;
                #else
                    return float3(0, 0, 0);
                #endif
            }

            float3 CalculateAnimeSpecular(float3 normal, float3 lightDir, float3 viewDir)
            {
                #if defined(_USE_SPECULAR)
                    float3 halfDir = normalize(lightDir + viewDir);
                    float NdotH = dot(normal, halfDir);
                    
                    #if defined(_MOBILE_MODE)
                        // Simplified specular for mobile
                        float specular = pow(max(0, NdotH), _SpecularSmoothness * 64.0 * _QualityLevel);
                        return _SpecularColor.rgb * step(_SpecularThreshold, specular) * _QualityLevel;
                    #else
                        if (_AnimeSpecular)
                        {
                            // Anime-style circular specular
                            float specularCircle = 1.0 - smoothstep(1.0 - _SpecularSize, 1.0, length(float2(NdotH, NdotH)));
                            return _SpecularColor.rgb * step(_SpecularThreshold, specularCircle);
                        }
                        else
                        {
                            // Traditional specular
                            float specular = pow(max(0, NdotH), _SpecularSmoothness * 128.0);
                            return _SpecularColor.rgb * step(_SpecularThreshold, specular);
                        }
                    #endif
                #else
                    return float3(0, 0, 0);
                #endif
            }

            float3 CalculateMatCap(float3 normalWS, float3 viewDirWS)
            {
                #if defined(_USE_MATCAP)
                    float3 normalVS = mul((float3x3)UNITY_MATRIX_V, normalWS);
                    float2 matcapUV = normalVS.xy * 0.5 + 0.5;
                    float3 matcap = SAMPLE_TEXTURE2D(_MatCap, sampler_MatCap, matcapUV).rgb;
                    return matcap * _MatCapStrength;
                #else
                    return float3(0, 0, 0);
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
                output.screenPos = ComputeScreenPos(output.positionCS);
                
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
                        if (_ShadowSteps >= 2)
                        {
                            float step1 = smoothstep(_ShadowThreshold - _ShadowSmoothness,
                                                  _ShadowThreshold + _ShadowSmoothness,
                                                  lightIntensity);
                            float step2 = smoothstep((_ShadowThreshold * 0.5) - _ShadowSmoothness,
                                                  (_ShadowThreshold * 0.5) + _ShadowSmoothness,
                                                  lightIntensity);
                            
                            toonColor = lerp(_ShadowColorSecond.rgb, _ShadowColor.rgb, step2);
                            toonColor = lerp(toonColor, float3(1,1,1), step1);
                        }
                        else
                        {
                            float step1 = smoothstep(_ShadowThreshold - _ShadowSmoothness,
                                                  _ShadowThreshold + _ShadowSmoothness,
                                                  lightIntensity);
                            toonColor = lerp(_ShadowColor.rgb, float3(1,1,1), step1);
                        }
                    #endif
                    
                    finalColor.rgb *= toonColor;
                #endif
                
                // Add rim lighting
                finalColor.rgb += CalculateRimLight(normalWS, viewDir);
                
                // Add specular
                finalColor.rgb += CalculateAnimeSpecular(normalWS, lightDir, viewDir);
                
                // Add MatCap
                finalColor.rgb += CalculateMatCap(normalWS, viewDir);
                
                // Apply color shifts
                #if defined(_USE_COLOR_SHIFT)
                    float3 colorShift = lerp(_DarkColor.rgb, _BrightColor.rgb, lightIntensity);
                    finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * colorShift, _ColorShiftStrength);
                #endif
                
                // Add emission
                #if defined(_USE_EMISSION)
                    float emissionPulse = 1;
                    if (_PulsingEmission)
                    {
                        emissionPulse = (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5) + 0.5;
                    }
                    float4 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv) * _EmissionColor * emissionPulse;
                    finalColor.rgb += emission.rgb;
                #endif

                // Apply state effects
                #if defined(_USE_STATE_EFFECTS)
                    // Apply highlight effect
                    if (_IsHighlighted)
                    {
                        float highlightPulse = (sin(_Time.y * _HighlightPulseSpeed) * 0.5 + 0.5);
                        float3 highlightColor = _HighlightColor.rgb * _HighlightIntensity * highlightPulse;
                        finalColor.rgb = lerp(finalColor.rgb, highlightColor + finalColor.rgb, _HighlightIntensity * highlightPulse);
                    }
                    
                    // Apply processing effect
                    if (_IsProcessing)
                    {
                        float processingPulse = (sin(_Time.y * _ProcessingPulseSpeed) * 0.5 + 0.5);
                        float3 processingColor = _ProcessingColor.rgb * processingPulse;
                        finalColor.rgb = lerp(finalColor.rgb, processingColor + finalColor.rgb, processingPulse * 0.7);
                    }
                    
                    // Apply success effect
                    if (_IsSuccess)
                    {
                        float successFade = saturate(1.0 - (_Time.y / _SuccessDuration));
                        float3 successColor = _SuccessColor.rgb * successFade;
                        finalColor.rgb = lerp(finalColor.rgb, successColor + finalColor.rgb, successFade * 0.8);
                    }
                    
                    // Apply error effect
                    if (_IsError)
                    {
                        float errorPulse = (sin(_Time.y * _ErrorPulseSpeed) * 0.5 + 0.5);
                        float3 errorColor = _ErrorColor.rgb * errorPulse;
                        finalColor.rgb = lerp(finalColor.rgb, errorColor + finalColor.rgb, errorPulse * 0.9);
                    }

                    // Apply disabled effect (applied last)
                    if (_IsDisabled)
                    {
                        // Convert to grayscale while preserving luminance
                        float luminance = dot(finalColor.rgb, float3(0.299, 0.587, 0.114));
                        float3 grayscale = float3(luminance, luminance, luminance);
                        finalColor.rgb = lerp(finalColor.rgb, grayscale * _DisabledColor.rgb, 1 - _DisabledSaturation);
                    }
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
            
            // Skip outline pass entirely on mobile if quality is low
            #pragma multi_compile_local __ _SKIP_OUTLINE_ON_MOBILE
            
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
                    // Skip outline on mobile if quality is below threshold or flag is set
                    #if defined(_MOBILE_MODE) && (defined(_SKIP_OUTLINE_ON_MOBILE) || _QualityLevel < 0.3)
                        // Just pass through the vertex without outline
                        float4 worldPos = mul(UNITY_MATRIX_M, float4(input.positionOS.xyz, 1.0));
                        output.positionCS = mul(UNITY_MATRIX_VP, worldPos);
                    #else
                        float outlineWidth = _OutlineWidth;
                        
                        #if defined(_MOBILE_MODE)
                            outlineWidth *= _QualityLevel;
                        #endif
                        
                        float3 normalOS = normalize(input.normalOS);
                        float3 posOS = input.positionOS.xyz + normalOS * (outlineWidth * 0.001);
                        
                        float4 worldPos = mul(UNITY_MATRIX_M, float4(posOS, 1.0));
                        output.positionCS = mul(UNITY_MATRIX_VP, worldPos);
                    #endif
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
                    // Skip outline on mobile if quality is below threshold or flag is set
                    #if defined(_MOBILE_MODE) && (defined(_SKIP_OUTLINE_ON_MOBILE) || _QualityLevel < 0.3)
                        discard;
                        return float4(0, 0, 0, 0);
                    #else
                        float depth = input.positionCS.z / input.positionCS.w;
                        float distanceFactor = saturate(depth / _OutlineFarDistance);
                        return lerp(_OutlineColor, _OutlineColorFar, distanceFactor);
                    #endif
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
                output.positionCS = GetShadowPositionHClip(input.positionOS, input.normalOS);
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
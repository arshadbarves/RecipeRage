Shader "Custom/SoftFakeLitMobileURP" // Renamed slightly to reflect added lighting concepts
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color Tint", Color) = (1,1,1,1)

        [Header(Texture Coordinates)]
        [Toggle(_USE_PROCEDURAL_UVS)] _UseProceduralUVs("Use Procedural UVs", Float) = 0
        _UVScale("UV Scale", Vector) = (1, 1, 0, 0)
        _UVOffset("UV Offset", Vector) = (0, 0, 0, 0)
        _UVRotation("UV Rotation (Degrees)", Range(0, 360)) = 0
        [KeywordEnum(UV1, UV2, Triplanar, Spherical)] _UVMapping("UV Mapping Mode", Float) = 0

        _TopColor("Top Color (Fake Light)", Color) = (1.0, 1.0, 1.0, 1.0) // Color for surfaces facing the fake light
        _BottomColor("Bottom Color (Ambient)", Color) = (0.5, 0.5, 0.5, 1.0) // Color for surfaces facing away
        _FakeLightDirection("Fake Light Direction (World)", Vector) = (50, 330, 0, 0) // Custom light direction

        [Header(Lighting Controls)]
        _LightIntensity("Light Intensity", Range(0.0, 4.0)) = 1.2 // Controls overall brightness
        _AmbientShadowStrength("Ambient Shadow Strength", Range(0.0, 1.0)) = 0.7 // Controls shadow darkness
        _AmbientOcclusionScale("Ambient Occlusion Scale", Range(0.0, 1.0)) = 0.6 // Controls AO effect strength
        _ShadowSharpness("Shadow Sharpness", Range(0.5, 10.0)) = 3.5 // Controls the transition between light and shadow
        _LightFalloff("Light Falloff", Range(0.0, 1.0)) = 0.7 // Controls how quickly light falls off
        _ShadowEdgeBlend("Shadow Edge Blend", Range(0.01, 0.5)) = 0.1 // Controls the edge blending of shadows

        [Header(Surface Properties)]
        _SpecColor("Specular Color", Color) = (1,1,1,1) // Specular tint (mainly for non-metals, multiplied by ~0.04)
        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0 // 0 = Non-metal, 1 = Metal
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5 // Controls specular highlight sharpness
        _EdgeSoftening("Edge Softening", Range(0.0, 1.0)) = 0.2 // Softens hard edges and seams

        [Header(Rim Effect)]
        _RimColor("Rim Color", Color) = (1,1,1,0) // Set Alpha to 0 to disable Rim
        _RimPower("Rim Power", Range(0.1, 10.0)) = 3.0 // Controls the sharpness/width of the rim
    }
    SubShader
    {
        // URP specific tags
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            Name "ForwardLit" // Standard URP Forward pass name
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Enable GPU instancing
            #pragma multi_compile_instancing

            // Custom keywords for UV mapping modes
            #pragma shader_feature_local _USE_PROCEDURAL_UVS
            #pragma multi_compile_local _UVMAPPING_UV1 _UVMAPPING_UV2 _UVMAPPING_TRIPLANAR _UVMAPPING_SPHERICAL

            // URP Core Includes - provides necessary functions and variables
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // Include Lighting for PBR helper functions/constants like unity_ColorSpaceDielectricSpec
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            // Include PBR functions for potential future use or reference, though we use a simplified model here
            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/PhysicalBasedLighting.hlsl"

            // Define CBuffer for material properties
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST; // For texture tiling/offset
                half4 _BaseColor;
                half4 _TopColor;
                half4 _BottomColor;
                float4 _FakeLightDirection; // Using float4 for CBuffer alignment, only xyz used
                half _LightIntensity;
                half _AmbientShadowStrength;
                half _AmbientOcclusionScale;
                half _ShadowSharpness;
                half _LightFalloff;
                half _ShadowEdgeBlend;
                half4 _SpecColor;
                half _Metallic;
                half _Smoothness;
                half _EdgeSoftening;
                half4 _RimColor;
                float _RimPower; // Using float for Range precision
                // Texture coordinate parameters
                float4 _UVScale;
                float4 _UVOffset;
                float _UVRotation;
            CBUFFER_END

            // Declare texture and sampler
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            // Input structure for the vertex shader
            struct Attributes
            {
                float4 positionOS   : POSITION;     // Object space position
                float2 uv           : TEXCOORD0;    // Base UV coordinates (UV1)
                float2 uv2          : TEXCOORD1;    // Secondary UV coordinates (UV2)
                float3 normalOS     : NORMAL;       // Object space normal
                UNITY_VERTEX_INPUT_INSTANCE_ID // For GPU Instancing
            };

            // Output structure for the vertex shader (input to fragment shader)
            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;  // Homogeneous clip space position
                float2 uv           : TEXCOORD0;    // UV coordinates
                float3 positionWS    : TEXCOORD3;    // World space position (for procedural UVs)
                half3 normalWS     : TEXCOORD1;    // World space normal
                half3 viewDirWS    : TEXCOORD2;    // World space view direction
                UNITY_VERTEX_INPUT_INSTANCE_ID // For GPU Instancing
                UNITY_VERTEX_OUTPUT_STEREO // For Stereo Rendering
            };

            // Vertex Shader (Mostly unchanged)
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                half3 positionWS = posInputs.positionWS;
                output.viewDirWS = normalize(_WorldSpaceCameraPos.xyz - positionWS);

                output.positionHCS = posInputs.positionCS;
                // Store world position for procedural mapping
                output.positionWS = positionWS;

                // Process UVs based on selected mapping mode
                #if defined(_USE_PROCEDURAL_UVS)
                    // Apply custom UV transformations
                    float2 customUV = input.uv;

                    // Apply scale
                    customUV *= _UVScale.xy;

                    // Apply rotation
                    float sinX = sin(radians(_UVRotation));
                    float cosX = cos(radians(_UVRotation));
                    float2x2 rotationMatrix = float2x2(cosX, -sinX, sinX, cosX);
                    customUV = mul(customUV - 0.5, rotationMatrix) + 0.5;

                    // Apply offset
                    customUV += _UVOffset.xy;

                    output.uv = customUV;
                #else
                    // Standard UV transformation
                    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                #endif
                output.normalWS = normalInputs.normalWS;

                return output;
            }

            // Fragment Shader
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Calculate texture coordinates based on mapping mode
                float2 finalUV = input.uv;

                #if defined(_UVMAPPING_UV2)
                    // Use secondary UV set
                    finalUV = input.uv2;
                #elif defined(_UVMAPPING_TRIPLANAR)
                    // Triplanar mapping
                    half3 blendWeights = abs(normalize(input.normalWS));
                    blendWeights /= (blendWeights.x + blendWeights.y + blendWeights.z);

                    // Sample texture from three directions
                    float2 uvX = input.positionWS.zy * _UVScale.xy + _UVOffset.xy;
                    float2 uvY = input.positionWS.xz * _UVScale.xy + _UVOffset.xy;
                    float2 uvZ = input.positionWS.xy * _UVScale.xy + _UVOffset.xy;

                    half4 colorX = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvX);
                    half4 colorY = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvY);
                    half4 colorZ = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvZ);

                    // Blend samples based on normal
                    half4 baseTexColor = colorX * blendWeights.x + colorY * blendWeights.y + colorZ * blendWeights.z;
                    half4 albedo = baseTexColor * _BaseColor;
                #elif defined(_UVMAPPING_SPHERICAL)
                    // Spherical mapping
                    float3 viewVec = normalize(input.positionWS - _WorldSpaceCameraPos);
                    float3 reflVec = reflect(viewVec, normalize(input.normalWS));

                    // Convert to spherical coordinates
                    float theta = atan2(reflVec.z, reflVec.x);
                    float phi = acos(reflVec.y);

                    // Map to UV space
                    float2 sphericalUV = float2(theta / (2.0 * 3.14159) + 0.5, phi / 3.14159);
                    sphericalUV = sphericalUV * _UVScale.xy + _UVOffset.xy;

                    half4 baseTexColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, sphericalUV);
                    half4 albedo = baseTexColor * _BaseColor;
                #else
                    // Standard UV mapping (UV1)
                    half4 baseTexColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, finalUV);
                    half4 albedo = baseTexColor * _BaseColor;
                #endif

                // Normalize interpolated vectors
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(input.viewDirWS);
                half3 fakeLightDirWS = normalize(_FakeLightDirection.xyz);

                // Calculate view-dependent edge factor to soften hard edges
                half edgeFactor = saturate(abs(dot(normalWS, viewDirWS)));
                // Apply edge softening - higher values of _EdgeSoftening will smooth out edges more
                edgeFactor = lerp(edgeFactor, 1.0, _EdgeSoftening);

                // --- Fake Diffuse Lighting Intensity ---
                half NdotL = dot(normalWS, fakeLightDirWS);

                // Apply shadow sharpness to create a sharper transition between light and shadow
                // Higher _ShadowSharpness values create a more defined edge
                half adjustedNdotL = pow(max(0, NdotL), _ShadowSharpness);

                // Apply light falloff to control how quickly light diminishes
                half lightFalloffFactor = lerp(0.5, 1.0, _LightFalloff);

                // Remap to [0, 1] with adjustable falloff
                half lightIntensity = saturate(adjustedNdotL * lightFalloffFactor + (1.0 - lightFalloffFactor));

                // Apply light intensity control
                lightIntensity = saturate(lightIntensity * _LightIntensity);

                // Calculate ambient occlusion effect (improved)
                // This creates darker areas in crevices and corners with less bleeding
                half ambientOcclusion = 1.0 - pow(1.0 - NdotL * 0.5 - 0.5, 2) * _AmbientOcclusionScale;

                // Create a sharper shadow mask to prevent light bleeding
                // Use the _ShadowEdgeBlend parameter to control the edge transition
                half shadowMask = smoothstep(0.0, _ShadowEdgeBlend, lightIntensity);

                // Apply ambient shadow strength with improved masking
                // Create more defined shadows with stronger contrast
                half shadowStrengthAdjusted = _AmbientShadowStrength * 1.2; // Boost shadow strength slightly
                half shadowFactor = lerp(ambientOcclusion * (1.0 - shadowStrengthAdjusted),
                                        1.0,
                                        pow(shadowMask, 1.2) * (1.0 - shadowStrengthAdjusted));

                // Base lighting color based on direction with shadow applied
                // Use the shadow mask to create a cleaner separation between lit and shadowed areas
                half3 directionalLightColor = lerp(_BottomColor.rgb * shadowFactor,
                                                 _TopColor.rgb * _LightIntensity,
                                                 shadowMask * lightIntensity);

                // --- Surface Properties based on Metallic ---
                // Non-metals have ~4% white specular reflectance (F0). Metals use albedo.
                // We multiply the dielectric spec by _SpecColor tint.
                half3 dielectricSpec = kDielectricSpec.rgb * _SpecColor.rgb; // kDielectricSpec (~0.04) from Lighting.hlsl
                half3 surfaceSpecColor = lerp(dielectricSpec, albedo.rgb, _Metallic);
                // Diffuse color is reduced for metals
                half3 surfaceDiffuseColor = albedo.rgb * (1.0h - _Metallic);

                // --- Simplified Blinn-Phong Specular ---
                half3 halfDir = normalize(fakeLightDirWS + viewDirWS);
                half NdotH = saturate(dot(normalWS, halfDir));
                // Map smoothness [0,1] to Blinn-Phong power [~2, ~1024] (adjust exponent as needed for visual style)
                // Using exp2 is often efficient. The '+1' ensures minimum power > 1.
                float phongPower = exp2(_Smoothness * 10.0h + 1.0h);
                half specularTerm = pow(NdotH, phongPower);

                // --- Combine Lighting Components ---
                // Diffuse component: Modulated by surface diffuse color and fake directional light
                half3 diffuse = surfaceDiffuseColor * directionalLightColor;

                // Specular component: Modulated by surface specular color, term, and incoming fake light color (_TopColor)
                // Only apply specular where the fake light is hitting (using lightIntensity)
                half3 specular = surfaceSpecColor * specularTerm * _TopColor.rgb * lightIntensity;

                // --- Rim Lighting ---
                half NdotV = dot(normalWS, viewDirWS);
                half rimFactor = pow(1.0h - saturate(NdotV), _RimPower);
                half3 rimLight = _RimColor.rgb * rimFactor * _RimColor.a;

                // --- Final Combination ---
                // Combine diffuse, specular, and rim with ambient shadows
                half3 finalColor = diffuse + specular + rimLight;

                // Apply ambient shadow to the final color with improved masking to prevent bleeding
                // Use the shadow factor but ensure lit areas stay bright while shadowed areas are properly darkened
                // Add more contrast between lit and shadowed areas
                half contrastBoost = 1.15; // Slightly boost contrast
                finalColor = pow(finalColor, 1.0/contrastBoost); // Apply gamma adjustment for contrast
                finalColor *= lerp(shadowFactor, 1.0, pow(shadowMask, 1.3)); // Sharper shadow transition

                // Apply edge softening to reduce visible seams and gaps
                // This helps blend harsh edges that might appear in the model
                finalColor = lerp(finalColor, finalColor * edgeFactor, 1.0 - _EdgeSoftening);

                finalColor = pow(finalColor, contrastBoost); // Restore gamma with contrast

                // Output the final color with the original alpha
                return half4(finalColor, albedo.a);
            }
            ENDHLSL
        }

        // Optional Shadow Caster Pass (Uncomment if needed)
        // Pass { ... }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}

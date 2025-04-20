Shader "Custom/StylizedUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        
        [Space(10)]
        [Header(Ambient Lighting)]
        _AmbientColor ("Ambient Color", Color) = (0.5,0.5,0.5,1)
        _AmbientIntensity ("Ambient Intensity", Range(0,1)) = 0.5
        
        [Space(10)]
        [Header(Fake Directional Light)]
        _LightDirection ("Light Direction", Vector) = (0.5, 0.5, 0, 0)
        _LightColor ("Light Color", Color) = (1,1,1,1)
        _LightIntensity ("Light Intensity", Range(0,2)) = 1.0
        _ShadowSoftness ("Shadow Softness", Range(0,1)) = 0.5
        
        [Space(10)]
        [Header(Specular)]
        _SpecularColor ("Specular Color", Color) = (1,1,1,1)
        _Glossiness ("Glossiness", Range(1,256)) = 32
        _SpecularIntensity ("Specular Intensity", Range(0,2)) = 1.0
        
        [Space(10)]
        [Header(Material Properties)]
        _Metallic ("Metallic", Range(0,1)) = 0
        _Roughness ("Roughness", Range(0,1)) = 0.5
        
        [Space(10)]
        [Header(Rim Effect)]
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0,1)) = 0.5
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        LOD 100

        Pass
        {
            Name "StylizedUnlit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            // For mobile optimization
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            
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
                float fogFactor : TEXCOORD3;
                float3 positionWS : TEXCOORD4;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _AmbientColor;
                half _AmbientIntensity;
                half4 _LightDirection;
                half4 _LightColor;
                half _LightIntensity;
                half _ShadowSoftness;
                half4 _SpecularColor;
                half _Glossiness;
                half _SpecularIntensity;
                half _Metallic;
                half _Roughness;
                half4 _RimColor;
                half _RimPower;
                half _RimIntensity;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                // Transform position from object to clip space
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                
                // Pass UV coordinates
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                // Transform normal from object to world space
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInput.normalWS;
                
                // Calculate view direction in world space
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                
                // Calculate fog factor
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Sample the texture
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Apply base color
                half4 color = texColor * _Color;
                
                // Normalize vectors
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                float3 lightDirWS = normalize(float3(_LightDirection.x, _LightDirection.y, _LightDirection.z));
                
                // Calculate ambient lighting
                half3 ambient = _AmbientColor.rgb * _AmbientIntensity;
                
                // Calculate fake directional lighting
                half NdotL = dot(normalWS, lightDirWS);
                half lightFactor = smoothstep(0.0, _ShadowSoftness, NdotL);
                half3 directionalLight = _LightColor.rgb * lightFactor * _LightIntensity;
                
                // Calculate specular reflection
                float3 halfVector = normalize(lightDirWS + viewDirWS);
                float NdotH = saturate(dot(normalWS, halfVector));
                
                // Apply roughness to specular
                float roughnessFactor = max(0.001, _Roughness); // Avoid division by zero
                float glossiness = _Glossiness * (1.0 - roughnessFactor);
                
                // Calculate specular term with roughness
                float specularTerm = pow(NdotH, glossiness) * lightFactor;
                
                // Apply metallic effect (metallic surfaces use the diffuse color for specular)
                half3 specularColor = lerp(_SpecularColor.rgb, color.rgb, _Metallic);
                half3 specular = specularColor * specularTerm * _SpecularIntensity;
                
                // Calculate rim lighting (fake fresnel for edge highlighting)
                half rim = 1.0 - saturate(dot(viewDirWS, normalWS));
                half3 rimLighting = _RimColor.rgb * pow(rim, _RimPower) * _RimIntensity;
                
                // Combine lighting components
                half3 finalColor = color.rgb * (ambient + directionalLight) + specular + rimLighting;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, color.a);
            }
            ENDHLSL
        }
        
        // Shadow casting pass
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            // For mobile optimization
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            
            ENDHLSL
        }
        
        // Depth pass
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            
            // For mobile optimization
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Lit"
    CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
}

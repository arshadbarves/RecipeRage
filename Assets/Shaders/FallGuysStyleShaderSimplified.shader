Shader "Custom/FallGuysStyleShaderSimplified"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _AmbientColor ("Ambient Color", Color) = (0.5,0.5,0.5,1)
        _AmbientIntensity ("Ambient Intensity", Range(0,1)) = 0.5
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
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
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _AmbientColor;
                half _AmbientIntensity;
                half4 _RimColor;
                half _RimPower;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                // Transform position from object to clip space
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                
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
                
                // Calculate ambient lighting
                half3 ambient = _AmbientColor.rgb * _AmbientIntensity;
                
                // Calculate rim lighting (fake fresnel for edge highlighting)
                half rim = 1.0 - saturate(dot(viewDirWS, normalWS));
                half3 rimLighting = _RimColor.rgb * pow(rim, _RimPower);
                
                // Combine lighting components
                half3 finalColor = color.rgb * ambient + rimLighting;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, color.a);
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Unlit"
}

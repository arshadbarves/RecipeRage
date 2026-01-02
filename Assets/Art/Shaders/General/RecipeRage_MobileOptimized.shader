Shader "Custom/RecipeRage_MobileOptimized"
{
    Properties
    {
        [Header(Base Settings)]
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        [Header(Fake Lighting)]
        // We manually set the light direction here. 
        // (0.5, 1, -0.5) is a standard "Top-Right" sun angle.
        _FakeLightDir ("Light Direction", Vector) = (0.5, 1, -0.5, 0)
        
        [Header(Clay Look)]
        _ShadowColor ("Shadow Tint", Color) = (0.85, 0.65, 0.65, 1)
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.5
        _ShadowSoftness ("Shadow Softness", Range(0.01, 0.5)) = 0.15

        [Header(Rim Light)]
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0
    }

    SubShader
    {
        // "Unlit" means Unity won't calculate lights for this object. Super fast.
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // We DO NOT include Lighting.hlsl to save performance
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
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD0;
                float3 viewDirWS : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _ShadowColor;
                float4 _FakeLightDir; // Our custom light vector
                float _ShadowThreshold;
                float _ShadowSoftness;
                float4 _RimColor;
                float _RimPower;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Standard Coordinate Calculations
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                
                // We still need normals for the "Fake" lighting to work
                // FIXED: Use TransformObjectToWorldNormal directly
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                // FIXED: Use positionWS instead of positionWorld
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                
                // 1. FAKE LIGHT CALCULATION
                // Instead of asking Unity for the sun, we use our variable
                float3 lightDir = normalize(_FakeLightDir.xyz);

                // 2. HALF-LAMBERT (The Soft Look)
                float NdotL = dot(normalWS, lightDir);
                float halfLambert = NdotL * 0.5 + 0.5;

                // 3. COLOR RAMP (The Toon Logic)
                float ramp = smoothstep(_ShadowThreshold - _ShadowSoftness, _ShadowThreshold + _ShadowSoftness, halfLambert);
                
                // 4. COMBINE
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                float3 finalColor = lerp(_ShadowColor.rgb * texColor.rgb, texColor.rgb, ramp);

                // 5. RIM LIGHT (Cheap math for style)
                float NdotV = 1.0 - saturate(dot(normalWS, viewDirWS));
                float rim = pow(NdotV, _RimPower) * ramp; // Multiply by ramp to hide rim in shadows
                finalColor += _RimColor.rgb * rim;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}

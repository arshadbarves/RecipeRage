Shader "Roystan/Toon/WaterURP"
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
        Tags { "Queue" = "Transparent" "RenderType"="Transparent" "RenderPipeline" = "UniversalRenderPipeline" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 positionOS : POSITION;
                float4 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 noiseUV : TEXCOORD0;
                float2 distortUV : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float3 normalWS : NORMAL;
            };

            // Properties
            sampler2D _SurfaceNoise;
            float4 _SurfaceNoise_ST;
            sampler2D _SurfaceDistortion;
            float4 _SurfaceDistortion_ST;

            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;
            float4 _FoamColor;
            float _DepthMaxDistance;
            float _FoamMaxDistance;
            float _FoamMinDistance;
            float _SurfaceNoiseCutoff;
            float _SurfaceDistortionAmount;
            float2 _SurfaceNoiseScroll;

            // Camera textures (URP requires these features enabled in Renderer)
            TEXTURE2D_X(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            v2f vert(appdata v)
            {
                v2f o;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS);

                o.positionCS = vertexInput.positionCS;
                o.screenPos = ComputeScreenPos(o.positionCS);
                o.distortUV = TRANSFORM_TEX(v.uv, _SurfaceDistortion);
                o.noiseUV = TRANSFORM_TEX(v.uv, _SurfaceNoise);
                o.normalWS = normalInput.normalWS;

                return o;
            }

            float4 alphaBlend(float4 top, float4 bottom)
            {
                float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
                float alpha = top.a + bottom.a * (1 - top.a);
                return float4(color, alpha);
            }

            float4 frag(v2f i) : SV_Target
            {
                // Retrieve depth (0-1 range)
                float existingDepth01 = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, i.screenPos.xy / i.screenPos.w).r;
                
                // Convert depth to linear depth in world space
                float existingDepthLinear = LinearEyeDepth(existingDepth01, _ZBufferParams);
                float depthDifference = existingDepthLinear - i.screenPos.w;

                // Compute water depth effect
                float waterDepth01 = saturate(depthDifference / _DepthMaxDistance);
                float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepth01);
                
                // Foam Calculation
                float foamDepth01 = saturate(depthDifference / lerp(_FoamMaxDistance, _FoamMinDistance, dot(float3(0,1,0), i.normalWS)));
                float foamNoiseThreshold = foamDepth01 * _SurfaceNoiseCutoff;

                // Distortion
                float2 distortSample = (tex2D(_SurfaceDistortion, i.distortUV).xy * 2 - 1) * _SurfaceDistortionAmount;
                float2 noiseUV = i.noiseUV + float2(_Time.y * _SurfaceNoiseScroll.x, _Time.y * _SurfaceNoiseScroll.y) + distortSample;
                float surfaceNoiseSample = tex2D(_SurfaceNoise, noiseUV).r;

                // Foam alpha
                float surfaceNoise = smoothstep(foamNoiseThreshold - 0.01, foamNoiseThreshold + 0.01, surfaceNoiseSample);
                float4 foamColor = _FoamColor;
                foamColor.a *= surfaceNoise;

                // Blend foam with water
                return alphaBlend(foamColor, waterColor);
            }
            ENDHLSL
        }
    }
}

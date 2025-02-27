Shader "Custom/StylizedToonWater"
{
    Properties
    {
        [Header(Colors)]
        _ShallowColor ("Shallow Color", Color) = (0.5, 0.8, 1, 0.5)
        _DeepColor ("Deep Color", Color) = (0.1, 0.3, 0.8, 0.8)
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        
        [Header(Wave Settings)]
        _WaveSpeed ("Wave Speed", Range(0, 2)) = 0.5
        _WaveScale ("Wave Scale", Range(1, 50)) = 10
        _WaveHeight ("Wave Height", Range(0, 1)) = 0.1
        _WaveDirection ("Wave Direction", Vector) = (1, 1, 0, 0)
        
        [Header(Foam Settings)]
        _FoamAmount ("Foam Amount", Range(0, 5)) = 1
        _FoamCutoff ("Foam Cutoff", Range(0, 1)) = 0.8
        _FoamScale ("Foam Scale", Range(1, 50)) = 25
        _FoamSpeed ("Foam Speed", Range(0, 2)) = 0.5
        
        [Header(Caustics)]
        _CausticsColor ("Caustics Color", Color) = (1, 1, 1, 1)
        _CausticsScale ("Caustics Scale", Range(1, 50)) = 15
        _CausticsSpeed ("Caustics Speed", Range(0, 2)) = 0.5
        _CausticsIntensity ("Caustics Intensity", Range(0, 1)) = 0.5
        
        [Header(Toon Settings)]
        _ToonSteps ("Toon Steps", Range(1, 5)) = 3
        _ToonCutoff ("Toon Cutoff", Range(0, 1)) = 0.5
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
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float4 _FoamColor;
                float4 _CausticsColor;
                float4 _WaveDirection;
                
                float _WaveSpeed;
                float _WaveScale;
                float _WaveHeight;
                float _FoamAmount;
                float _FoamCutoff;
                float _FoamScale;
                float _FoamSpeed;
                float _CausticsScale;
                float _CausticsSpeed;
                float _CausticsIntensity;
                float _ToonSteps;
                float _ToonCutoff;
            CBUFFER_END
            
            // Hash function for procedural noise
            float2 hash22(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                          dot(p, float2(269.5, 183.3)));
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }
            
            // Voronoi noise function
            float2 voronoi(float2 x)
            {
                float2 n = floor(x);
                float2 f = frac(x);
                float2 mg = 0;
                float md = 8.0;
                
                for(int j = -1; j <= 1; j++)
                {
                    for(int i = -1; i <= 1; i++)
                    {
                        float2 g = float2(i, j);
                        float2 o = hash22(n + g);
                        o = 0.5 + 0.5 * sin(_Time.y * _WaveSpeed + 6.2831 * o);
                        float2 r = g + o - f;
                        float d = dot(r, r);
                        if(d < md)
                        {
                            md = d;
                            mg = g;
                        }
                    }
                }
                return float2(md, mg.x + mg.y * 2.0);
            }
            
            float3 hash33(float3 p)
            {
                p = float3(dot(p, float3(127.1, 311.7, 74.7)),
                          dot(p, float3(269.5, 183.3, 246.1)),
                          dot(p, float3(113.5, 271.9, 124.6)));
                
                return -1.0 + 2.0 * frac(sin(p) * 43758.5453123);
            }
            
            float noise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                float3 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(lerp(lerp(dot(hash33(i + float3(0.0, 0.0, 0.0)), f - float3(0.0, 0.0, 0.0)),
                                    dot(hash33(i + float3(1.0, 0.0, 0.0)), f - float3(1.0, 0.0, 0.0)), u.x),
                               lerp(dot(hash33(i + float3(0.0, 1.0, 0.0)), f - float3(0.0, 1.0, 0.0)),
                                    dot(hash33(i + float3(1.0, 1.0, 0.0)), f - float3(1.0, 1.0, 0.0)), u.x), u.y),
                          lerp(lerp(dot(hash33(i + float3(0.0, 0.0, 1.0)), f - float3(0.0, 0.0, 1.0)),
                                    dot(hash33(i + float3(1.0, 0.0, 1.0)), f - float3(1.0, 0.0, 1.0)), u.x),
                               lerp(dot(hash33(i + float3(0.0, 1.0, 1.0)), f - float3(0.0, 1.0, 1.0)),
                                    dot(hash33(i + float3(1.0, 1.0, 1.0)), f - float3(1.0, 1.0, 1.0)), u.x), u.y), u.z);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Apply wave displacement
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                float wave = noise(float3(worldPos.xz * _WaveScale, _Time.y * _WaveSpeed));
                worldPos.y += wave * _WaveHeight;
                
                output.positionCS = TransformWorldToHClip(worldPos);
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.worldPos = worldPos;
                
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                // Sample depth and calculate water depth
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float sceneDepth = SampleSceneDepth(screenUV);
                float linearDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);
                float waterDepth = linearDepth - input.positionCS.w;
                
                // Calculate foam
                float2 foamUV = input.worldPos.xz * _FoamScale + _Time.y * _FoamSpeed;
                float foam = voronoi(foamUV).x;
                foam = step(_FoamCutoff, foam) * (1 - exp(-_FoamAmount * waterDepth));
                
                // Calculate caustics
                float2 causticsUV = input.worldPos.xz * _CausticsScale + _Time.y * _CausticsSpeed;
                float caustics = voronoi(causticsUV).x * _CausticsIntensity;
                
                // Toon shading
                float depthFactor = saturate(exp(-waterDepth * 0.3));
                float toon = floor(depthFactor * _ToonSteps) / _ToonSteps;
                
                // Final color
                float4 waterColor = lerp(_DeepColor, _ShallowColor, toon);
                waterColor.rgb += caustics * _CausticsColor.rgb;
                waterColor = lerp(waterColor, _FoamColor, foam);
                
                return waterColor;
            }
            ENDHLSL
        }
    }
} 
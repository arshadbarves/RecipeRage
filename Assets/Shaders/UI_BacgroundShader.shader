Shader "Custom/UI Background Shader"
{
    Properties
    {
        [Header(Base Colors)]
        _MainTex ("Base (Main) Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 0.8078432, 0, 1)
        _Color1 ("Color 1", Color) = (1, 0.67256093, 0, 1)
        _Color2 ("Color 2", Color) = (1, 0.7411765, 0, 1)
        _Color3 ("Color 3", Color) = (1, 0.7411765, 0, 1)

        [Header(Wave Effects)]
        _Scale1 ("Wave 1 (Scale, Speed, Threshold)", Vector) = (15, 0.4, 0.975, 0)
        _Scale2 ("Wave 2 (Scale, Speed, Threshold)", Vector) = (25, 0.8, 0.5, 0)
        _Scale3 ("Wave 3 (Scale, Speed, Threshold)", Vector) = (75, 3.2, 0.8, 0)

        [Header(Noise Effect)]
        _NoiseScale ("Noise Scale", Float) = 10
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.1
        _NoiseSpeed ("Noise Animation Speed", Float) = 0.5

        [Header(Vignette Effect)]
        _VignetteStrength ("Vignette Strength", Range(0, 2)) = 0.5
        _VignetteSmoothing ("Vignette Smoothing", Range(0.01, 2)) = 0.2
        _VignetteColor ("Vignette Color", Color) = (0.7372549, 0.49019608, 0.68235296, 1)

        [Header(Pulse Effect)]
        _PulseSpeed ("Pulse Speed", Float) = 1
        _PulseStrength ("Pulse Strength", Range(0, 1)) = 0.1

        [Header(Seasonal Effects)]
        [Enum(None, 0, Snow, 1, Leaves, 2, Bubbles, 3)] _SeasonalEffect ("Seasonal Effect", Float) = 3
        _SeasonalColor ("Seasonal Color", Color) = (1, 0.8078432, 0, 1)
        _SeasonalDensity ("Seasonal Density", Range(0, 100)) = 100
        _SeasonalSpeed ("Seasonal Speed", Float) = 1

        [Header(Overlay Texture)]
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _OverlayStrength ("Overlay Strength", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };

            TEXTURE2D(_OverlayTex);
            SAMPLER(sampler_OverlayTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor, _Color1, _Color2, _Color3;
                float4 _Scale1, _Scale2, _Scale3;
                float _NoiseScale, _NoiseStrength, _NoiseSpeed;
                float _VignetteStrength, _VignetteSmoothing;
                float4 _VignetteColor;
                float _PulseSpeed, _PulseStrength;
                float _SeasonalEffect, _SeasonalDensity, _SeasonalSpeed;
                float4 _SeasonalColor;
                float _OverlayStrength;
                float4 _OverlayTex_ST;
            CBUFFER_END

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float noise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);
                float a = random(i);
                float b = random(i + float2(1.0, 0.0));
                float c = random(i + float2(0.0, 1.0));
                float d = random(i + float2(1.0, 1.0));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            float seasonalEffect(float2 uv, float time)
            {
                float effect = 0;

                if (_SeasonalEffect == 1) // Snow
                {
                    float2 snowUV = uv * _SeasonalDensity;
                    snowUV.y -= time * _SeasonalSpeed;
                    effect = step(0.98, noise(snowUV));
                }
                else if (_SeasonalEffect == 2) // Leaves
                {
                    float2 leafUV = uv * _SeasonalDensity;
                    leafUV += sin(time * _SeasonalSpeed + leafUV.yx * 5) * 0.1;
                    effect = step(0.95, noise(leafUV));
                }
                else if (_SeasonalEffect == 3) // Bubbles
                {
                    float2 bubbleUV = uv * _SeasonalDensity;
                    bubbleUV.y += time * _SeasonalSpeed;
                    bubbleUV.x += sin(bubbleUV.y * 10) * 0.1;
                    effect = smoothstep(0.9, 1.0, 1 - length(frac(bubbleUV) - 0.5));
                }

                return effect;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv - 0.5;
                float2 resolution = _ScreenParams.xy;
                float aspect = resolution.x / resolution.y;
                float pos = length(uv * float2(aspect, 1));
                float time = _Time.y;

                // Wave effect
                float f1 = sin(pos * _Scale1.x - time * _Scale1.y);
                float f2 = sin(pos * _Scale2.x - time * _Scale2.y);
                float f3 = sin(pos * _Scale3.x - time * _Scale3.y);

                // Noise effect
                float2 noiseUV = IN.uv * _NoiseScale;
                float noiseValue = noise(noiseUV + time * _NoiseSpeed) * _NoiseStrength;

                // Vignette effect
                float vignette = smoothstep(_VignetteStrength, _VignetteStrength - _VignetteSmoothing, pos);

                // Pulsing effect
                float pulse = sin(time * _PulseSpeed) * 0.5 + 0.5;
                pulse = lerp(1, pulse, _PulseStrength);

                // Base color selection
                float3 col = _BaseColor.rgb;
                if (f1 > _Scale1.z) col = _Color1.rgb;
                else if (f2 > _Scale2.z) col = _Color2.rgb;
                else if (f3 > _Scale3.z) col = _Color3.rgb;

                // Apply effects
                col += noiseValue;
                col = lerp(col, _VignetteColor.rgb, 1 - vignette);
                col *= pulse;

                // Seasonal effect
                float seasonalEffectValue = seasonalEffect(IN.uv, time);
                col = lerp(col, _SeasonalColor.rgb, seasonalEffectValue);

                // Overlay texture
                float2 overlayUV = IN.uv * _OverlayTex_ST.xy + _OverlayTex_ST.zw;
                float4 overlayColor = SAMPLE_TEXTURE2D(_OverlayTex, sampler_OverlayTex, overlayUV);
                col = lerp(col, overlayColor.rgb, overlayColor.a * _OverlayStrength);

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
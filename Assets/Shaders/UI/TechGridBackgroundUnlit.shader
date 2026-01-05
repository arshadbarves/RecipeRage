Shader "Unlit/TechGridBackground"
{
    Properties
    {
        [Header(Radial Gradient)]
        _CenterColor ("Center Color", Color) = (0.102, 0.125, 0.173, 1) // #1a202c
        _EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)                 // #000000
        _GradientRadius ("Gradient Radius", Float) = 1.0
        _GradientFalloff ("Gradient Falloff", Range(0.1, 5.0)) = 1.0
        [Toggle] _CorrectAspect ("Correct Aspect Ratio", Float) = 1.0

        [Header(Grid Lines)]
        _GridColor ("Grid Color", Color) = (1, 1, 1, 1)
        _GridSize ("Grid Size (px)", Float) = 40.0
        _LineWidth ("Line Width (px)", Float) = 1.0
        _GridAlpha ("Grid Alpha", Range(0, 0.2)) = 0.03

        [Header(Vignette)]
        _VignetteIntensity ("Vignette Intensity", Range(0, 3.0)) = 1.2
        _VignetteSmoothness ("Vignette Smoothness", Range(0.1, 5.0)) = 1.5
        _VignetteRoundness ("Vignette Roundness", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Background"
        }

        LOD 100
        Cull Off
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };

            fixed4 _CenterColor;
            fixed4 _EdgeColor;
            float _GradientRadius;
            float _GradientFalloff;
            float _CorrectAspect;

            fixed4 _GridColor;
            float _GridSize;
            float _LineWidth;
            float _GridAlpha;

            float _VignetteIntensity;
            float _VignetteSmoothness;
            float _VignetteRoundness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // === 1. RADIAL GRADIENT ===
                // CSS: radial-gradient(circle at center, ...)
                float2 uv = i.uv - 0.5; // Center UV at 0

                // Correction for Aspect Ratio to make it a perfect circle
                if (_CorrectAspect > 0.5)
                {
                    float aspect = _ScreenParams.x / _ScreenParams.y;
                    uv.x *= aspect;
                }

                float dist = length(uv);

                // Scale radius (default 0.5 reaches edge of height if aspect corrected and size matches)
                // _GradientRadius 1.0 means it covers more screen.
                // Standard CSS circle at center reaches closest side (0.5).
                // Let's normalize so 1.0 is "full screen height" radius approx.
                float radius = 0.5 * _GradientRadius;

                float t = saturate(dist / radius);
                t = pow(t, _GradientFalloff); // Curve control

                fixed3 bgColor = lerp(_CenterColor.rgb, _EdgeColor.rgb, t);

                // === 2. GRID LINES ===
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float2 pixels = screenUV * _ScreenParams.xy;

                // Get position within each grid cell
                float2 cellPos = fmod(abs(pixels), _GridSize);

                // Check if we're in the first _LineWidth pixels of the cell
                float lineX = step(cellPos.x, _LineWidth);
                float lineY = step(cellPos.y, _LineWidth);

                // Either horizontal OR vertical line
                float isOnLine = max(lineX, lineY);

                // === 3. COMPOSITE GRID ===
                fixed3 gridColor = _GridColor.rgb;
                fixed3 compositedColor = lerp(bgColor, gridColor, isOnLine * _GridAlpha);

                // === 4. VIGNETTE ===
                // Vignette usually follows screen shape (elliptical) or circle?
                // Standard is elliptical (based on uncorrected UV).
                // Let's use uncorrected UV for vignette (i.uv)
                float2 uvVig = i.uv - 0.5;
                // Roundness control: 1 = Circle (aspect corrected), 0 = Ellipse (screen fit)
                // Actually simpler: mix aspect corrected UVs with uncorrected.
                float2 vigCoord = uvVig;
                if (_CorrectAspect > 0.5)
                {
                    float aspect = _ScreenParams.x / _ScreenParams.y;
                    // Mixed based on roundness
                    // Roundness 1 -> Corrected
                    // Roundness 0 -> Uncorrected (stretch to fit screen)
                    vigCoord.x *= lerp(1.0, aspect, _VignetteRoundness);
                }

                float vigDist = length(vigCoord);
                vigDist *= _VignetteIntensity; // Scale

                float vignette = saturate(1.0 - vigDist);
                vignette = pow(vignette, _VignetteSmoothness);

                fixed3 finalColor = compositedColor * vignette;

                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}

Shader "Unlit/TechGridBackground"
{
    Properties
    {
        _CenterColor ("Center Color", Color) = (0.1, 0.12, 0.17, 1) // #1a202c
        _EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)        // #000000
        _GridColor ("Grid Color", Color) = (1, 1, 1, 0.03)     // White with low alpha
        _GridSize ("Grid Size", Float) = 40.0
        _LineWidth ("Line Width", Float) = 1.0
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
        ZTest LEqual

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
            fixed4 _GridColor;
            float _GridSize;
            float _LineWidth;

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
                // 1. Radial Gradient (Center to Corners)
                // Matches: radial-gradient(circle at center, #1a202c 0%, #000000 100%)
                float2 uv = i.uv;
                float dist = distance(uv, float2(0.5, 0.5));
                // For a circular gradient to edges (not corners), use 0.5 as radius
                // This matches CSS "circle at center" with "closest-side" behavior
                float radius = 0.5;
                float t = saturate(dist / radius);
                fixed4 bgColor = lerp(_CenterColor, _EdgeColor, t);

                // 2. Grid Lines using screen-space coordinates
                // Matches: linear-gradient(rgba(255,255,255,0.03) 1px, transparent 1px)
                // repeated every 40px in both directions
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float2 screenPixels = screenUV * _ScreenParams.xy;
                
                // Grid logic: modulo to get repeating pattern
                float2 gridMod = fmod(screenPixels, _GridSize);
                
                // Check if we're on a grid line (first _LineWidth pixels of each tile)
                float onLineX = step(gridMod.x, _LineWidth);
                float onLineY = step(gridMod.y, _LineWidth);
                float isGrid = max(onLineX, onLineY);
                
                // Alpha blend white grid lines over background
                // CSS compositing: rgba(255,255,255,0.03) over radial gradient
                float gridAlpha = isGrid * _GridColor.a;
                fixed3 gridRGB = fixed3(1, 1, 1); // Pure white
                
                // Standard alpha blending: dst = src * alpha + bg * (1 - alpha)
                fixed4 finalColor = fixed4(
                    lerp(bgColor.rgb, gridRGB, gridAlpha),
                    1.0
                );
                
                return finalColor;
            }
            ENDCG
        }
    }
}

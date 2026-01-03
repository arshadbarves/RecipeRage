Shader "UI/TechGridBackground"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CenterColor ("Center Color", Color) = (0.1, 0.12, 0.17, 1) // #1a202c
        _EdgeColor ("Edge Color", Color) = (0, 0, 0, 1)        // #000000
        _GridColor ("Grid Color", Color) = (1, 1, 1, 0.03)     // White with low alpha
        _GridSize ("Grid Size", Float) = 40.0
        _LineWidth ("Line Width", Float) = 1.0
        
        // UI Masking
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 screenPos : TEXCOORD1;           
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            
            fixed4 _CenterColor;
            fixed4 _EdgeColor;
            fixed4 _GridColor;
            float _GridSize;
            float _LineWidth;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color;
                
                // Screen position for pixel-perfect grid based on screen coords
                // Or use local coords if we want it attached to element.
                // Using screen pos for 'tech' look that stays fixed is cool, 
                // but local rect is safer for UI scaling. 
                // Let's use local TexCoords which are 0-1 for the element.
                // To maintain square grid, we need Aspect Ratio correction? 
                // For standard grid background, screen-space fragment logic is easiest.
                OUT.screenPos = ComputeScreenPos(OUT.vertex);
                
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. Radial Gradient (Center to Corners)
                // Center is (0.5, 0.5) in UV space
                float2 uv = IN.texcoord;
                float dist = distance(uv, float2(0.5, 0.5));
                // radius scale approx 0.707 (sqrt(2)/2) is corner
                float radius = 0.707;
                float t = saturate(dist / radius);
                // Non-linear falloff for "radial-gradient" look
                // CSS radial defaults to elliptical if size is different, but here circle is requested.
                fixed4 bgColor = lerp(_CenterColor, _EdgeColor, t * 1.5); // *1.5 to darken corners faster

                // 2. Grid Lines
                // Use screen coordinates (converted to pixels) to make grid consistent size
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float2 screenPixels = screenUV * _ScreenParams.xy;
                
                // Grid Logic
                // pixel position modulo grid size
                float2 gridMod = fmod(screenPixels, _GridSize);
                
                // Line width check
                // If mod < line_width, we are on a line
                float onLineX = step(gridMod.x, _LineWidth);
                float onLineY = step(gridMod.y, _LineWidth);
                
                float isGrid = max(onLineX, onLineY);
                
                // Grid Fade (optional - fade grid at edges like vignette?)
                // Let's keep it uniform as per reference "linear-gradient ... 1px"
                
                fixed4 finalColor = lerp(bgColor, _GridColor, isGrid * _GridColor.a);
                finalColor.a = 1.0; // Opaque background

                // Unity UI Clipping
                finalColor.a *= UnityGet2DClipping(IN.vertex.xy, _ClipRect);
                
                return finalColor;
            }
            ENDCG
        }
    }
}

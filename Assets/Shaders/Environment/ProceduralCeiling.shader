Shader "Custom/ProceduralCeiling"
{
    Properties
    {
        [Header(Tile Colors)]
        _TileColor("Tile Color", Color) = (0.92, 0.88, 0.82, 1)
        _GridColor("Grid Line Color", Color) = (0.80, 0.76, 0.70, 1)
        
        [Header(Grid Settings)]
        _TileScaleX("Tiles X", Float) = 8.0
        _TileScaleY("Tiles Y", Float) = 6.0
        _GridThickness("Grid Line Thickness", Range(0.005, 0.1)) = 0.02
        _GridSoftness("Grid Softness", Range(0.0, 1.0)) = 0.2
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

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
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _TileColor;
                float4 _GridColor;
                float _TileScaleX;
                float _TileScaleY;
                float _GridThickness;
                float _GridSoftness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 scaledUV = input.uv * float2(_TileScaleX, _TileScaleY);
                float2 grid = frac(scaledUV);
                
                // Thin, soft grid lines
                float softness = _GridThickness * _GridSoftness;
                float lineX = smoothstep(0.0, _GridThickness + softness, grid.x) * 
                              smoothstep(0.0, _GridThickness + softness, 1.0 - grid.x);
                float lineY = smoothstep(0.0, _GridThickness + softness, grid.y) * 
                              smoothstep(0.0, _GridThickness + softness, 1.0 - grid.y);
                float isTile = lineX * lineY;
                
                return lerp(_GridColor, _TileColor, isTile);
            }
            ENDHLSL
        }
    }
}

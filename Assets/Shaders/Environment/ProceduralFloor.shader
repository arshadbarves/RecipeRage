Shader "Custom/ProceduralFloor"
{
    Properties
    {
        [Header(Tile Colors)]
        _TileColor("Tile Color", Color) = (0.85, 0.72, 0.60, 1)
        _GroutColor("Grout Color", Color) = (0.70, 0.58, 0.48, 1)
        
        [Header(Grid Settings)]
        _TileScaleX("Tiles X", Float) = 6.0
        _TileScaleY("Tiles Y", Float) = 4.0
        _GroutThickness("Grout Thickness", Range(0.01, 0.2)) = 0.04
        _GroutSoftness("Grout Softness", Range(0.0, 1.0)) = 0.3
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
                float4 _GroutColor;
                float _TileScaleX;
                float _TileScaleY;
                float _GroutThickness;
                float _GroutSoftness;
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
                
                // Soft grout lines using smoothstep
                float softness = _GroutThickness * _GroutSoftness;
                float lineX = smoothstep(0.0, _GroutThickness + softness, grid.x) * 
                              smoothstep(0.0, _GroutThickness + softness, 1.0 - grid.x);
                float lineY = smoothstep(0.0, _GroutThickness + softness, grid.y) * 
                              smoothstep(0.0, _GroutThickness + softness, 1.0 - grid.y);
                float isTile = lineX * lineY;
                
                return lerp(_GroutColor, _TileColor, isTile);
            }
            ENDHLSL
        }
    }
}

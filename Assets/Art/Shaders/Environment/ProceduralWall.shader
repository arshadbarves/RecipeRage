Shader "Custom/ProceduralWall"
{
    Properties
    {
        [Header(Gradient Colors)]
        _TopColor("Top Color", Color) = (0.55, 0.78, 0.85, 1)
        _BottomColor("Bottom Color", Color) = (0.38, 0.58, 0.68, 1)
        _EdgeColor("Edge/Vignette Color", Color) = (0.32, 0.50, 0.60, 1)
        
        [Header(Gradient Settings)]
        _GradientStart("Gradient Start", Range(0.0, 1.0)) = 0.0
        _GradientEnd("Gradient End", Range(0.0, 1.0)) = 0.35
        _GradientSmoothness("Gradient Smoothness", Range(0.01, 1.0)) = 0.4
        
        [Header(Vignette Settings)]
        [Toggle] _EnableVignette("Enable Vignette", Float) = 1
        _VignetteStrength("Vignette Strength", Range(0.0, 1.0)) = 0.25
        _VignetteRadius("Vignette Radius", Range(0.0, 2.0)) = 0.9
        _VignetteSoftness("Vignette Softness", Range(0.01, 1.0)) = 0.6
        _VignetteAspect("Vignette Aspect (Width/Height)", Range(0.1, 3.0)) = 1.5
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
                float4 _TopColor;
                float4 _BottomColor;
                float4 _EdgeColor;
                float _GradientStart;
                float _GradientEnd;
                float _GradientSmoothness;
                float _EnableVignette;
                float _VignetteStrength;
                float _VignetteRadius;
                float _VignetteSoftness;
                float _VignetteAspect;
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
                float2 uv = input.uv;
                
                // Base gradient from bottom to top
                float gradientT = smoothstep(_GradientStart, _GradientEnd + _GradientSmoothness, uv.y);
                half4 baseColor = lerp(_BottomColor, _TopColor, gradientT);
                
                // Optional vignette effect
                if (_EnableVignette > 0.5)
                {
                    // Center UV and apply aspect ratio
                    float2 centeredUV = uv - 0.5;
                    centeredUV.x *= _VignetteAspect; // Wider than tall
                    
                    float dist = length(centeredUV);
                    float vignette = smoothstep(_VignetteRadius, _VignetteRadius - _VignetteSoftness, dist);
                    
                    // Blend: center stays baseColor, edges darken toward EdgeColor
                    half4 edgeTinted = lerp(baseColor, _EdgeColor, _VignetteStrength);
                    baseColor = lerp(edgeTinted, baseColor, vignette);
                }
                
                return baseColor;
            }
            ENDHLSL
        }
    }
}

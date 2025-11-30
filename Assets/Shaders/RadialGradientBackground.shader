Shader "UI/RadialGradientBackground"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        
        _CenterColor ("Center Color", Color) = (1, 0.95, 0.85, 1)
        _EdgeColor ("Edge Color", Color) = (1, 0.65, 0.65, 1)
        _GradientPower ("Gradient Power", Range(0.5, 3.0)) = 1.5
        _CenterX ("Center X", Range(0, 1)) = 0.5
        _CenterY ("Center Y", Range(0, 1)) = 0.5
        _GradientScale ("Gradient Scale", Range(0.5, 2.0)) = 1.0
        
        [Header(Sparkle Settings)]
        _SparkleColor ("Sparkle Color", Color) = (1, 1, 1, 0.9)
        _SparkleIntensity ("Sparkle Intensity", Range(0, 1)) = 0.8
        _SparkleSpeed ("Sparkle Animation Speed", Range(0, 3)) = 1.0
        
        [Header(Floating Bubbles)]
        _BubbleColor ("Bubble Color", Color) = (1, 1, 1, 0.15)
        _BubbleCount ("Bubble Count", Range(3, 12)) = 8
        _BubbleSpeed ("Bubble Float Speed", Range(0, 1)) = 0.2
        _BubbleSize ("Bubble Size", Range(0.01, 0.15)) = 0.05
        
        [Header(Vignette)]
        _VignetteStrength ("Vignette Strength", Range(0, 1)) = 0.3
        _VignetteRadius ("Vignette Radius", Range(0.3, 1.5)) = 0.8
        
        [Header(Animation)]
        _AnimationSpeed ("Gradient Animation Speed", Range(0, 2)) = 0.3
        _PulseAmount ("Pulse Amount", Range(0, 0.2)) = 0.05
        
        [Header(Aspect Ratio)]
        _AspectRatio ("Aspect Ratio (Width/Height)", Float) = 1.777
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Background" 
            "RenderType"="Opaque"
            "PreviewType"="Plane"
        }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
            };
            
            float4 _CenterColor;
            float4 _EdgeColor;
            float _GradientPower;
            float _CenterX;
            float _CenterY;
            float _GradientScale;
            
            float4 _SparkleColor;
            float _SparkleIntensity;
            float _SparkleSpeed;
            
            float4 _BubbleColor;
            float _BubbleCount;
            float _BubbleSpeed;
            float _BubbleSize;
            
            float _VignetteStrength;
            float _VignetteRadius;
            
            float _AnimationSpeed;
            float _PulseAmount;
            float _AspectRatio;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            // Pseudo-random function
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            // Creates a 4-pointed star/sparkle shape
            float drawSparkle(float2 uv, float2 pos, float size, float rotation)
            {
                float2 delta = uv - pos;
                
                // Apply rotation
                float c = cos(rotation);
                float s = sin(rotation);
                delta = float2(delta.x * c - delta.y * s, delta.x * s + delta.y * c);
                
                // Create 4-pointed star using absolute values
                float d = length(delta);
                
                // Diamond shape - sharper edges
                float diamond = abs(delta.x) + abs(delta.y);
                float starShape = smoothstep(size, size * 0.6, diamond);
                
                // Create cross shape for sparkle rays - much sharper
                float cross = smoothstep(size * 1.8, size * 0.8, abs(delta.x)) * smoothstep(size * 0.2, size * 0.05, abs(delta.y));
                cross += smoothstep(size * 1.8, size * 0.8, abs(delta.y)) * smoothstep(size * 0.2, size * 0.05, abs(delta.x));
                
                // Combine shapes
                float sparkle = max(starShape, cross * 0.8);
                
                // Minimal glow for crisp edges
                float glow = exp(-d * 40.0 / size) * 0.08;
                
                return saturate(sparkle + glow);
            }
            
            // Simple dot sparkle
            float drawDot(float2 uv, float2 pos, float size)
            {
                float d = length(uv - pos);
                float dot = smoothstep(size, size * 0.5, d);
                float glow = exp(-d * 50.0 / size) * 0.06;
                return saturate(dot + glow);
            }
            
            // Floating bubble with vertical movement
            float drawBubble(float2 uv, float2 basePos, float size, float time, float seed)
            {
                // Vertical movement
                float yOffset = frac(time + seed) * 1.2 - 0.1; // Float from bottom to top
                
                // Gentle horizontal sway
                float sway = sin(time * 2.0 + seed * 6.28318) * 0.03;
                
                float2 pos = float2(basePos.x + sway, yOffset);
                
                float2 delta = uv - pos;
                float d = length(delta);
                
                // Soft circular bubble
                float bubble = smoothstep(size, size * 0.7, d);
                
                // Add highlight for 3D effect
                float2 highlightOffset = float2(-0.003, 0.003);
                float highlight = smoothstep(size * 0.4, size * 0.1, length(delta - highlightOffset));
                
                // Fade out at top and bottom
                float fadeFactor = smoothstep(0.0, 0.1, yOffset) * smoothstep(1.1, 0.9, yOffset);
                
                return saturate((bubble + highlight * 0.3) * fadeFactor);
            }
            
            // Vignette effect
            float vignette(float2 uv, float2 center, float radius, float strength)
            {
                float dist = length(uv - center);
                float vig = smoothstep(radius, radius * 0.3, dist);
                return 1.0 - (1.0 - vig) * strength;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Correct aspect ratio
                float2 uv = i.uv;
                uv.x *= _AspectRatio;
                
                float time = _Time.y * _AnimationSpeed;
                float sparkleTime = _Time.y * _SparkleSpeed;
                
                // Calculate distance from center with animation
                float2 center = float2(_CenterX * _AspectRatio, _CenterY);
                float2 delta = uv - center;
                
                // Add subtle animation to gradient
                float pulse = sin(time) * _PulseAmount;
                float dist = length(delta) * _GradientScale + pulse;
                
                // Apply gradient power for smoother falloff
                float gradient = pow(saturate(dist), _GradientPower);
                
                // Blend colors
                fixed4 color = lerp(_CenterColor, _EdgeColor, gradient);
                
                // Add subtle animated noise for texture
                float noise = frac(sin(dot(i.uv * 10.0 + time * 0.1, float2(12.9898, 78.233))) * 43758.5453);
                color.rgb += (noise - 0.5) * 0.01;
                
                // Define sparkle positions (aspect-ratio corrected)
                // Large sparkles (4-pointed stars)
                float2 sparklePositions[8];
                sparklePositions[0] = float2(0.92 * _AspectRatio, 0.08);  // Bottom right
                sparklePositions[1] = float2(0.15 * _AspectRatio, 0.12);  // Bottom left
                sparklePositions[2] = float2(0.08 * _AspectRatio, 0.45);  // Mid left
                sparklePositions[3] = float2(0.85 * _AspectRatio, 0.52);  // Mid right
                sparklePositions[4] = float2(0.12 * _AspectRatio, 0.88);  // Top left
                sparklePositions[5] = float2(0.50 * _AspectRatio, 0.92);  // Top center
                sparklePositions[6] = float2(0.88 * _AspectRatio, 0.85);  // Top right
                sparklePositions[7] = float2(0.78 * _AspectRatio, 0.28);  // Mid right 2
                
                // Small dot sparkles
                float2 dotPositions[6];
                dotPositions[0] = float2(0.25 * _AspectRatio, 0.25);
                dotPositions[1] = float2(0.70 * _AspectRatio, 0.15);
                dotPositions[2] = float2(0.20 * _AspectRatio, 0.75);
                dotPositions[3] = float2(0.82 * _AspectRatio, 0.65);
                dotPositions[4] = float2(0.45 * _AspectRatio, 0.18);
                dotPositions[5] = float2(0.60 * _AspectRatio, 0.78);
                
                float totalSparkle = 0.0;
                
                // Draw large sparkles with twinkling animation
                for (int j = 0; j < 8; j++)
                {
                    // Each sparkle has unique timing based on position
                    float sparklePhase = random(sparklePositions[j]) * 6.28318;
                    float twinkle = (sin(sparkleTime * 1.5 + sparklePhase) * 0.5 + 0.5);
                    twinkle = pow(twinkle, 2.0); // Make the twinkle more dramatic
                    
                    float size = lerp(0.008, 0.015, random(sparklePositions[j] * 2.0));
                    float rotation = sparkleTime * 0.5 + random(sparklePositions[j] * 3.0) * 6.28318;
                    
                    float sparkleValue = drawSparkle(uv, sparklePositions[j], size, rotation);
                    totalSparkle += sparkleValue * twinkle;
                }
                
                // Draw small dot sparkles with twinkling
                for (int k = 0; k < 6; k++)
                {
                    float dotPhase = random(dotPositions[k] * 1.5) * 6.28318;
                    float dotTwinkle = (sin(sparkleTime * 2.0 + dotPhase) * 0.5 + 0.5);
                    dotTwinkle = pow(dotTwinkle, 3.0);
                    
                    float dotSize = lerp(0.003, 0.006, random(dotPositions[k] * 2.5));
                    float dotValue = drawDot(uv, dotPositions[k], dotSize);
                    totalSparkle += dotValue * dotTwinkle * 0.7;
                }
                
                // Apply sparkles to final color
                totalSparkle = saturate(totalSparkle);
                color.rgb = lerp(color.rgb, _SparkleColor.rgb, totalSparkle * _SparkleIntensity);
                
                // Add floating bubbles
                float totalBubbles = 0.0;
                int bubbleCount = int(_BubbleCount);
                
                for (int b = 0; b < bubbleCount; b++)
                {
                    float seed = float(b) / float(bubbleCount);
                    float bubbleX = random(float2(seed, 0.5)) * _AspectRatio;
                    float2 basePos = float2(bubbleX, 0.0);
                    
                    // Vary bubble size slightly
                    float bubbleSize = _BubbleSize * (0.7 + random(float2(seed, 0.8)) * 0.6);
                    
                    // Stagger animation timing
                    float bubbleTime = _Time.y * _BubbleSpeed + seed * 5.0;
                    
                    float bubbleValue = drawBubble(uv, basePos, bubbleSize, bubbleTime, seed);
                    totalBubbles += bubbleValue;
                }
                
                // Blend bubbles
                totalBubbles = saturate(totalBubbles);
                color.rgb = lerp(color.rgb, _BubbleColor.rgb, totalBubbles * _BubbleColor.a);
                
                // Apply vignette
                float2 vignetteCenter = float2(_CenterX * _AspectRatio, _CenterY);
                float vignetteValue = vignette(uv, vignetteCenter, _VignetteRadius, _VignetteStrength);
                color.rgb *= vignetteValue;
                
                return color;
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}

Shader "UI/RadialGradient"
{
    Properties
    {
        _CenterColor ("Center Color", Color) = (0.102, 0.102, 0.102, 1) // #1a1a1a
        _EdgeColor ("Edge Color", Color) = (0, 0, 0, 1) // #000000
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius Scale", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }
        LOD 100

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
            float4 _Center;
            float _Radius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = distance(i.uv, _Center.xy);
                // Simple radial gradient logic (CSS: radial-gradient(circle at center...))
                // Map dist 0 -> Radius to 0 -> 1 clamp
                float t = saturate(dist / _Radius);
                return lerp(_CenterColor, _EdgeColor, t);
            }
            ENDCG
        }
    }
}

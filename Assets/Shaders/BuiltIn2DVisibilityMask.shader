Shader "Custom/BuiltIn2DVisibilityMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DarkColor ("Dark Color", Color) = (0, 0, 0, 0.99)
        _Center ("Light Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Light Radius", Float) = 0.2
        _Softness ("Edge Softness", Float) = 0.05
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Overlay"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        Blend SrcAlpha OneMinusSrcAlpha

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
                half4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            sampler2D _MainTex;

            half4 _DarkColor;
            float4 _Center;
            float _Radius;
            float _Softness;

            v2f vert(appdata input)
            {
                v2f output;
                
                // Convert the RawImage vertices from object local space to clip space.
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color;

                return output;
            }

            half4 frag(v2f input) : SV_Target
            {
                float2 offset = input.uv - _Center.xy;

                // Fixed Screen Ratio, avoid stretching
                offset.x *= _ScreenParams.x / _ScreenParams.y;

                float distanceToCenter = length(offset);

                float darknessMask = smoothstep(
                    _Radius,
                    _Radius + max(_Softness, 0.0001),
                    distanceToCenter
                );

                half4 finalColor = _DarkColor;

                finalColor.a *= darknessMask;
                finalColor.a *= input.color.a;

                return finalColor;
            }

            ENDCG
        }
    }

    Fallback Off
}
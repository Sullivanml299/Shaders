Shader "Unlit/PageStackSides"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Count ("Count", Float) = 1 
        _BaseColor ("Base Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Count;
            float4 _BaseColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float parabola(float x)
            {
                return 4.0 * x * (1.0 - x);
            }

            float frac(float x)
            {
                return x - floor(x);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                i.uv.y *= _Count;
                float f = frac(i.uv.y); 
                fixed4 col = _BaseColor * parabola(f);

                return col;
            }
            ENDCG
        }
    }
}

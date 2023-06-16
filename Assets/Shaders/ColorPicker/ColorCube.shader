// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/ColorCube"
{
    Properties
    {
        // _MainTex ("Texture", 2D) = "white" {}
        _Offset ("Offset", Vector) = (0, 0, 0, 0)
        _Scale ("Scale", Float) = 5
        _BackgroundColor ("BackgroundColor", Color) = (0, 0, 0, 0.1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
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
                float4 color : COLOR;
            };

            // sampler2D _MainTex;
            // float4 _MainTex_ST;
            float2 _Offset;
            float _Scale;
            float4 _BackgroundColor;

            v2f vert (appdata v)
            {   
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;// TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.vertex+0.5;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //if inside a local cirlce
                i.uv -= _Offset;
                i.uv *= _Scale;
                i.uv = pow(i.uv, 2);
                bool colored = (i.uv.x + i.uv.y)%1/_Scale <= 0.25/_Scale;

                // return float4(i.color.xyz, colored ? 1 : 0);
                if (colored) return float4(i.color.xyz, 1);
                else return _BackgroundColor;
                
            }
            ENDCG
        }
    }
}

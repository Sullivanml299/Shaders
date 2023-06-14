// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/ColorCube"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {   
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.vertex+0.5;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float scale = 5;
                // bool clear = (uint(i.uv.x*scale) & 1) ^ (uint(i.uv.y*scale) & 1);
                //if inside a local cirlce
                bool clear = ((i.uv.x*scale-0.5)*(i.uv.x*scale-0.5) + (i.uv.y*scale-0.5)*(i.uv.y*scale-0.5))%1/scale <= 0.25/scale;
                return float4(i.color.xyz, clear ? 1 : 0);
            }
            ENDCG
        }
    }
}

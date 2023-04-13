Shader "Unlit/Experiment1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            // Plot a line on Y using a value between 0.0-1.0
            float plot(float2 uv) {    
                return smoothstep(0.02, 0.0, abs(uv.y - uv.x));
            }

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                // fixed4 col = tex2D(_MainTex, i.uv);
                float y = i.uv.x;
                float pct = plot(i.uv);
                float3 color = float3(y, y, y);
                color = (1.0-pct)*color+pct*float3(0.0,1.0,0.0);
                // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, color);
                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }
}

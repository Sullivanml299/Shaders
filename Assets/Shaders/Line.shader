Shader "Unlit/Line"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainColor] _LeadColor("Leading Color", Color) = (0, 1, 0, 1)
        [MainColor] _FillColor("Fill Color", Color) = (1, 0, 0, 1)
        _lerp ("lerp", Range(0,1)) = 0.0
        _leadDistance("Leading Distance", Range(0,1)) = 0.1

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
                float4 worldVertex : TEXCOORD1;
            };

            float3 _fillVertex;
            float4 _BaseColor;
            float _lerp;
            float _leadDistance;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldVertex = mul(unity_ObjectToWorld, v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                // fill all
                if(_lerp >= 1.0){
                    return fixed4(1,0,0,1);
                }
                else if(_lerp <= 0.0){
                    return _BaseColor;
                }

                // fill
                if(_lerp > i.uv.x){
                    return fixed4(1,0,0,1);
                }

                // leading edge
                if(abs(i.uv.x - _lerp) < _leadDistance){
                    return fixed4(0,1,0,1);
                }

                // base color
                return _BaseColor;
            }
            ENDCG
        }
    }
}

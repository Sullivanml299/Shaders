Shader "Unlit/Line"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainColor] _LeadColor("Leading Color", Color) = (0, 1, 0, 1)
        [MainColor] _FillColor("Fill Color", Color) = (1, 0, 0, 1)
        _lerp ("lerp", vector) = (0.0, 0.0, 0.0, 0.0)
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
            };

            float3 _fillVertex;
            float4 _BaseColor;
            float4 _lerp;
            float _leadDistance;
            float4 _FillColor;
            float4 _LeadColor;

            float3 hash3( uint n ){
                // integer hash copied from Hugo Elias
                n = (n << 13U) ^ n;
                n = n * (n * n * 15731U + 789221U) + 1376312589U;
                uint3 k = n * uint3(n,n*16807U,n*48271U);
                return float3( k & uint3(0x7fffffffU,0x7fffffffU,0x7fffffffU))/float(0x7fffffff);
            }

            float getLerpValue(float uv_y){
                float lineWidth = 1./4.;
                if(uv_y < lineWidth){
                    return _lerp.x;
                }
                else if(uv_y < lineWidth*2){
                    return _lerp.y;
                }
                else if(uv_y < lineWidth*3){
                    return _lerp.z;
                }
                else{
                    return _lerp.w;
                }
            }

            v2f vert (appdata v)
            {
                //TODO: the time multiplier should be a property
                //random movement
                if(_lerp.x > v.uv.x){
                    v.vertex.xyz = v.vertex.xyz + hash3(v.vertex.x + 1920U*v.vertex.y + _Time*10000)*0.02;
                }

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 noise = fixed4(hash3(i.vertex.x + 1920U*i.vertex.y + (1920U*1080U)*_Time),1);
                fixed4 fill = _FillColor*noise;
                fixed4 lead = _LeadColor*noise;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                float lerpValue = getLerpValue(i.uv.y);

                // fill all
                if(lerpValue >= 1.0){
                    return fill;
                }
                // fill none
                else if(lerpValue <= 0.0){
                    return _BaseColor;
                }

                // fill
                if(lerpValue > i.uv.x){
                    return fill;
                }

                // leading edge
                if(abs(i.uv.x - lerpValue) < _leadDistance){
                    return lead;
                }

                // base color
                return _BaseColor;
            }
            ENDCG
        }
    }
}

Shader "Unlit/CloudyCube"
{
    Properties
    {
        _MousePos("Mouse", Vector) = (-1, -1, 0, 0)
        _MouseColor("Mouse Color", Color) = (0, 0, 0, 0)
        _TriangleIndex("Triangle Index", Integer) = 0
        _Scale ("Scale", Float) = 5
        _Persistance ("Persistance", Range(0.0, 2.0)) = 1.0
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

            float2 _MousePos;
            float _Scale;
            float _Persistance;
            float4 _MouseColor;
            uint _TriangleIndex;

            v2f vert (appdata v)
            {   
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.vertex+0.5;
                return o;
            }

            float random(float2 uv){
                return frac(sin(dot(uv.xy, float2(12.9898,78.233)))*43758.5453123);
            }

            float noise(float2 uv){
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f*f*(3.0-2.0*f);
                return lerp(lerp(random(i), random(i+float2(1,0)), f.x),
                            lerp(random(i+float2(0,1)), random(i+float2(1,1)), f.x), f.y);
            }

            float fbm(float2 uv){
                float total = 0.0;
                float persistence = _Persistance;
                float lacunarity = 2.;
                float2 shift = float2(100, 100);
                for(int i = 0; i < 4; i++){
                    total += noise(uv)*persistence;
                    uv = uv*lacunarity + shift;
                    persistence *= 0.5;
                }
                return total;
            }

            float4 saturate(float4 color){
                return clamp(color, 0, 1);
            }

            bool inCircle(float2 uv, float2 center, float radius){
                float2 diff = uv - center;
                return dot(diff, diff) < radius*radius;
            }

            bool inFace(uint pid){
                float2 uv = _MousePos.xy;
                if(uv.x > uv.y){
                    if(_TriangleIndex == pid || _TriangleIndex == pid+1){
                        return true;
                    }
                    return false;
                }
                else{
                    if(_TriangleIndex == pid || _TriangleIndex == pid-1){
                        return true;
                    }
                    return false;
                }
            }

            fixed4 frag (v2f i, uint pid:SV_PrimitiveID) : SV_Target
            {
                float4 color = float4(i.color.xyz, 1);

                if(_MousePos.x>=0 && inFace(pid) && inCircle(i.uv, _MousePos.xy, 0.1)){
                    color = _MouseColor;
                }
                else{
                    i.uv *= _Scale;
                    float2 offset = float2(_SinTime.y,_CosTime.y);
                    float n1 = fbm(i.uv+offset);
                    float n2 = fbm(i.uv-offset);
                    float f = fbm(i.uv+ fbm(i.uv + fbm(i.uv + offset)));
                    color = saturate(color*f*n1*n2);
                }


                // return saturate(color*n1*n2);
                return color;
            }
            ENDCG
        }
    }
}

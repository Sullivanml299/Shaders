Shader "Unlit/CloudyCube"
{
    Properties
    {
        _MousePos("Mouse", Vector) = (-1, -1, 0, 0)
        _MouseColor("Mouse Color", Color) = (0, 0, 0, 0)
        _TriangleIndex("Triangle Index", Integer) = 0
        _Scale ("Scale", Float) = 5
        _Persistance ("Persistance", Range(0.0, 2.0)) = 1.5
        _InnerRadius ("Inner Radius", Range(0.0, 1.0)) = 0.2
        _OuterRadius ("Outer Radius", Range(0.0, 1.0)) = 0.4
        _RippleFrequency ("Ripple Frequency", Range(1.0, 200.0)) = 100.0
        _RippleSpeed ("Ripple Speed", Range(0.1, 10.0)) = 1.5
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
            float _InnerRadius;
            float _OuterRadius;
            float _RippleSpeed;
            float _RippleFrequency;
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

            float rippleValue(float2 uv, float2 center, float radius){
                float2 diff = uv - center;
                float dist = length(diff);
                float t = 1.0 - sin(sin(1-dist*dist*_RippleFrequency+_Time.w*_RippleSpeed));
                return t;
            }

            float4 saturate(float4 color){
                return clamp(color, 0, 1);
            }

            float3 saturate(float3 color){
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

            float4 liquid(float4 color, v2f i){
                i.uv *= _Scale;
                float2 offset = float2(_SinTime.x,_CosTime.x);
                float n1 = fbm(i.uv+offset);
                float n2 = fbm(i.uv-offset);
                float f = fbm(i.uv+ fbm(i.uv + fbm(i.uv + offset)));
                color = color*f*n1*n2;
                return color;
            }

            fixed4 frag (v2f i, uint pid:SV_PrimitiveID) : SV_Target
            {
                float4 color = float4(i.color.xyz, 1);
                float4 liquidColor = liquid(color, i);
                color = liquidColor;

                if(_MousePos.x>=0 && inFace(pid) && inCircle(i.uv, _MousePos.xy, _OuterRadius)){
                    float2 diff = i.uv - _MousePos.xy;
                    float dist = length(diff);
                    float ripple = rippleValue(i.uv, _MousePos.xy, 0.1);
                    float3 rippleColor = liquidColor.xyz * float3(.01,.01,.01) * ripple*100;

                    //blend outer radius and base color
                    color.xyz = lerp(rippleColor, liquidColor.xyz, dist/_OuterRadius); 

                    //blend inner radius and base color
                    if(inCircle(i.uv, _MousePos.xy, _InnerRadius)){
                        color.xyz = lerp(_MouseColor.xyz, color.xyz, dist/_InnerRadius);
                    } 
                }
                else{
                    color = liquidColor;
                }

                return saturate(color);
            }
            ENDCG
        }
    }
}

Shader "Unlit/ColorCube"
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

            float random(float3 position){
                return frac(sin(dot(position, float3(12.9898,78.233,29.1653)))*43758.5453123);
            }

            float noise(float3 position){
                float3 i = floor(position);
                float3 f = frac(position);
                f = f*f*(3.0-2.0*f);
                return lerp(lerp(lerp(random(i), random(i+float3(1,0,0)),f.x),
                                lerp(random(i+float3(0,1,0)), random(i+float3(1,1,0)),f.x),f.y),
                            lerp(lerp(random(i+float3(0,0,1)), random(i+float3(1,0,1)),f.x),
                                lerp(random(i+float3(0,1,1)), random(i+float3(1,1,1)),f.x),f.y),f.z);
            }

            float fbm(float3 position){
                float total = 0.0;
                float persistence = _Persistance;
                float lacunarity = 2.;
                float3 shift = float3(100, 100, 100);
                for(int i = 0; i < 4; i++){
                    total += noise(position)*persistence;
                    position = position*lacunarity + shift;
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
                i.color *= _Scale;
                float3 offset = float3(_SinTime.x,_CosTime.x, _SinTime.y);
                float n1 = fbm(i.color.xyz+offset);
                float n2 = fbm(i.color.xyz-offset);
                float f = fbm(i.color.xyz + fbm(i.color.xyz + fbm(i.color.xyz+ offset)));
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

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Volume1"
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
            #define STEPS 64
            #define STEP_SIZE 0.1
            #define _Center float3(0,0,0)
            #define _Radius 0.5

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            bool sphereHit (float3 p)
            {
                return distance(p,_Center) < _Radius;
            }
            
            bool raymarchHit (float3 position, float3 direction)
            {
                for (int i = 0; i < STEPS; i++)
                {
                    if ( sphereHit(position) )
                        return true;
                    position += direction * STEP_SIZE;
                }
                return false;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                float3 worldPosition = i.worldPosition;
                float3 viewDirection = normalize(i.worldPosition - _WorldSpaceCameraPos);

                if(raymarchHit(worldPosition, viewDirection))
                    return fixed4(1,0,0,1);
                else
                    return fixed4(1,1,1,1);
            }
            ENDCG
        }
    }
}

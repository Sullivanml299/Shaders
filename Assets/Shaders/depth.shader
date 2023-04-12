// This Unity shader reconstructs the world space positions for pixels using a depth
// texture and screen space UV coordinates. The shader draws a checkerboard pattern
// on a mesh to visualize the positions.
Shader "Example/URPReconstructWorldPos"
{
    Properties
    {
        _innerFresnelPower("Inner Fresnel Power", Range(0, 10)) = 2.0
        _outerFresnelPower("Outer Fresnel Power", Range(0, 10)) = 5.0
     }

    // The SubShader block containing the Shader code.
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            // This line defines the name of the vertex shader.
            #pragma vertex vert
            // This line defines the name of the fragment shader.
            #pragma fragment frag

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // The DeclareDepthTexture.hlsl file contains utilities for sampling the
            // Camera depth texture.
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS : POSITION;
                // Declaring the variable containing the normal vector for each vertex.
                half3 normal : NORMAL;
            };

            struct v2f
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
                // The variable for storing the normal vector values.
                half3 normal : NORMAL;
                half3 viewDir : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                // The following line declares the _BaseColor variable, so that you
                // can use it in the fragment shader.
                float _outerFresnelPower;
                float _innerFresnelPower;
            CBUFFER_END

            // The vertex shader definition with properties defined in the Varyings
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            v2f vert(Attributes IN)
            {
                // Declaring the output object (OUT) with the Varyings struct.
                v2f OUT;
                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous clip space.
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normal = TransformObjectToWorldNormal(IN.normal);
                OUT.worldPos = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.viewDir = GetWorldSpaceViewDir(OUT.worldPos.xyz);
                // Returning the output.
                return OUT;
            }

            half fresnel(half3 normal, half3 viewDir, half power){
                return pow((1.0 - saturate(dot(normalize(normal),normalize(viewDir)))), power);
            }

            bool isSparkle(float3 worldPos){
                if (frac((worldPos*_CosTime.xyz*34242.432).z ) > 0.5){
                    return true;
                }
                return false;
            }

            float3 hash3( uint n ){
                // integer hash copied from Hugo Elias
                n = (n << 13U) ^ n;
                n = n * (n * n * 15731U + 789221U) + 1376312589U;
                uint3 k = n * uint3(n,n*16807U,n*48271U);
                return float3( k & uint3(0x7fffffffU,0x7fffffffU,0x7fffffffU))/float(0x7fffffff);
            }


            // The fragment shader definition.
            // The Varyings input structure contains interpolated values from the
            // vertex shader. The fragment shader uses the `positionHCS` property
            // from the `Varyings` struct to get locations of pixels.
            half4 frag(v2f IN) : SV_Target
            {
                // To calculate the UV coordinates for sampling the depth buffer,
                // divide the pixel location by the render target resolution
                // _ScaledScreenParams.
                float2 UV = IN.positionHCS.xy / _ScaledScreenParams.xy;

                // Sample the depth from the Camera depth texture.
                #if UNITY_REVERSED_Z
                    real depth = SampleSceneDepth(UV);
                #else
                    // Adjust Z to match NDC for OpenGL ([-1, 1])
                    real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
                #endif

                // Reconstruct the world space positions.
                float3 worldPos = ComputeWorldSpacePosition(UV, depth, UNITY_MATRIX_I_VP);

                // // The following part creates the checkerboard effect.
                // // Scale is the inverse size of the squares.
                // uint scale = 1;
                // // Scale, mirror and snap the coordinates.
                // uint3 worldIntPos = uint3(abs(worldPos.xyz * scale));
                // // Divide the surface into squares. Calculate the color ID value.
                // bool white = ((worldIntPos.x) & 1) ^ (worldIntPos.y & 1) ^ (worldIntPos.z & 1);
                // // Color the square based on the ID value (black or white).
                // half4 color = white ? half4(1,1,1,1) : half4(0,0,0,1);

                // Set the color to black in the proximity to the far clipping
                // plane.
                #if UNITY_REVERSED_Z
                    // Case for platforms with REVERSED_Z, such as D3D.
                    if(depth < 0.0001)
                        return half4(0,0,0,1);
                #else
                    // Case for platforms without REVERSED_Z, such as OpenGL.
                    if(depth > 0.9999)
                        return half4(0,0,0,1);
                #endif

                //override checkerboard effect
                half4 color = half4(1,1,1,1);

                //Add fresnel effect
                float outerFresnelValue = fresnel(IN.normal, IN.viewDir, _outerFresnelPower);
                float innerFresnelValue = fresnel(IN.normal, IN.viewDir, _innerFresnelPower);

                // standard fresnel effect
                color.rgb *= outerFresnelValue* half3(0.1,.0,.5);
                color.rgb += innerFresnelValue* half3(0.,.5,0.);

                // inverted fresnel effect
                // color.rgb -= fresnelValue* half3(1.,1.,1.);
                // color.rgb += fresnelValue* half3(.5,0.,0.);

                // // fresnel effect with custom background
                // color.rgb *= fresnelValue;
                // color.rgb += (1 - fresnelValue)* half3(0.1,.0,.5);
                
                // static effect
                float3 staticEffect = hash3( IN.positionHCS.x + 1920U*IN.positionHCS.y + (1920U*1080U)*_Time );
                color.rgb *= staticEffect;

                return color;
            }
            ENDHLSL
        }
    }
}
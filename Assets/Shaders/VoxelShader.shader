Shader "VoxelEngine/URP Basic Voxel"
{
    Properties
    { 
        _MainTex("Texture", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {        
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // function to calculate the UV
            float2 CalculateUV(float3 normal, float3 position)
            {
                float3 absN = abs(normal);
                float sum = absN.x + absN.y + absN.z;
                float3 weights = absN / sum;

                float2 uv_xy = position.xy; // for faces along Z
                float2 uv_xz = position.xz; // for top/bottom  Y
                float2 uv_zy = position.zy; // for faces along X

                return uv_xy * weights.z + uv_xz * weights.y + uv_zy * weights.x;

            }

            struct a2v
            {
                float4 position   : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 position  : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            half4 _BaseColor;
            half4 _MainTex_ST;
            sampler2D _MainTex;

            v2f vert(a2v i)
            {
                v2f vert;
                vert.position = TransformObjectToHClip(i.position.xyz);
                vert.normal = i.normal;
                vert.uv = CalculateUV(i.normal, i.position);
                return vert;
            }

            half4 frag(v2f vert) : SV_Target
            {
                //Sample the texture
                half4 texColor = tex2D(_MainTex, vert.uv);
                return texColor * _BaseColor;
            }

            
            ENDHLSL
        }
    }
}
Shader "Unlit/Voxel_Terrain_Shader"
{
    Properties
    {
        _TintColor ("Color", Color) = (1,1,1,1)
        _TopTexture ("Top Texture", 2D) = "white" {}
        _SideTexture ("Side Texture", 2D) = "white" {}
        _BottomTexture ("Bottom Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            // indicate that our pass is the "base" pass in forward
            // rendering pipeline. It gets ambient and main directional
            // light data set up; light direction in _WorldSpaceLightPos0
            // and color in _LightColor0
            Tags {"LightMode"="ForwardBase"}
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // For shadows
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            
            #include "UnityCG.cginc" // for UnityObjectToWorldNormal
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            // Properties
            fixed4 _TintColor;
            sampler2D _TopTexture;
            sampler2D _SideTexture;
            sampler2D _BottomTexture;

            //Const
            //Shadow min factor
            #define SHADOW_MIN 0.2f


            // Struct for vertex shader input
            struct a2v
            {
                fixed4 vertex : POSITION;
                fixed3 normal : NORMAL;
                fixed faceIndex : TEXCOORD0;
            };
            
            struct v2f
            {
                fixed4 diff : COLOR0; // diffuse lighting color
                fixed4 pos : SV_POSITION;
                fixed3 normal : NORMAL;
                
                fixed2 faceIndex : TEXCOORD0;
                fixed3 worldPos : TEXCOORD1;
                SHADOW_COORDS(2) // put shadows data into TEXCOORD2
            };

            half2 CalculateUV(float3 normal, float3 position)
            {
                half maskZ = (1 - abs(normal.x)) * (1 - abs(normal.y)); // for Z faces
                half maskY = (1 - abs(normal.x)) * (1 - abs(normal.z)); // for Y faces
                half maskX = (1 - abs(normal.y)) * (1 - abs(normal.z)); // for X faces

                fixed2 uv;
                uv.x = maskZ * position.x + maskY * position.x + maskX * position.z;
                uv.y = maskZ * position.y + maskY * position.z + maskX * position.y;
                return uv;
            }

            v2f vert (a2v v)
            {
                v2f o;
                o.faceIndex = v.faceIndex;
                o.pos = UnityObjectToClipPos(v.vertex); // Clip space
                half3 worldNormal = UnityObjectToWorldNormal(v.normal); //World space
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; //World space
                o.normal = worldNormal; //World space
                
                // dot product between normal and light direction for
                // standard diffuse (Lambert) lighting
                fixed normalLight = max(SHADOW_MIN, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                
                // factor in the light color
                o.diff = normalLight * _LightColor0;

                // compute shadows data
                TRANSFER_SHADOW(o)

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _TintColor;

                fixed2 uv = CalculateUV(i.normal, i.worldPos);
                uv %= 1.0f;

                // Get the texture based on the face index
                fixed4 texTop = tex2D(_TopTexture, uv) * (i.faceIndex.x == 0 ? 1 : 0); // 0 Top
                fixed4 texSide = tex2D(_SideTexture, uv) * (i.faceIndex.x >= 2 && i.faceIndex.x <= 5 ? 1 : 0); // 2 Left, 3 Right, 4 Front, 5 Back 
                fixed4 texBottom = tex2D(_BottomTexture, uv) * (i.faceIndex.x == 1 ? 1 : 0); // 1 Bottom

                // Combine the textures (only one of them will be non zero)
                col *= texTop + texSide + texBottom;
                
                fixed shadow = SHADOW_ATTENUATION(i);
                // Dont allow shadows to darken the object too much
                shadow = clamp(shadow, SHADOW_MIN, 1);

                fixed4 lighting = i.diff * shadow ; // I could add ambient lighting here (+ i.ambient)
                col *= lighting;
                
                return col;
            }
            ENDCG
        }
        
        // Shadow casting
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}
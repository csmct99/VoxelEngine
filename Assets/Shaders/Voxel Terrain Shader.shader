Shader "Unlit/Voxel_Terrain_Shader"
{
    Properties
    {
        _TintColor ("Color", Color) = (1,1,1,1)
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

            //Const
            //Shadow min factor
            #define SHADOW_MIN 0.2f
            
            // Struct for vertex shader input
            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 diff : COLOR0; // diffuse lighting color
                float4 pos : SV_POSITION;
                SHADOW_COORDS(1) // put shadows data into TEXCOORD1
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // Clip space
                o.uv = v.texcoord;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal); //World space
                
                // dot product between normal and light direction for
                // standard diffuse (Lambert) lighting
                half normalLight = max(SHADOW_MIN, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                
                // factor in the light color
                o.diff = normalLight * _LightColor0;

                // compute shadows data
                TRANSFER_SHADOW(o)

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _TintColor;
                
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
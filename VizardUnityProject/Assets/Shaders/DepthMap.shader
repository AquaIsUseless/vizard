Shader "Custom/DepthMap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 position : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //get depth from depth texture
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                //linearize depth between camera and far clipping plane
                depth = Linear01Depth(depth);
                if (depth == 1.0f)
                {
                    return half4 (1.0f,1.0f,1.0f,1.0f);
                }
                //NOTE: Three channel output is being overwritten by Unity after leaving the fragment shader after updating
                //from Unity 2020.3 to Unity 2022.3, resulting in incorrect depth values calculated from the render texture
                // pixels. Until this is resolved, only the red channel will be used to encode depth.

                // float green = depth*256.0f; 
                // green = (green - (int)green); 
                // float blue = green*256.0f; 
                // blue = (blue - (int)blue);
                //
                // return fixed4 (depth, green,blue,1.0f); //values between 0 and 1
                //return fixed4  (depth, 0, 0, 1.0f);

                float red = (floor(depth * 255.0f))/255.0f;
                //red = depth* 255.0f;
                float green = (floor(frac(depth *255.0f)*255.0f))/255.0f;
                float blue = (floor(frac(frac(depth*255.0f)*255.0f)*255.0f))/255.0f;

                //return fixed4(red,green,blue,1.0);
                return fixed4(red, green, blue,1.0f);
            }
            ENDCG
        }
    }
}

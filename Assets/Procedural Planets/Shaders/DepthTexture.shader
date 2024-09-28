Shader "Hidden/DepthTexture"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D_float _CameraDepthNormalsTexture;

            struct v2f {
               float4 pos : SV_POSITION;
               float4 scrPos : TEXCOORD0;
               float3 normal : NORMAL;
            };

            float _DepthPower;
            float _ShowNormals;

            //Vertex Shader
            v2f vert (appdata_base v){
               v2f o;
               o.pos = UnityObjectToClipPos (v.vertex);
               o.scrPos = ComputeScreenPos(o.pos);
               o.normal = v.normal;
               return o;
            }

            //Fragment Shader
            half4 frag (v2f i) : COLOR
            {
               float3 normals;
               float depth;

               DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.scrPos.xy), depth, normals);
               depth = pow(depth, _DepthPower);

               if(_ShowNormals == 0)
               {
                    return depth;
               }

               if(depth > 0.99)
               {
                    return depth;
               }

               return float4(normals, depth);

            }
        ENDCG
        }
    }
    FallBack "Diffuse"
}

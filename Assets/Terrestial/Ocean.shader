
Shader "Hidden/Ocean"
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


            float2 raySphere(float3 centre, float radius, float3 rayOrigin, float3 rayDir) 
            {
                float3 offset = rayOrigin - centre;
                const float a = 1; // set to dot(rayDir, rayDir) instead if rayDir may not be normalized
                float b = 2 * dot(offset, rayDir);
                float c = dot (offset, offset) - radius * radius;

                float discriminant = b*b-4*a*c;
                // No intersections: discriminant < 0
                // 1 intersection: discriminant == 0
                // 2 intersections: discriminant > 0
                if (discriminant > 0) {
                    float s = sqrt(discriminant);
                    float dstToSphereNear = max(0, (-b - s) / (2 * a));
                    float dstToSphereFar = (-b + s) / (2 * a);

                    if (dstToSphereFar >= 0) {
                        return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
                    }
                }
	             // Ray did not intersect sphere
                return float2(3.402823466e+38, 0);
            }


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 viewVector : TEXCOORD1;
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
				o.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));  
                return o;
            }
            
            sampler2D _CameraDepthTexture;
            sampler2D _MainTex;

            float4 colA;
			float4 colB;
			float depthMultiplier;
			float alphaMultiplier;

			float3 oceanCentre;
			float oceanRadius;
            float planetScale;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 originalCol = tex2D(_MainTex, i.uv); //original color

                float3 rayPos = _WorldSpaceCameraPos; //ray starts from camera
                float viewLength = length(i.viewVector); //length of view vector
                float3 rayDir = i.viewVector / viewLength; 

                float nonLinearDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv); 
                float sceneDepth = LinearEyeDepth(nonLinearDepth) * length(i.viewVector); //depth texture

                float2 hitInfo = raySphere(oceanCentre, oceanRadius, rayPos, rayDir);
                float dstToOcean = hitInfo.x;
                float dstThroughOcean = hitInfo.y;
                float3 rayOceanIntersectPos = rayPos + rayDir * dstToOcean - oceanCentre;

                float oceanViewDepth = min(dstThroughOcean, sceneDepth - dstToOcean);

                if(oceanViewDepth > 0)
                {
                    float opticalDepth = 1 - exp(-oceanViewDepth * depthMultiplier);
                    float alpha = 1 - exp(-oceanViewDepth * alphaMultiplier);
                    float4 oceanCol = lerp(colA, colB, opticalDepth);

                    return lerp(originalCol, oceanCol, alpha);
                }
                return originalCol;
            }
            ENDCG
        }
    }
}

﻿Shader "Hidden/Ocean"
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
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members projPos)
#pragma exclude_renderers d3d11
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
			struct appdata {
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
			};

			struct v2f {
					float4 pos : SV_POSITION;
					float3 projPos : TEXCOORD1;
					float2 uv : TEXCOORD0;
					float3 viewVector : TEXCOORD1;
			};
			
			v2f vert (appdata v) {
					v2f output;
					output.pos = UnityObjectToClipPos(v.vertex);
					output.uv = v.uv;
					return output;
			}

			float4 colA;
			float4 colB;
			float depthMultiplier;
			float alphaMultiplier;

			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;

			float4 _CameraDepthTexture_TexelSize;

			float planetScale;
			float3 oceanCentre;
			float oceanRadius;
			fixed4 frag (v2f i) : SV_Target
			{
	
				fixed4 originalCol = tex2D(_MainTex, i.uv);

				float3 relCameraPos = float3(0,0,_WorldSpaceCameraPos.z);
				relCameraPos.xy = i.pos;
				float3 rayPos = relCameraPos;
				float3 rayDir = float3(0,0,1);

				float nonlin_depth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r;
				float sceneDepth = LinearEyeDepth(nonlin_depth);
				return sceneDepth;
				float2 hitInfo = raySphere(oceanCentre, oceanRadius, rayPos, rayDir);
				float dstToOcean = hitInfo.x;
				float dstThroughOcean = hitInfo.y;
				float3 rayOceanIntersectPos = rayPos + rayDir * dstToOcean - oceanCentre;

				// dst that view ray travels through ocean (before hitting terrain / exiting ocean)
				float oceanViewDepth = min(dstThroughOcean, sceneDepth - dstToOcean);


				if (oceanViewDepth > 0) {
					float3 clipPlanePos = rayPos + i.viewVector * _ProjectionParams.y;

					float dstAboveWater = length(clipPlanePos - oceanCentre) - oceanRadius;

					float t = 1 - exp(-oceanViewDepth / planetScale * depthMultiplier);
					float alpha =  1-exp(-oceanViewDepth / planetScale * alphaMultiplier);
					float4 oceanCol = lerp(colA, colB, t);
					return oceanCol;
				}

				
				return originalCol;
			}
			ENDCG
		}
	}
}
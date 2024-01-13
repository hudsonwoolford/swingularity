sampler2D _CameraDepthTexture, _WaterBackground;
float4 _CameraDepthTexture_TexelSize;
float3 _WaterFogColor;
float _WaterFogDensity;
float _OceanRadius;
float3 _OceanCentre;

float2 raySphere(float3 rayOrigin, float3 rayDir) 
{
    float radius = _OceanRadius;
    float3 centre = _OceanCentre;
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
float3 ColorBelowWater(float3 viewDir, float4 screenPos, float distFromCam, float3 relCameraPos)
{

    float2 uv = screenPos.xy / screenPos.w;
    float2 rayHit = raySphere(relCameraPos, viewDir);
    float throughWaterDepth = rayHit.y;
    float toWaterDist = rayHit.x;
    float vertexSceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));

    float depthDifference = min(throughWaterDepth, vertexSceneDepth - toWaterDist);

    float3 backgroundColor = tex2D(_WaterBackground, uv).rgb;
    float fogFactor = exp2(-_WaterFogDensity * depthDifference);
    return lerp(_WaterFogColor, backgroundColor, fogFactor);
}


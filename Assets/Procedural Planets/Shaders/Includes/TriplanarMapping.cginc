float4 triplanar(float3 vertexPos, float3 normal, float scale, sampler2D tex){
    float2 uvX = vertexPos.zy * scale;
    float2 uvY = vertexPos.xz * scale;
    float2 uvZ = vertexPos.xy * scale;

    //Colours based on each UV readout
    float4 colX = tex2D (tex, uvX);
    float4 colY = tex2D (tex, uvY);
    float4 colZ = tex2D (tex, uvZ);

    float3 blendWeight = normal * normal;
    
    //Normalizing vector back
    blendWeight /= dot(blendWeight, 1);
    return colX * blendWeight.x + colY * blendWeight.y + colZ * blendWeight.z;
}

inline float unity_NoiseTri_randomValue (float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
}

inline float unity_NoiseTri_interpolate (float a, float b, float t)
{
    return (1.0-t)*a + (t*b);
}

inline float unity_ValueNoiseTri (float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    uv = abs(frac(uv) - 0.5);
    float2 c0 = i + float2(0.0, 0.0);
    float2 c1 = i + float2(1.0, 0.0);
    float2 c2 = i + float2(0.0, 1.0);
    float2 c3 = i + float2(1.0, 1.0);
    float r0 = unity_NoiseTri_randomValue(c0);
    float r1 = unity_NoiseTri_randomValue(c1);
    float r2 = unity_NoiseTri_randomValue(c2);
    float r3 = unity_NoiseTri_randomValue(c3);

    float bottomOfGrid = unity_NoiseTri_interpolate(r0, r1, f.x);
    float topOfGrid = unity_NoiseTri_interpolate(r2, r3, f.x);
    float t = unity_NoiseTri_interpolate(bottomOfGrid, topOfGrid, f.y);
    return t;
}


float Unity_SimpleNoise(float2 UV, float Scale)
{
    float t = 0.0;

    float freq = pow(2.0, float(0));
    float amp = pow(0.5, float(3-0));
    t += unity_ValueNoiseTri(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

    freq = pow(2.0, float(1));
    amp = pow(0.5, float(3-1));
    t += unity_ValueNoiseTri(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

    freq = pow(2.0, float(2));
    amp = pow(0.5, float(3-2));
    t += unity_ValueNoiseTri(float2(UV.x*Scale/freq, UV.y*Scale/freq))*amp;

    return t;
}

float4 TriplanarNoise(float3 vertexPos, float3 normal, float scale, float noiseScale){
    float2 uvX = vertexPos.zy * scale;
    float2 uvY = vertexPos.xz * scale;
    float2 uvZ = vertexPos.xy * scale;

    //Colours based on each UV readout
    float4 colX = Unity_SimpleNoise(uvX, noiseScale);
    float4 colY = Unity_SimpleNoise(uvY, noiseScale);
    float4 colZ = Unity_SimpleNoise(uvZ, noiseScale);

    float3 blendWeight = normal * normal;
    
    //Normalizing vector back
    blendWeight /= dot(blendWeight, 1);
    return colX * blendWeight.x + colY * blendWeight.y + colZ * blendWeight.z;
}



float3 ObjectToTangentVector(float4 tangent, float3 normal, float3 objectSpaceVector) {
	float3 normalizedTangent = normalize(tangent.xyz);
	float3 binormal = cross(normal, normalizedTangent) * tangent.w;
	float3x3 rot = float3x3 (normalizedTangent, binormal, normal);
	return mul(rot, objectSpaceVector);
}

// Reoriented Normal Mapping
// http://blog.selfshadow.com/publications/blending-in-detail/
// Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
float3 blend_rnm(float3 n1, float3 n2)
{
	n1.z += 1;
	n2.xy = -n2.xy;

	return n1 * dot(n1, n2) / n1.z - n2;
}

// Sample normal map with triplanar coordinates
// Returned normal will be in obj/world space (depending whether pos/normal are given in obj or world space)
// Based on: medium.com/@bgolus/normal-mapping-for-a-triplanar-shader-10bf39dca05a
float3 triplanarNormal(float3 vertPos, float3 normal, float3 scale, float2 offset, sampler2D normalMap) {
	float3 absNormal = abs(normal);

	// Calculate triplanar blend
	float3 blendWeight = saturate(pow(normal, 4));
	// Divide blend weight by the sum of its components. This will make x + y + z = 1
	blendWeight /= dot(blendWeight, 1);

	// Calculate triplanar coordinates
	float2 uvX = vertPos.zy * scale + offset;
	float2 uvY = vertPos.xz * scale + offset;
	float2 uvZ = vertPos.xy * scale + offset;

	// Sample tangent space normal maps
	// UnpackNormal puts values in range [-1, 1] (and accounts for DXT5nm compression)
	float3 tangentNormalX = UnpackNormal(tex2D(normalMap, uvX));
	float3 tangentNormalY = UnpackNormal(tex2D(normalMap, uvY));
	float3 tangentNormalZ = UnpackNormal(tex2D(normalMap, uvZ));

	// Swizzle normals to match tangent space and apply reoriented normal mapping blend
	tangentNormalX = blend_rnm(half3(normal.zy, absNormal.x), tangentNormalX);
	tangentNormalY = blend_rnm(half3(normal.xz, absNormal.y), tangentNormalY);
	tangentNormalZ = blend_rnm(half3(normal.xy, absNormal.z), tangentNormalZ);

	// Apply input normal sign to tangent space Z
	float3 axisSign = sign(normal);
	tangentNormalX.z *= axisSign.x;
	tangentNormalY.z *= axisSign.y;
	tangentNormalZ.z *= axisSign.z;

	// Swizzle tangent normals to match input normal and blend together
	float3 outputNormal = normalize(
		tangentNormalX.zyx * blendWeight.x +
		tangentNormalY.xzy * blendWeight.y +
		tangentNormalZ.xyz * blendWeight.z
	);

	return outputNormal;
}

float3 triplanarNormalTangentSpace(float3 vertPos, float3 normal, float3 scale, float4 tangent, sampler2D normalMap) {
	float3 textureNormal = triplanarNormal(vertPos, normal, scale, 0, normalMap);
	return ObjectToTangentVector(tangent, normal, textureNormal);
}

// Remap a float value (with a known mininum and maximum) to a value between 0 and 1
float remap01(float v, float minOld, float maxOld) {
    return saturate((v-minOld) / (maxOld-minOld));
}

// Remap a float2 value (with a known mininum and maximum) to a value between 0 and 1
float2 remap01(float2 v, float2 minOld, float2 maxOld) {
    return saturate((v-minOld) / (maxOld-minOld));
}

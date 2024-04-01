struct TriplanarUV{
    float2 x, y, z;
};
float3 GetTriplanarWeights (float3 normal) 
{
	float3 triW = pow(abs(normal),10);
	return triW / (triW.x + triW.y + triW.z);
}
TriplanarUV GetTriplanarUV (float3 pos, float3 mapOffset, float mapScale)
{
    TriplanarUV triUV;
    float3 p = pos * mapScale;
    triUV.x = p.zy + mapOffset.x;
    triUV.y = p.xz + mapOffset.y;
    triUV.z = p.xy + mapOffset.z;
    return triUV;
}

float3 GetMappedTriplanarNormal(sampler2D tex, TriplanarUV triUV, float power, float3 normal)
{
    half4 texX = tex2D(tex, triUV.x);
	half4 texY = tex2D(tex, triUV.y);
	half4 texZ = tex2D(tex, triUV.z);

    float3 normalX = UnpackScaleNormal(texX, power).zyx;
    float3 normalY = UnpackScaleNormal(texY, power).xzy;
    float3 normalZ = UnpackScaleNormal(texZ, power).yxz;

    float3 triW = GetTriplanarWeights(normal);

    return normalize(normalX * triW.x + normalY * triW.y + normalZ * triW.z);
}
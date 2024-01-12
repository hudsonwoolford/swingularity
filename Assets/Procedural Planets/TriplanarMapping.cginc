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

// Remap a float value (with a known mininum and maximum) to a value between 0 and 1
float remap01(float v, float minOld, float maxOld) {
    return saturate((v-minOld) / (maxOld-minOld));
}

// Remap a float2 value (with a known mininum and maximum) to a value between 0 and 1
float2 remap01(float2 v, float2 minOld, float2 maxOld) {
    return saturate((v-minOld) / (maxOld-minOld));
}

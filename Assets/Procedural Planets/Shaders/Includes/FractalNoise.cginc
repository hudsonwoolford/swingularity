#include "SimplexNoise.cginc"

float SimpleNoise(float3 position, int octaves, float scale, float persistence, float lacunarity, float multiplier, float gain, float3 offset)
{
    float noiseSum = 0;
    float amplitude = 1;
    float frequency = scale;
    float weight = 1;
    
    for (int i = 0; i < octaves; i++)
    {
        float noiseVal = snoise(position * frequency + offset) * .5 + .5;
        noiseVal *= weight;
        weight = saturate(noiseVal * gain);
        
        noiseSum += noiseVal * amplitude;
        amplitude *= persistence;
        frequency *= lacunarity;
    }
    
    return noiseSum * multiplier;

}

float SimpleNoise(float3 position, int octaves, float scale, float persistence, float lacunarity, float gain, float3 offset)
{
    float noiseSum = 0;
    float amplitude = 1;
    float frequency = scale;
    float weight = 1;
    
    for (int i = 0; i < octaves; i++)
    {
        float noiseVal = snoise(position * frequency + offset) * .5 + .5;
        noiseVal *= weight;
        weight = saturate(noiseVal * gain);
        
        noiseSum += noiseVal * amplitude;
        amplitude *= persistence;
        frequency *= lacunarity;
    }
    
    return noiseSum;

}


float RidgeNoise(float3 position, int octaves, float scale, float persistence, float lacunarity, float multiplier, float power, float gain, float3 offset, float smoothing)
{
    float noiseSum = 0;
    float amplitude = 1;
    float frequency = scale;
    float weight = 1;
    
    for (int i = 0; i < octaves; i++)
    {
        float noiseVal = 1 - abs(snoise(position * frequency + offset));
        noiseVal = pow(abs(noiseVal), power);
        noiseVal *= weight;
        weight = saturate(noiseVal * gain);
        
        noiseSum += noiseVal * amplitude;
        amplitude *= persistence;
        frequency *= lacunarity;
    }
    
    return noiseSum * multiplier;
}

float SmoothedRidgeNoise(float3 position, int octaves, float scale, float persistence, float lacunarity, float multiplier, float power, float gain, float3 offset, float smoothing)
{
    float3 sphereNormal = normalize(position);
    float3 axisA = cross(sphereNormal, float3(0, 1, 0));
    float3 axisB = cross(sphereNormal, axisA);
    
    //Averages samples of noise from different directions along the sphere
    float offsetDst = smoothing * 0.01;
    float sample0 = RidgeNoise(position, octaves, scale, persistence, lacunarity, multiplier, power, gain, offset, smoothing);
    float sample1 = RidgeNoise(position - axisA * offsetDst, octaves, scale, persistence, lacunarity, multiplier, power, gain, offset, smoothing);
    float sample2 = RidgeNoise(position + axisA * offsetDst, octaves, scale, persistence, lacunarity, multiplier, power, gain, offset, smoothing);
    float sample3 = RidgeNoise(position - axisB * offsetDst, octaves, scale, persistence, lacunarity, multiplier, power, gain, offset, smoothing);
    float sample4 = RidgeNoise(position + axisB * offsetDst, octaves, scale, persistence, lacunarity, multiplier, power, gain, offset, smoothing);
    return (sample0 + sample1 + sample2 + sample3 + sample4) / 5;
}

float3 WarpedNoise(float3 position, int octaves, float scale, float persistence, float lacunarity, float multiplier, float gain, float3 offset)
{
    float x = SimpleNoise(position, octaves, scale, persistence, lacunarity, multiplier, gain, offset);
    float y = SimpleNoise(position, octaves, scale, persistence, lacunarity, multiplier, gain, offset);
    float z = SimpleNoise(position, octaves, scale, persistence, lacunarity, multiplier, gain, offset);
    return float3(x, y, z);
}

float ContinentNoise(float3 position, int octaves, float scale, float persistence, float lacunarity, float multiplier, float gain, float3 offset)
{
    float noiseSum = 0;
    float amplitude = 1;
    float frequency = scale;
    float weight = 1;
    
    for (int i = 0; i < octaves; i++)
    {
        float noiseVal = snoise(position * frequency + offset);
        noiseVal *= weight;
        weight = saturate(noiseVal * gain);
        
        noiseSum += noiseVal * amplitude;
        amplitude *= persistence;
        frequency *= lacunarity;
    }
    
    return noiseSum * multiplier;

}

float MaskNoise(float3 position, int octaves, float scale, float persistence, float lacunarity, float multiplier, float gain, float3 offset, float verticalShift)
{
    float noiseSum = 0;
    float amplitude = 1;
    float frequency = scale;
    float weight = 1;
    
    for (int i = 0; i < octaves; i++)
    {
        float noiseVal = snoise(position * frequency + offset);
        noiseVal *= weight;
        weight = saturate(noiseVal * gain);
        
        noiseSum += noiseVal * amplitude;
        amplitude *= persistence;
        frequency *= lacunarity;
    }
    
    return noiseSum * multiplier + verticalShift;

}

//3D Voronoi referenced from https://www.shadertoy.com/view/flSGDK
float hash(float x) { return frac(x + 1.3215 * 1.8152); }

float hash3(float3 a) { return frac((hash(a.z * 42.8883) + hash(a.y * 36.9125) + hash(a.x * 65.4321)) * 291.1257); }

float3 rehash3(float x) { return float3(hash(((x + 0.5283) * 59.3829) * 274.3487), hash(((x + 0.8192) * 83.6621) * 345.3871), hash(((x + 0.2157f) * 36.6521f) * 458.3971f)); }

float sqr(float x) {return x*x;}
float fastdist(float3 a, float3 b) { return sqr(b.x - a.x) + sqr(b.y - a.y) + sqr(b.z - a.z); }

void voronoi3D_float(float3 pos, float density, float exponent, out float Out, out float Cells) {
    float4 p[27];
	pos *= density;
    float x = pos.x;
	float y = pos.y;
	float z = pos.z;
    for (int _x = -1; _x < 2; _x++) for (int _y = -1; _y < 2; _y++) for(int _z = -1; _z < 2; _z++) {
        float3 _p = float3(floor(x), floor(y), floor(z)) + float3(_x, _y, _z);
        float h = hash3(_p);
        p[(_x + 1) + ((_y + 1) * 3) + ((_z + 1) * 3 * 3)] = float4((rehash3(h) + _p).xyz, h);
    }
    float m = 9999.9999, w = 0.0;
    for (int i = 0; i < 27; i++) {
        float3 diff = abs(float3(x, y, z) - p[i].xyz);
        float d = pow(diff.x, exponent) + pow(diff.y, exponent) + pow(diff.z, exponent);
        d = pow(d, 1.0 / exponent);
        if(d < m) { m = d; w = p[i].w; }
    }
	
    Out = m;
	Cells = w;
}

void voronoi3D_float(float3 pos, float density, out float Out, out float Cells) {
    float4 p[27];
	pos *= density;
    float x = pos.x;
	float y = pos.y;
	float z = pos.z;
    for (int _x = -1; _x < 2; _x++) for (int _y = -1; _y < 2; _y++) for(int _z = -1; _z < 2; _z++) {
        float3 _p = float3(floor(x), floor(y), floor(z)) + float3(_x, _y, _z);
        float h = hash3(_p);
        p[(_x + 1) + ((_y + 1) * 3) + ((_z + 1) * 3 * 3)] = float4((rehash3(h) + _p).xyz, h);
    }
    float m = 9999.9999, w = 0.0;
    for (int i = 0; i < 27; i++) {
        float d = fastdist(float3(x, y, z), p[i].xyz);
        if(d < m) { m = d; w = p[i].w; }
    }
	
    Out = m;
	Cells = w;
}



float Blend(float startHeight, float blendDst, float height)
{
    return smoothstep(startHeight - blendDst / 2, startHeight + blendDst / 2, height);
}

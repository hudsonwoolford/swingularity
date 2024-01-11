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


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "terainVar", menuName = "terrainVar")]
[Serializable]
public class TerrainVar : ScriptableObject
{
    public GeneralSettings generalSettings;
    public CraterSettings craterSettings;
    public ShapeNoiseSettings shapeNoiseSettings;
    public DetailNoiseSettings detailNoiseSettings;
    public RidgeNoiseSettings ridgeNoiseSettings;
    public WarpedNoiseSettings warpedNoiseSettings;
    public ContinentNoiseSettings continentNoiseSettings;
    public BiomeNoiseSettings biomeNoiseSettings;
    public BiomeWarpNoiseSettings biomeWarpNoiseSettings;
}
[Serializable]
public class GeneralSettings
{
    [Range(3, 9)]
    public int subdivisions = 0;

    public int seed;
}
[Serializable]
public class CraterSettings
{
    [Range(0f, 1f)]
    public float rimWidth;

    [Range(0f, 1f)]
    public float rimSteepness;

    [Range(0f, 1f)]
    public float smoothness;

    [Range(0f, 1f)]
    public float craterBias;

    [Range(0f, 0.75f)]
    public float minCraterSize;

    [Range(0f, 1f)]
    public float maxCraterSize;

    [Range(0f, 5000f)]
    public int numCraters;
}

[Serializable]
public class ShapeNoiseSettings
{
    public bool applyNoise;
    public int octaves;
    public float scale;
    public float persistence;
    public float lacunarity;
    public float multiplier;
    public float gain;
    public float[] offset = new float[3];
    public float elevation;
}

[Serializable]
public class DetailNoiseSettings
{
    public bool applyNoise;
    public int octaves;
    public float scale;
    public float persistence;
    public float lacunarity;
    public float multiplier;
    public float gain;
    public float[] offset = new float[3];
    public float elevation;
}

[Serializable]
public class RidgeNoiseSettings
{
    public bool applyNoise;
    public int octaves;
    public float scale;
    public float persistence;
    public float lacunarity;
    public float multiplier;
    public float gain;
    public float power;
    public float[] offset = new float[3];
    public float elevation;
    public float smoothing;
}

[Serializable]
public class WarpedNoiseSettings
{
    public bool applyNoise;
    public int octaves;
    public float scale;
    public float persistence;
    public float lacunarity;
    public float multiplier;
    public float gain;
    public float[] offset = new float[3];
    public float strength;
}

[Serializable]
public class ContinentNoiseSettings
{
    public bool applyNoise;
    public int octaves;
    public float scale;
    public float persistence;
    public float lacunarity;
    public float multiplier;
    public float gain;
    public float[] offset = new float[3];
    public float depthMultiplier;
    public float oceanFloorDepth;
    public float oceanFloorSmoothing;
    public float maskBlend;
}

[Serializable]
public class BiomeNoiseSettings
{
    public int _Octaves;
    public Vector3 _Offset;
    public float _Scale, _Persistence, _Lacunarity, _Multiplier, _Gain;
}

[Serializable]
public class BiomeWarpNoiseSettings
{
    public int _WarpOctaves;
    public Vector3 _WarpOffset;
    public float _WarpScale, _WarpPersistence, _WarpLacunarity, _WarpMultiplier, _WarpGain;
}




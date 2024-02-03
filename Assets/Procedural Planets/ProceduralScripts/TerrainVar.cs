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
    public int subdivisions = 8;

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
    public int octaves = 3;
    public float scale = 0.3f;
    public float persistence;
    public float lacunarity;
    public float multiplier = 1;
    public float gain = 1;
    public float power = 1;
    public float[] offset = new float[3];
    public float elevation;
    public float smoothing;
}

[Serializable]
public class WarpedNoiseSettings
{
    public bool applyNoise;
    public int octaves = 3;
    public float scale = 1;
    public float persistence;
    public float lacunarity;
    public float multiplier = 1;
    public float gain = 1;
    public float[] offset = new float[3];
    public float strength = 1;
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
    public int _Octaves = 3;
    public Vector3 _Offset;
    public float _Scale = 1, _Persistence, _Lacunarity, _Multiplier = 1, _Gain = 1;
}

[Serializable]
public class BiomeWarpNoiseSettings
{
    public int _WarpOctaves = 3;
    public Vector3 _WarpOffset;
    public float _WarpScale = 1, _WarpPersistence, _WarpLacunarity, _WarpMultiplier = 1, _WarpGain = 1;
}




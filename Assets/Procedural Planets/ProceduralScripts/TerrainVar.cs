using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "terainVar", menuName = "terrainVar")]
[Serializable]
public class TerrainVar : ScriptableObject
{
    public ColorSchemePools colorSchemePools;
    public GeneralSettings generalSettings;
    public CraterSettings craterSettings;
    public ShapeNoiseSettings shapeNoiseSettings;
    public DetailNoiseSettings detailNoiseSettings;
    public RidgeNoiseSettings ridgeNoiseSettings;
    public WarpedNoiseSettings warpedNoiseSettings;
    public ContinentNoiseSettings continentNoiseSettings;
    public MaskNoiseSettings maskNoiseSettings;
    public BiomeNoiseSettings biomeNoiseSettings;
    public BiomeWarpNoiseSettings biomeWarpNoiseSettings;
    public BiomeDetailNoiseSettings biomeDetailNoiseSettings;
    public PolarBiomeSettings polarBiomeSettings;
}
[Serializable]
public class GeneralSettings
{
    [Range(3, 9)]
    public int subdivisions = 8;

    public int seed;

    public bool ocean = true;

    [Range(0, 5)]
    public float _SeaLevel = 1;
}
[Serializable]
public class CraterSettings
{
    [Range(-1f, 5f)]
    public float rimWidth;

    [Range(-1f, 1f)]
    public float floorHeight;

    [Range(-1f, 1f)]
    public float rimSteepness;

    [Range(-1f, 1f)]
    public float smoothness;

    [Range(-1f, 1f)]
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
}

[Serializable]
public class MaskNoiseSettings
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
    public float maskBlend;
    public float maskVerticalShift;
}

[Serializable]
public class BiomeNoiseSettings
{
    public bool randomizeNoise = true;
    public int _Octaves = 3;
    public Vector3 _Offset;
    public float _Scale = 1, _Persistence, _Lacunarity, _Multiplier = 1, _Gain = 1;
}

[Serializable]
public class BiomeWarpNoiseSettings
{
    public bool randomizeNoise = true;
    public int _WarpOctaves = 3;
    public Vector3 _WarpOffset;
    public float _WarpScale = 1, _WarpPersistence, _WarpLacunarity, _WarpMultiplier = 1, _WarpGain = 1;
}

[Serializable]
public class BiomeDetailNoiseSettings
{
    public int _Octaves = 3;
    public Vector3 _Offset;
    public float _Scale = 1, _Persistence, _Lacunarity, _Multiplier = 1, _Gain = 1;
}

[Serializable]
public class PolarBiomeSettings
{
    public float _PolarHeight, _PolarBlendIntensity;
}

[Serializable]
public static class ColorSchemeStaticReference{
    [Serializable]
    public struct ColorSchemePool
    {
        public string name; //name of the pool
        public bool oPrimary, aPrimary; //ocean variables or atmosphere variables are taken from primary biome
        [Tooltip("This is optional")]
        public PlanetColorScheme primaryBiome; //the primary biome
        public PlanetColorScheme[] planetColorSchemes; //array of color schemes
        public PlanetColorScheme[] polarColorSchemes; //array of color schemes
    }
}

[Serializable]
public class ColorSchemePools
{   
    public ColorSchemeStaticReference.ColorSchemePool[] colorSchemePools;
}




using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "planetVar", menuName = "PlanetVar")]
[Serializable]
public class PlanetVar : ScriptableObject
{
    [Serializable]
    public class GeneralSettings
    {
        [Range(3, 9)]
        public int subdivisions = 0;

        [Range(0.05f, 100f)]
        public float radius = 1f;
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

    public GeneralSettings generalSettings;
    public CraterSettings craterSettings;
    public ShapeNoiseSettings shapeNoiseSettings;
    public DetailNoiseSettings detailNoiseSettings;
    public RidgeNoiseSettings ridgeNoiseSettings;
    public WarpedNoiseSettings warpedNoiseSettings;
    public ContinentNoiseSettings continentNoiseSettings;
}

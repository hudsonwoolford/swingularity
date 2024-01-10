using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "planetVar", menuName = "PlanetVar")]
public class PlanetVar : ScriptableObject
{
    [Serializable]
    public class GeneralSettings
    {
        [Range(3, 8)]
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

        [Range(-1f, 0.5f)]
        public float floorHeight;

        [Range(0f, 1f)]
        public float craterBias;

        [Range(0f, 0.75f)]
        public float minCraterSize;

        [Range(0f, 1f)]
        public float maxCraterSize;

        [Range(0f, 1000f)]
        public int numCraters;
    }
    public GeneralSettings generalSettings;
    public CraterSettings craterSettings;
}

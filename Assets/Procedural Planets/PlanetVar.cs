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
        public int subdivisions = 0;
        public float radius = 1f;
        public int seed;
    }
    [Serializable]
    public class CraterSettings
    {
        public float rimWidth, rimSteepness, smoothness, floorHeight, craterBias;
        public int numCraters;
    }
    public GeneralSettings generalSettings;
    public CraterSettings craterSettings;
}

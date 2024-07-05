using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyController : MonoBehaviour
{
    [Serializable]
    public struct GalaxyIcon{
        public ColorSchemeStaticReference.PlanetType planetType;
        public Color[] possibleColors;
    }
    
    public GalaxyParticleController stars, nebula;
    public int seed;
    public GalaxyIcon[] galaxyIcons;
    public int galaxySchemeIndex;

    void Update(){
            ApplyGalaxyIconScheme();
    }
    void ApplyGalaxyIconScheme(){
        UnityEngine.Random.InitState(seed);
        Color[] cols = galaxyIcons[galaxySchemeIndex].possibleColors;
        stars.col1 = cols[UnityEngine.Random.Range(0, cols.Length)];
        UnityEngine.Random.InitState(seed + 1);
        stars.col2 = cols[UnityEngine.Random.Range(0, cols.Length)];

        UnityEngine.Random.InitState(seed + 2);
        Color nebulaCol1 = cols[UnityEngine.Random.Range(0, cols.Length)];
        UnityEngine.Random.InitState(seed + 3);
        Color nebulaCol2 = cols[UnityEngine.Random.Range(0, cols.Length)];
        nebulaCol1.a = 1f/255f;
        nebulaCol2.a = 1f/255f;

        nebula.col1 = nebulaCol1;
        nebula.col2 = nebulaCol2;

        stars.Init(seed);
        nebula.Init(seed);
    }
}

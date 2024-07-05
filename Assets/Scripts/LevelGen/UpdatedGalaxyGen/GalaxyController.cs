using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyController : MonoBehaviour
{
    [Serializable]
    public struct GalaxyIcon{
        public ColorSchemeStaticReference.PlanetType planetType;
        public Color[] possibleStarColors;
        public Color[] possibleNebulaColors;
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
        Color[] starCols = galaxyIcons[galaxySchemeIndex].possibleStarColors;

        List<Color> starColsList= new List<Color>();

        for (int i = 0; i < starCols.Length; i++){
            starColsList.Add(starCols[i]);
        }
        
        stars.col1 = starColsList[UnityEngine.Random.Range(0, starColsList.Count)];
        
        starColsList.Remove(stars.col1);
        UnityEngine.Random.InitState(seed + 1);
        stars.col2 = starColsList[UnityEngine.Random.Range(0, starColsList.Count)];


        UnityEngine.Random.InitState(seed + 2);

        Color[] nebulaCols = galaxyIcons[galaxySchemeIndex].possibleNebulaColors;

        List<Color> nebulaColsList= new List<Color>();

        for (int i = 0; i < nebulaCols.Length; i++){
            nebulaColsList.Add(nebulaCols[i]);
        }
        
        nebula.col1 = nebulaColsList[UnityEngine.Random.Range(0, nebulaColsList.Count)];
        
        nebulaColsList.Remove(nebula.col1);
        UnityEngine.Random.InitState(seed + 3);
        nebula.col2 = nebulaColsList[UnityEngine.Random.Range(0, nebulaColsList.Count)];


        stars.Init(seed);
        nebula.Init(seed);
    }
}

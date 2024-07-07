using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyController : MonoBehaviour
{
    [Serializable]
    public struct GalaxyIcon
    {
        public string name;
        public ColorSchemeStaticReference.PlanetType type;
        public Color[] possibleStarColors;
        public Color[] possibleNebulaColors;
    }
    [SerializeField] GalaxyIcon[] galaxyIcons;
    [SerializeField] int galaxySchemeIndex;
    [SerializeField] int seed;
    [SerializeField] GalaxyParticleController stars, nebula;

    void Start()
    {
        ApplyGalaxyIconScheme();
    }
    void Update()
    {
    }
    void ApplyGalaxyIconScheme()
    {
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
    public void SetProceduralScheme(LevelManager.GalaxyParams galaxyParams)
    {
        for (int i = 0; i < galaxyIcons.Length; i++)
        {
            for (int x = 0; x < galaxyParams.possibleTypes.Length; x++)
            {
                if (galaxyIcons[i].type == galaxyParams.possibleTypes[x])
                {
                    galaxySchemeIndex = i;
                }
            }
        }
        seed = galaxyParams.seed;
    }
    public GalaxyIcon SetScheme(ColorSchemeStaticReference.PlanetType type)
    {
        for (int i = 0; i < galaxyIcons.Length; i++)
        {
            if (galaxyIcons[i].type == type)
            {
                galaxySchemeIndex = i;
                return galaxyIcons[galaxySchemeIndex];
            }
        }
        return galaxyIcons[galaxySchemeIndex];
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class ProceduralLevelGeneration : MonoBehaviour
{
    public struct Level{
        float numPlanets;
        float[] planetSizes;
        Vector3[] planetPoints;
        Vector3[] planetSeeds;
        public Level(float numPlanets, float[] planetSizes, Vector3[] planetPoints, Vector3[] planetSeeds){
            this.numPlanets = numPlanets;
            this.planetSizes = planetSizes;
            this.planetPoints = planetPoints;
            this.planetSeeds = planetSeeds;
        }
    }

    private static float ApplyBias(float input, float bias)
    {
        float k = Mathf.Pow(1 - bias, 3);
        return (input * k) / (input * k - input + 1);
    }
    public static Level CalculateLevel(int seed, params float[] levelGenSettings){
        UnityEngine.Random.InitState(seed);
        float minPlanets = levelGenSettings[0];
        float maxPlanets = levelGenSettings[1];

        float numPlanets = UnityEngine.Random.Range(minPlanets, maxPlanets);

        float[] planetSizes = new float[Mathf.RoundToInt(numPlanets)];
        float minSize = levelGenSettings[2];
        float maxSize = levelGenSettings[3];
        float bias = levelGenSettings[4];
        for (int i = 0; i < numPlanets; i++){
            planetSizes[i] = Mathf.Lerp(minSize, maxSize, ApplyBias(UnityEngine.Random.value, bias));
        }
        
        Vector3[] planetPoints = new Vector3[Mathf.RoundToInt(numPlanets)];
        Vector3[] planetSeeds = new Vector3[Mathf.RoundToInt(numPlanets)];
            
        return new Level(numPlanets, planetSizes, planetPoints, planetSeeds);
    }
}

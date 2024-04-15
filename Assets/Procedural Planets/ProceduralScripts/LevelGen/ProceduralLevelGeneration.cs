using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineInternal;

public static class ProceduralLevelGeneration
{
    [Serializable]
    public struct Level{
        public int numPlanets;
        public float[] planetSizes;
        public Vector3[] planetPoints;
        public Vector3[] planetSeeds;
        public ColorSchemeStaticReference.PlanetType[] planetTypes;
        public Vector2 levelSize;
        public Level(int numPlanets, float[] planetSizes, Vector3[] planetPoints, Vector3[] planetSeeds,  ColorSchemeStaticReference.PlanetType[] planetTypes, Vector2 levelSize){
            this.numPlanets = numPlanets;
            this.planetSizes = planetSizes;
            this.planetPoints = planetPoints;
            this.planetSeeds = planetSeeds;
            this.planetTypes = planetTypes;
            this.levelSize = levelSize;
        }
    }

    private static float ApplyBias(float input, float bias)
    {
        float k = Mathf.Pow(1 - bias, 3);
        return (input * k) / (input * k - input + 1);
    }
    /*Level Gen settings
    0: minimum number of planets
    1: maximum number of planets
    2: minimum size for each planet
    3: maximum size for each planet
    4: bias towards or against planet size
    5: radius for poisson disk algorithm
    6: x dimension of size of level
    7: y dimension of size of level
    8: number of poisson disk samples before rejecting the point as a spawner
    9: offset scale for determining random values of planet
    */
    public static Level CalculateLevel(int seed, ColorSchemeStaticReference.PlanetType[] possiblePlanetTypes,  params float[] levelGenSettings){
        //Set seed and init variables
        UnityEngine.Random.InitState(seed);
        float minPlanets = levelGenSettings[0];
        float maxPlanets = levelGenSettings[1];

        int numPlanets = (int)UnityEngine.Random.Range(minPlanets, maxPlanets);

        float[] planetSizes = new float[numPlanets];
        float minSize = levelGenSettings[2];
        float maxSize = levelGenSettings[3];
        float bias = levelGenSettings[4];
        //Loop through planets and assign a size
        for (int i = 0; i < numPlanets; i++){
            planetSizes[i] = Mathf.Lerp(minSize, maxSize, ApplyBias(UnityEngine.Random.value, bias));
        }

        float minRadius = levelGenSettings[5];
        Vector2 regionBounds = new Vector2 (levelGenSettings[6], levelGenSettings[7]);
        int numSamplesBeforeRejection = (int)levelGenSettings[8];
        Vector3[] planetPoints = new Vector3[numPlanets];

        planetPoints = PoissonDiskSampling(seed, minRadius, regionBounds, numSamplesBeforeRejection);

        Vector3[] planetSeeds = new Vector3[numPlanets];
        float offsetScale = levelGenSettings[9];
        for (int i = 0; i < numPlanets; i++){
            float xOffset = UnityEngine.Random.value * offsetScale + i * offsetScale;
            float yOffset = UnityEngine.Random.value * offsetScale + 2 * i * offsetScale;
            float zOffset = UnityEngine.Random.value * offsetScale + 3 * i * offsetScale;
            planetSeeds[i] = new Vector3(xOffset, yOffset, zOffset);
        }

        ColorSchemeStaticReference.PlanetType[] planetTypes = new ColorSchemeStaticReference.PlanetType[numPlanets];        
        for (int i = 0; i < numPlanets; i++){
            int index = UnityEngine.Random.Range(0, possiblePlanetTypes.Length);
            planetTypes[i] = possiblePlanetTypes[index];
        }
    
        return new Level(numPlanets, planetSizes, planetPoints, planetSeeds, planetTypes, regionBounds);
    }

    public static Vector3[] PoissonDiskSampling(int seed, float radius, Vector2 regionBounds, int numSamplesBeforeRejection){
        //Cell diagonal is same length as radius
        float cellSize = radius/Mathf.Sqrt(2);
        int[,] grid = new int[Mathf.CeilToInt(regionBounds.x/cellSize), Mathf.CeilToInt(regionBounds.y/cellSize)];

        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        spawnPoints.Add(regionBounds/2);

        UnityEngine.Random.InitState(seed);

        while (spawnPoints.Count > 0){
            int spawnIndex = UnityEngine.Random.Range(0, spawnPoints.Count);

            Vector2 spawnCentre = spawnPoints[spawnIndex];
            bool candidateAccepted = false;
            for (int i = 0; i < numSamplesBeforeRejection; i++){
                float angle = UnityEngine.Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), MathF.Cos(angle));
                Vector2 candidate = spawnCentre + dir * UnityEngine.Random.Range(radius, 2*radius);

                if (PointIsValid(candidate, regionBounds, cellSize, radius, points, grid)){
                    //Add point to the lists
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    //Set grid square to include that point
                    grid[(int)(candidate.x/cellSize), (int)(candidate.y/cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }   
            }

            if (!candidateAccepted){
                spawnPoints.RemoveAt(spawnIndex);
            }
        }
        Vector3[] pointsArray = VectorConversion(points.ToArray());

        return pointsArray;
    }

    public static bool PointIsValid(Vector2 candidate, Vector2 regionBounds, float cellSize, float radius, List<Vector2> points, int [,] grid){
        //First check that the point is within bounds
        if (candidate.x >= 0 && candidate.y >= 0 && candidate.x < regionBounds.x && candidate.y < regionBounds.y){
            //Calculate 5x5 bounds around point (maximum distance for intersection)
            int cellX = (int)(candidate.x/cellSize);
            int cellY = (int)(candidate.y/cellSize);
            int searchStartX = Mathf.Max(0, cellX-2);
            int searchEndX = Mathf.Min(cellX+2, grid.GetLength(0)-1);
            int searchStartY = Mathf.Max(0, cellY-2);
            int searchEndY = Mathf.Min(cellY+2, grid.GetLength(1)-1);
            //Iterate through each grid point in the 5x5 area
            for (int x = searchStartX; x <= searchEndX; x++){
                for (int y = searchStartY; y <= searchEndY; y++){
                    //If grid[x,y]=0 i.e. no point, then grid[x,y] = -1
                    int pointIndex = grid[x,y]-1;
                    if (pointIndex != -1){
                        //Calculate the candidate distance from the nearby point
                        float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
                        if (sqrDst < radius*radius){
                            return false;
                        }
                    }
                }  
            }
            return true;
        }
        return false;
    }

    public static Vector3[] VectorConversion (Vector2[] vector2Array) {
        Vector3[] vector3Array = new Vector3[vector2Array.Length];
        for (int i = 0; i < vector2Array.Length; i++)
        {
            vector3Array[i] = new Vector3(vector2Array[i].x, vector2Array[i].y, 0);
        }
        return vector3Array;
    }
}

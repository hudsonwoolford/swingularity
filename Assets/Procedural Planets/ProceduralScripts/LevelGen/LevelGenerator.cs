using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

    /*Level Gen params settings
    0: minimum number of planets
    1: maximum number of planets
    2: minimum size for each planet
    3: maximum size for each planet
    4: bias towards or against planet size
    5: x dimension of size of level
    6: y dimension of size of level
    7: number of poisson disk samples before rejecting the point as a spawner
    8: offset scale for determining random values of planet
    */
    public int seed;
    //Level gen params
    float minPlanets, maxPlanets, minSize, maxSize, sizeBias, xRegionSize, yRegionSize, samplesBeforeRejection, offsetScale;

    void Start()
    {
        float[] levelGenSettings = new float[9]{
            minPlanets,
            maxPlanets,
            minSize,
            maxSize,
            sizeBias,
            xRegionSize,
            yRegionSize,
            samplesBeforeRejection,
            offsetScale
        };
        ProceduralLevelGeneration.Level level = ProceduralLevelGeneration.CalculateLevel(seed, levelGenSettings);
        Generatelevel(level);
    }
    void Generatelevel(ProceduralLevelGeneration.Level level){
        for (int i = 0; i < level.numPlanets; i++){
            
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonTest : MonoBehaviour
{
    public float minRadius = 1;
    public float maxRadius;
    public float sizeBias;
    public float displayRadiusScale = 1;
    public Vector2 regionSize = Vector2.one;
    public int rejectionSamples = 30;
    public int seed;

    (Vector3, float)[] spheres;
    void OnValidate(){
       spheres = ProceduralLevelGeneration.PoissonDiskSampling(seed, minRadius, maxRadius, sizeBias,regionSize, rejectionSamples, 21470000);
    }

    void OnDrawGizmos(){
        if (spheres != null){
            foreach ((Vector3, float) point in spheres){
                Gizmos.DrawSphere(point.Item1, displayRadiusScale * point.Item2);
            }
        }
    }
}

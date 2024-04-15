using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonTest : MonoBehaviour
{
    public float radius = 1;
    public float displayRadius = 1;
    public Vector2 regionSize = Vector2.one;
    public int rejectionSamples = 30;
    public int seed;

    Vector3[] points;
    void OnValidate(){
       points = ProceduralLevelGeneration.PoissonDiskSampling(seed, radius, regionSize, rejectionSamples);
    }

    void OnDrawGizmos(){
        if (points != null){
            foreach (Vector3 point in points){
                Gizmos.DrawSphere(point, displayRadius);
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class OctahedronSphereTester : MonoBehaviour
{
    public FullPlanetObj fullPlanet;
    public ComputeShader calculateVertexHeight;
    private void Update()
    {
        GetComponent<MeshFilter>().mesh = OctahedronSphereCreator.Create(fullPlanet.terrainVar, calculateVertexHeight);
    }
}

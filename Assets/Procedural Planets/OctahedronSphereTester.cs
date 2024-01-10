using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class OctahedronSphereTester : MonoBehaviour
{
    public CraterSettings craterSettings = new CraterSettings();
    public GeneralSettings generalSettings = new GeneralSettings();
    public ComputeShader calculateVertexHeight;
    private void Update()
    {
        SetComputeShaderValues();
        GetComponent<MeshFilter>().mesh = OctahedronSphereCreator.Create(generalSettings.subdivisions, generalSettings.radius, generalSettings.seed, craterSettings.numCraters, craterSettings.craterBias, calculateVertexHeight);
    }

    private void SetComputeShaderValues()
    {
        calculateVertexHeight.SetFloat("rimWidth", craterSettings.rimWidth);
        calculateVertexHeight.SetFloat("rimSteepness", craterSettings.rimSteepness);
        calculateVertexHeight.SetFloat("smoothness", craterSettings.smoothness);
        calculateVertexHeight.SetFloat("floorHeight", craterSettings.floorHeight);
        calculateVertexHeight.SetFloat("numCraters", craterSettings.numCraters);
    }
}

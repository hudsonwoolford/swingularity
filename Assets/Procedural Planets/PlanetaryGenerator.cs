using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static PlanetVar;
using static UnityEngine.EventSystems.EventTrigger;

public class PlanetaryGenerator : MonoBehaviour
{
    public struct Crater
    {
        public float radius;
        public Vector3 centre;
        public float floorHeight;
    };
    public static Mesh ModifyGeometry(Mesh mesh, PlanetVar planetVar, ComputeShader calculateVertexHeight)
    {
        Vector3[] vertices = mesh.vertices;
        vertices = AdjustTerrain(vertices, GenerateRandomCraters(vertices, planetVar), planetVar, calculateVertexHeight);
        mesh.vertices = vertices;
        return mesh;
    }
    #region CraterGeneration
    private static Crater[] GenerateRandomCraters(Vector3[] vertices, PlanetVar planetVar)
    {
        Crater[] craters = new Crater[planetVar.craterSettings.numCraters];
        UnityEngine.Random.InitState(planetVar.generalSettings.seed);
        for (int i = 0; i < planetVar.craterSettings.numCraters; i++)
        {
            float radius = Mathf.Lerp(planetVar.craterSettings.minCraterSize, planetVar.craterSettings.maxCraterSize, ApplyBias(UnityEngine.Random.value, planetVar.craterSettings.craterBias));
            Vector3 centre = UnityEngine.Random.onUnitSphere;
            float floorHeight = UnityEngine.Random.Range(-0.25f, 0f);
            craters[i].radius = radius;
            craters[i].centre = centre;
            craters[i].floorHeight = floorHeight;
        }
        return craters;
    }

    private static float ApplyBias(float input, float bias)
    {
        float k = Mathf.Pow(1 - bias, 3);
        return (input * k) / (input * k - input + 1);
    }
    #endregion

    #region HeightComputation
    private static Vector3[] AdjustTerrain(Vector3[] vertices, Crater[] craters, PlanetVar planetVar, ComputeShader calculateVertexHeight)
    {
        //Init compute buffers for use with compute shader
        ComputeBuffer vertexBuffer = new ComputeBuffer(vertices.Length, 12);
        vertexBuffer.SetData(vertices);
        ComputeBuffer heightBuffer = new ComputeBuffer(vertices.Length, 4);
        float[] heights = new float[vertices.Length];
        heightBuffer.SetData(heights);

        SetShaderParams(calculateVertexHeight, planetVar);


        if (planetVar.craterSettings.numCraters > 0)
        {
            ComputeBuffer craterBuffer;
            craterBuffer = new ComputeBuffer(craters.Length, 20);
            craterBuffer.SetData(craters);

            vertices = ApplyComputeHeights(calculateVertexHeight, vertexBuffer, heightBuffer, craterBuffer);
        }
        else
        {
            Crater[] cratersNull = new Crater[1];
            cratersNull[0].radius = 0;
            cratersNull[0].centre = new Vector3 (0, 0, 0);
            cratersNull[0].floorHeight = 0;
            ComputeBuffer craterBuffer;
            craterBuffer = new ComputeBuffer(cratersNull.Length, 20);
            craterBuffer.SetData(cratersNull);
            vertices = ApplyComputeHeights(calculateVertexHeight, vertexBuffer, heightBuffer, craterBuffer);
        }
        
        return vertices;
    }

    private static void SetShaderParams(ComputeShader calculateVertexHeight, PlanetVar planetVar)
    {
        //Crater params
        calculateVertexHeight.SetFloat("rimWidth", planetVar.craterSettings.rimWidth);
        calculateVertexHeight.SetFloat("rimSteepness", planetVar.craterSettings.rimSteepness);
        calculateVertexHeight.SetFloat("smoothness", planetVar.craterSettings.smoothness);
        calculateVertexHeight.SetFloat("numCraters", planetVar.craterSettings.numCraters);

        //ShapeNoise params
        calculateVertexHeight.SetInt("shapeNoiseOctaves", planetVar.shapeNoiseSettings.octaves);
        calculateVertexHeight.SetFloat("shapeNoiseScale", planetVar.shapeNoiseSettings.scale);
        calculateVertexHeight.SetFloat("shapeNoisePersistence", planetVar.shapeNoiseSettings.persistence);
        calculateVertexHeight.SetFloat("shapeNoiseLacunarity", planetVar.shapeNoiseSettings.lacunarity);
        calculateVertexHeight.SetFloat("shapeNoiseMultiplier", planetVar.shapeNoiseSettings.multiplier);
        calculateVertexHeight.SetFloat("shapeNoiseGain", planetVar.shapeNoiseSettings.gain);
        calculateVertexHeight.SetFloats("shapeNoiseOffset", planetVar.shapeNoiseSettings.offset);
        calculateVertexHeight.SetFloat("shapeNoiseElevation", planetVar.shapeNoiseSettings.elevation);

        //DetailNoise params
        calculateVertexHeight.SetInt("detailNoiseOctaves", planetVar.detailNoiseSettings.octaves);
        calculateVertexHeight.SetFloat("detailNoiseScale", planetVar.detailNoiseSettings.scale);
        calculateVertexHeight.SetFloat("detailNoisePersistence", planetVar.detailNoiseSettings.persistence);
        calculateVertexHeight.SetFloat("detailNoiseLacunarity", planetVar.detailNoiseSettings.lacunarity);
        calculateVertexHeight.SetFloat("detailNoiseMultiplier", planetVar.detailNoiseSettings.multiplier);
        calculateVertexHeight.SetFloat("detailNoiseGain", planetVar.detailNoiseSettings.gain);
        calculateVertexHeight.SetFloats("detailNoiseOffset", planetVar.detailNoiseSettings.offset);
        calculateVertexHeight.SetFloat("detailNoiseElevation", planetVar.detailNoiseSettings.elevation);

        //RidgeNoise params
        calculateVertexHeight.SetInt("ridgeNoiseOctaves", planetVar.ridgeNoiseSettings.octaves);
        calculateVertexHeight.SetFloat("ridgeNoiseScale", planetVar.ridgeNoiseSettings.scale);
        calculateVertexHeight.SetFloat("ridgeNoisePersistence", planetVar.ridgeNoiseSettings.persistence);
        calculateVertexHeight.SetFloat("ridgeNoiseLacunarity", planetVar.ridgeNoiseSettings.lacunarity);
        calculateVertexHeight.SetFloat("ridgeNoiseMultiplier", planetVar.ridgeNoiseSettings.multiplier);
        calculateVertexHeight.SetFloat("ridgeNoiseGain", planetVar.ridgeNoiseSettings.gain);
        calculateVertexHeight.SetFloats("ridgeNoiseOffset", planetVar.ridgeNoiseSettings.offset);
        calculateVertexHeight.SetFloat("ridgeNoiseElevation", planetVar.ridgeNoiseSettings.elevation);
        calculateVertexHeight.SetFloat("ridgeNoiseSmoothing", planetVar.ridgeNoiseSettings.smoothing);
        calculateVertexHeight.SetFloat("ridgeNoisePower", planetVar.ridgeNoiseSettings.power);
    }

    private static Vector3[] ApplyComputeHeights(ComputeShader calculateVertexHeight, ComputeBuffer vertexBuffer, ComputeBuffer heightBuffer, ComputeBuffer craterBuffer)
    {
        //Set values for compute shader use
        calculateVertexHeight.SetInt("numVertices", vertexBuffer.count);
        calculateVertexHeight.SetBuffer(0, "vertices", vertexBuffer);
        calculateVertexHeight.SetBuffer(0, "heights", heightBuffer);
        calculateVertexHeight.SetBuffer(0, "craters", craterBuffer);

        //Run the shader using a function which calculates the number of dispatch calls required based on the number of iterations required and the threading dimensions of the shader
        RunComputeShader(calculateVertexHeight, vertexBuffer.count);

        //Create a placeholder vertex array
        var vertices = new Vector3[vertexBuffer.count];

        //Write compute shader output to the vertex array
        vertexBuffer.GetData(vertices);

        //Remove buffers from GPU memory to save resources
        vertexBuffer.Release();
        heightBuffer.Release();
        craterBuffer.Release();
        return vertices;
    }

    public static void RunComputeShader(ComputeShader cs, int numIterationsX, int numIterationsY = 1, int numIterationsZ = 1, int kernelIndex = 0)
    {
        //Calculate required dispatch sizing based on the number of iterations required and the threading dimensions of the shader
        Vector3Int threadGroupSizes = GetThreadGroupSizes(cs, kernelIndex);
        int numGroupsX = Mathf.CeilToInt(numIterationsX / (float)threadGroupSizes.x);
        int numGroupsY = Mathf.CeilToInt(numIterationsY / (float)threadGroupSizes.y);
        int numGroupsZ = Mathf.CeilToInt(numIterationsZ / (float)threadGroupSizes.y);
        cs.Dispatch(kernelIndex, numGroupsX, numGroupsY, numGroupsZ);
    }

    private static Vector3Int GetThreadGroupSizes(ComputeShader cs, int kernelIndex)
    {
        //Access the threading dimensions of the compute shader
        uint x, y, z;
        cs.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
        return new Vector3Int((int)x, (int)y, (int)z);
    }

    #endregion

}

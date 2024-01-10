using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static PlanetVar;

public class PlanetaryGenerator : MonoBehaviour
{
    public struct Crater
    {
        public float radius;
        public Vector3 centre;
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
            craters[i].radius = radius;
            craters[i].centre = centre;
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
        ComputeBuffer craterBuffer;

        if (craters.Length > 0)
        {
            craterBuffer = new ComputeBuffer(craters.Length, 16);
            craterBuffer.SetData(craters);
            calculateVertexHeight.SetFloat("rimWidth", planetVar.craterSettings.rimWidth);
            calculateVertexHeight.SetFloat("rimSteepness", planetVar.craterSettings.rimSteepness);
            calculateVertexHeight.SetFloat("smoothness", planetVar.craterSettings.smoothness);
            calculateVertexHeight.SetFloat("floorHeight", planetVar.craterSettings.floorHeight);
            calculateVertexHeight.SetFloat("numCraters", planetVar.craterSettings.numCraters);
            vertices = ApplyComputeHeights(calculateVertexHeight, vertexBuffer, heightBuffer, craterBuffer);
        }
        else
        {
            vertices = ApplyComputeHeights(calculateVertexHeight, vertexBuffer, heightBuffer);
        }
        
        return vertices;
    }

    private static Vector3[] ApplyComputeHeights(ComputeShader calculateVertexHeight, ComputeBuffer vertexBuffer, ComputeBuffer heightBuffer, ComputeBuffer craterBuffer)
    {
        //Set values for compute shader use
        calculateVertexHeight.SetInt("numVertices", vertexBuffer.count);
        calculateVertexHeight.SetBuffer(0, "vertices", vertexBuffer);
        calculateVertexHeight.SetBuffer(0, "heights", heightBuffer);
        if (craterBuffer != null) {
            calculateVertexHeight.SetBuffer(0, "craters", craterBuffer);
        }

        //Run the shader using a function which calculates the number of dispatch calls required based on the number of iterations required and the threading dimensions of the shader
        RunComputeShader(calculateVertexHeight, vertexBuffer.count);

        //Create a placeholder vertex array
        var vertices = new Vector3[vertexBuffer.count];

        //Write compute shader output to the vertex array
        vertexBuffer.GetData(vertices);

        //Remove buffers from GPU memory to save resources
        vertexBuffer.Release();
        heightBuffer.Release();
        return vertices;
    }

    //Overload method without craters
    private static Vector3[] ApplyComputeHeights(ComputeShader calculateVertexHeight, ComputeBuffer vertexBuffer, ComputeBuffer heightBuffer)
    {
        //Set values for compute shader use
        calculateVertexHeight.SetInt("numVertices", vertexBuffer.count);
        calculateVertexHeight.SetBuffer(0, "vertices", vertexBuffer);
        calculateVertexHeight.SetBuffer(0, "heights", heightBuffer);

        //Run the shader using a function which calculates the number of dispatch calls required based on the number of iterations required and the threading dimensions of the shader
        RunComputeShader(calculateVertexHeight, vertexBuffer.count);

        //Create a placeholder vertex array
        var vertices = new Vector3[vertexBuffer.count];

        //Write compute shader output to the vertex array
        vertexBuffer.GetData(vertices);

        //Remove buffers from GPU memory to save resources
        vertexBuffer.Release();
        heightBuffer.Release();
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

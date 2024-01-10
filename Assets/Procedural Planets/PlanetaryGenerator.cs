using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class PlanetaryGenerator : MonoBehaviour
{
    public struct Crater
    {
        public float radius;
        public Vector3 centre;
    };
    public static Mesh ModifyGeometry(Mesh mesh, ComputeShader calculateVertexHeight, int seed, int numCraters, float craterBias)
    {
        Vector3[] vertices = mesh.vertices;
        vertices = AdjustTerrain(vertices, GenerateRandomCraters(vertices, seed, numCraters, craterBias), calculateVertexHeight);
        mesh.vertices = vertices;
        return mesh;
    }
    #region CraterGeneration
    private static Crater[] GenerateRandomCraters(Vector3[] vertices, int seed, int numCraters, float craterBias)
    {
        Crater[] craters = new Crater[numCraters];
        UnityEngine.Random.InitState(seed);
        for (int i = 0; i < numCraters; i++)
        {
            float radius = ApplyBias(UnityEngine.Random.value, craterBias);
            Vector3 centre = UnityEngine.Random.onUnitSphere;
            craters[i].radius = radius;
            craters[i].centre = centre;
        }
        return craters;
    }

    private static float ApplyBias(float input, float bias)
    {
        return input + (1 - Mathf.Exp(-bias * input));
    }
    #endregion

    #region HeightComputation
    private static Vector3[] AdjustTerrain(Vector3[] vertices, Crater[] craters, ComputeShader calculateVertexHeight)
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

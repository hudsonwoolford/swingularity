using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class PlanetaryGenerator : MonoBehaviour
{
    public struct Crater
    {
        public float radius;
        public Vector3 centre;
        public float floorHeight;
    };
    public static Mesh ModifyGeometry(Mesh mesh, TerrainVar terrainVar, ComputeShader calculateVertexHeight)
    {
        Vector3[] vertices = mesh.vertices;
        vertices = AdjustTerrain(vertices, GenerateRandomCraters(vertices, terrainVar), terrainVar, calculateVertexHeight);
        mesh.vertices = vertices;
        return mesh;
    }
    #region CraterGeneration
    private static Crater[] GenerateRandomCraters(Vector3[] vertices, TerrainVar terrainVar)
    {
        Crater[] craters = new Crater[terrainVar.craterSettings.numCraters];
        UnityEngine.Random.InitState(terrainVar.generalSettings.seed);
        for (int i = 0; i < terrainVar.craterSettings.numCraters; i++)
        {
            float radius = Mathf.Lerp(terrainVar.craterSettings.minCraterSize, terrainVar.craterSettings.maxCraterSize, ApplyBias(UnityEngine.Random.value, terrainVar.craterSettings.craterBias));
            Vector3 centre = UnityEngine.Random.onUnitSphere;
            float floorHeight = UnityEngine.Random.Range(-0.25f, 0f);
            bool intersecting = true;
            for(int u = 0; u < craters.Length; u++){
                if ((centre - craters[u].centre).magnitude <= craters[u].radius + terrainVar.craterSettings.rimWidth){
                    intersecting = true;
                }
            }
            if (!intersecting){
                craters[i].radius = radius;
                craters[i].centre = centre;
                craters[i].floorHeight = floorHeight;
            }
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
    private static Vector3[] AdjustTerrain(Vector3[] vertices, Crater[] craters, TerrainVar terrainVar, ComputeShader calculateVertexHeight)
    {
        //Init compute buffers for use with compute shader
        ComputeBuffer vertexBuffer = new ComputeBuffer(vertices.Length, 12);
        vertexBuffer.SetData(vertices);
        ComputeBuffer heightBuffer = new ComputeBuffer(vertices.Length, 4);
        float[] heights = new float[vertices.Length];
        heightBuffer.SetData(heights);

        SetShaderParams(calculateVertexHeight, terrainVar);


        if (terrainVar.craterSettings.numCraters > 0)
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

    private static void SetShaderParams(ComputeShader calculateVertexHeight, TerrainVar terrainVar)
    {
        //Crater params
        calculateVertexHeight.SetFloat("rimWidth", terrainVar.craterSettings.rimWidth);
        calculateVertexHeight.SetFloat("rimSteepness", terrainVar.craterSettings.rimSteepness);
        calculateVertexHeight.SetFloat("smoothness", terrainVar.craterSettings.smoothness);
        calculateVertexHeight.SetFloat("numCraters", terrainVar.craterSettings.numCraters);
        calculateVertexHeight.SetFloat("floorHeight", terrainVar.craterSettings.floorHeight);

        //ShapeNoise params
        calculateVertexHeight.SetInt("shapeNoiseOctaves", terrainVar.shapeNoiseSettings.octaves);
        calculateVertexHeight.SetFloat("shapeNoiseScale", terrainVar.shapeNoiseSettings.scale);
        calculateVertexHeight.SetFloat("shapeNoisePersistence", terrainVar.shapeNoiseSettings.persistence);
        calculateVertexHeight.SetFloat("shapeNoiseLacunarity", terrainVar.shapeNoiseSettings.lacunarity);
        calculateVertexHeight.SetFloat("shapeNoiseMultiplier", terrainVar.shapeNoiseSettings.multiplier);
        calculateVertexHeight.SetFloat("shapeNoiseGain", terrainVar.shapeNoiseSettings.gain);
        calculateVertexHeight.SetFloats("shapeNoiseOffset", terrainVar.shapeNoiseSettings.offset);
        calculateVertexHeight.SetFloat("shapeNoiseElevation", terrainVar.shapeNoiseSettings.elevation);
        calculateVertexHeight.SetBool("shapeNoiseApply", terrainVar.shapeNoiseSettings.applyNoise);

        //DetailNoise params
        calculateVertexHeight.SetInt("detailNoiseOctaves", terrainVar.detailNoiseSettings.octaves);
        calculateVertexHeight.SetFloat("detailNoiseScale", terrainVar.detailNoiseSettings.scale);
        calculateVertexHeight.SetFloat("detailNoisePersistence", terrainVar.detailNoiseSettings.persistence);
        calculateVertexHeight.SetFloat("detailNoiseLacunarity", terrainVar.detailNoiseSettings.lacunarity);
        calculateVertexHeight.SetFloat("detailNoiseMultiplier", terrainVar.detailNoiseSettings.multiplier);
        calculateVertexHeight.SetFloat("detailNoiseGain", terrainVar.detailNoiseSettings.gain);
        calculateVertexHeight.SetFloats("detailNoiseOffset", terrainVar.detailNoiseSettings.offset);
        calculateVertexHeight.SetFloat("detailNoiseElevation", terrainVar.detailNoiseSettings.elevation);
        calculateVertexHeight.SetBool("detailNoiseApply", terrainVar.detailNoiseSettings.applyNoise);

        //RidgeNoise params
        calculateVertexHeight.SetInt("ridgeNoiseOctaves", terrainVar.ridgeNoiseSettings.octaves);
        calculateVertexHeight.SetFloat("ridgeNoiseScale", terrainVar.ridgeNoiseSettings.scale);
        calculateVertexHeight.SetFloat("ridgeNoisePersistence", terrainVar.ridgeNoiseSettings.persistence);
        calculateVertexHeight.SetFloat("ridgeNoiseLacunarity", terrainVar.ridgeNoiseSettings.lacunarity);
        calculateVertexHeight.SetFloat("ridgeNoiseMultiplier", terrainVar.ridgeNoiseSettings.multiplier);
        calculateVertexHeight.SetFloat("ridgeNoiseGain", terrainVar.ridgeNoiseSettings.gain);
        calculateVertexHeight.SetFloats("ridgeNoiseOffset", terrainVar.ridgeNoiseSettings.offset);
        calculateVertexHeight.SetFloat("ridgeNoiseElevation", terrainVar.ridgeNoiseSettings.elevation);
        calculateVertexHeight.SetFloat("ridgeNoiseSmoothing", terrainVar.ridgeNoiseSettings.smoothing);
        calculateVertexHeight.SetFloat("ridgeNoisePower", terrainVar.ridgeNoiseSettings.power);
        calculateVertexHeight.SetBool("ridgeNoiseApply", terrainVar.ridgeNoiseSettings.applyNoise);

        //WarpedNoise params
        calculateVertexHeight.SetInt("warpedNoiseOctaves", terrainVar.warpedNoiseSettings.octaves);
        calculateVertexHeight.SetFloat("warpedNoiseScale", terrainVar.warpedNoiseSettings.scale);
        calculateVertexHeight.SetFloat("warpedNoisePersistence", terrainVar.warpedNoiseSettings.persistence);
        calculateVertexHeight.SetFloat("warpedNoiseLacunarity", terrainVar.warpedNoiseSettings.lacunarity);
        calculateVertexHeight.SetFloat("warpedNoiseMultiplier", terrainVar.warpedNoiseSettings.multiplier);
        calculateVertexHeight.SetFloat("warpedNoiseGain", terrainVar.warpedNoiseSettings.gain);
        calculateVertexHeight.SetFloats("warpedNoiseOffset", terrainVar.warpedNoiseSettings.offset);
        calculateVertexHeight.SetFloat("warpedNoiseStrength", terrainVar.warpedNoiseSettings.strength);
        calculateVertexHeight.SetBool("warpedNoiseApply", terrainVar.warpedNoiseSettings.applyNoise);

        //ContinentNoise params
        calculateVertexHeight.SetInt("continentNoiseOctaves", terrainVar.continentNoiseSettings.octaves);
        calculateVertexHeight.SetFloat("continentNoiseScale", terrainVar.continentNoiseSettings.scale);
        calculateVertexHeight.SetFloat("continentNoisePersistence", terrainVar.continentNoiseSettings.persistence);
        calculateVertexHeight.SetFloat("continentNoiseLacunarity", terrainVar.continentNoiseSettings.lacunarity);
        calculateVertexHeight.SetFloat("continentNoiseMultiplier", terrainVar.continentNoiseSettings.multiplier);
        calculateVertexHeight.SetFloat("continentNoiseGain", terrainVar.continentNoiseSettings.gain);
        calculateVertexHeight.SetFloats("continentNoiseOffset", terrainVar.continentNoiseSettings.offset);
        calculateVertexHeight.SetFloat("continentNoiseDepth", terrainVar.continentNoiseSettings.depthMultiplier);
        calculateVertexHeight.SetBool("continentNoiseApply", terrainVar.continentNoiseSettings.applyNoise);
        calculateVertexHeight.SetFloat("oceanFloorDepth", terrainVar.continentNoiseSettings.oceanFloorDepth);
        calculateVertexHeight.SetFloat("oceanFloorSmoothing", terrainVar.continentNoiseSettings.oceanFloorSmoothing);

        //MaskNoise 
        calculateVertexHeight.SetInt("maskNoiseOctaves", terrainVar.maskNoiseSettings.octaves);
        calculateVertexHeight.SetFloat("maskNoiseScale", terrainVar.maskNoiseSettings.scale);
        calculateVertexHeight.SetFloat("maskNoisePersistence", terrainVar.maskNoiseSettings.persistence);
        calculateVertexHeight.SetFloat("maskNoiseLacunarity", terrainVar.maskNoiseSettings.lacunarity);
        calculateVertexHeight.SetFloat("maskNoiseMultiplier", terrainVar.maskNoiseSettings.multiplier);
        calculateVertexHeight.SetFloat("maskNoiseGain", terrainVar.maskNoiseSettings.gain);
        calculateVertexHeight.SetFloats("maskNoiseOffset", terrainVar.maskNoiseSettings.offset);
        calculateVertexHeight.SetFloat("maskNoiseStrength", terrainVar.maskNoiseSettings.strength);
        calculateVertexHeight.SetBool("maskNoiseApply", terrainVar.maskNoiseSettings.applyNoise);
        calculateVertexHeight.SetFloat("maskBlend", terrainVar.maskNoiseSettings.maskBlend);
        calculateVertexHeight.SetFloat("maskVerticalShift", terrainVar.maskNoiseSettings.maskVerticalShift);
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

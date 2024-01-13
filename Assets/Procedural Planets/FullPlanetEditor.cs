using System.Numerics;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FullPlanetGeneration))]
public class FullPlanetEditor : Editor
{
    #region properties
    //[Header(Parameters)] //Parameters
    // [Toggle()] _BiomeA("BiomeA", float) = 1
    // [Toggle()] _BiomeB("BiomeB", float) = 1
    // _BiomeAEnd("Biome A End", Range(0,1)) = 0.4
    // _BiomeBStart("Biome B  Start", Range(0,1)) = 0.6
    // [Toggle()] _SwapBiome("SwapBiome", float) = 0
    // _SteepMultiplier("Steep Multiplier", Range(0,5)) = 1

    // [Header(Biome A)] //Biome A

    //_FlatColorA("Flat Color A", Color) = (1,1,1)
    // _SteepColorA("Steep Color A", Color) = (1,1,1)
    // _ShorelineColorA("Shoreline Color A", Color) = (1,1,1)
    // _HighElevationColorA("High Elevation Color A", Color) = (1,1,1) 
    // _HighElevationSteepColorA("High Elevation Steep Color A", Color) = (1,1,1) 

    // _MetallicA("Metallic A", Range(0, 1)) = 0
    // _SmoothnessA("Smoothness A", Range(0,1)) = 0

    // _ShorelineA("Shoreline Elevation A", Range(0, 0.2)) = 0.01
    // _VegetationLineA("Vegetation Line A", Range(0, 0.5)) = 0.1
    // [PowerSlider(3.0)] _VegetationLineHarshnessA("Vegetation Line Harshness A", Range(0, 10)) = 2

    // [Header(Biome B)] //Biome B

    //_FlatColorB("Flat Color B", Color) = (0,0,0)
    // _SteepColorB("Steep Color B", Color) = (0,0,0)
    // _ShorelineColorB("Shoreline Color B", Color) = (0,0,0)
    // _HighElevationColorB("High Elevation Color B", Color) = (0,0,0)
    // _HighElevationSteepColorB("High Elevation Steep Color B", Color) = (0,0,0)

    // _MetallicB("Metallic B", Range(0, 1)) = 0
    // _SmoothnessB("Smoothness B", Range(0,1)) = 0

    // _ShorelineB("Shoreline Elevation A", Range(0, 0.2)) = 0.01
    // _VegetationLineB("Vegetation Line B", Range(0, 0.5)) = 0.1
    // [PowerSlider(3.0)] _VegetationLineHarshnessB("Vegetation Line Harshness B", Range(0, 10)) = 2

    // [Header(Constants)] //Constants

    //_SeaLevel("Sea Level", Range(0, 2)) = 1
    // _OceanBlendEnd("Ocean Blend End", Range(0, 2)) = 0.5
    // [PowerSlider(3.0)] _OceanBlendIntensity("Ocean Blend Intensity", Range(0, 10)) = 1
    // _SeabedColor("Sea Bed Color", Color) = (1,1,1)

    // [Header(Fresnel)] //Fresnel
    //[Toggle()] _UseFresnel("UseFresnel", float) = 0
    // _FresnelColor("Fresnel Color", Color) = (1,1,1)
    // _FresnelStrengthNear("Fresnel Strength Near", float) = 2
    // _FresnelStrengthFar("Fresnel Strength Far", float) = 5
    // _FresnelPower("Fresnel Power", float) = 2

    // [Header(Noise)] //Noise

    //[IntRange] _Octaves("Octaves", Range(0,5)) = 1
    // _Offset("Offset", vector) = (0,0,0)
    // _Scale("Scale", Range(0,5)) = 1
    // _Persistence("Persistence", Range(0, 5)) = 1
    // _Lacunarity("Lacunarity", Range(0,5)) = 1
    // _Multiplier("Multiplier", Range(0,5)) = 1
    // _Gain("Gain", Range(0, 5)) = 1
    // [Header(WarpNoise)]
    //[IntRange] _WarpOctaves("Warp Octaves", Range(0,5)) = 1
    // _WarpOffset("Warp Offset", vector) = (0,0,0)
    // _WarpScale("Warp Scale", Range(0,5)) = 1
    // _WarpPersistence("Warp Persistence", Range(0, 5)) = 1
    // _WarpLacunarity("Warp Lacunarity", Range(0,5)) = 1
    // _WarpMultiplier("Warp Multiplier", Range(0,5)) = 1
    // _WarpGain("Warp Gain", Range(0, 5)) = 1
    #endregion
    public override void OnInspectorGUI()
    {
        //Content
    }
}

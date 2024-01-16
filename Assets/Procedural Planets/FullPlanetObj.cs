using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "fullPlanetObj", menuName = "fullPlanetObj")]
public class FullPlanetObj : ScriptableObject
{
    public Material planetMaterial;
    public TerrainVar terrainVar;
    [HideInInspector]
    public OceanVariables oceanVariables;
    [HideInInspector]
    public AtmosphereVariables atmosphereVariables;
}
[Serializable]
public class OceanVariables
{
    public float _SeaLevel;
    public float _OceanBlendEnd = 0.01f;
    public float _OceanBlendIntensity = 0.01f;
    public Color _ShorelineColor = Color.white;
    public Color _SeabedColor = Color.white;
    public Color _OceanColor = Color.white;
    public float _OceanMetallic;
    public float _OceanSmoothness;
}
[Serializable]
public class AtmosphereVariables
{
    public Color OuterBandColor = Color.white;
    public float OuterBandRange = 1;
    public float OuterBandIntensity = 1;

    public Color InnerBandColor = Color.white;
    public float InnerBandIntensity = 1;
    public float InnerBandRange = 1;

    public Color FrontColor = Color.white;
    public float FrontRange = 1;
    public float FrontIntensity = 1;
}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "fullPlanetObj", menuName = "fullPlanetObj")]
public class FullPlanetObj : ScriptableObject
{
    public Material planetMaterial;
    public TerrainVar terrainVar;
    [SerializeField] public OceanVariables oceanVariables;
    [SerializeField] public AtmosphereVariables atmosphereVariables;
}
[Serializable]
public class OceanVariables
{
    [Range(0, 5)]
    [SerializeField] public float _SeaLevel = 1;
    [Range(0, 1)]
    [SerializeField] public float _OceanBlendEnd = 0.01f;
    [Range(0, 10)]
    [SerializeField] public float _OceanBlendIntensity = 0.01f;
    [SerializeField] public Color _ShorelineColor = Color.white;
    [SerializeField] public Color _SeabedColor = Color.black;
    [SerializeField] public Color _OceanColor = Color.blue;
    [Range(0, 1)]
    [SerializeField] public float _OceanMetallic = 0.2f;
    [Range(0, 1)]
    [SerializeField] public float _OceanSmoothness = 0.6f;
}
[Serializable]
public class AtmosphereVariables
{
    [SerializeField] public Color _OuterBandColor = Color.white;
    [Range(0, 5)]
    [SerializeField] public float _OuterBandRange = 2.2f;
    [Range(0, 5)]
    [SerializeField] public float _OuterBandIntensity = 0.6f;

    [SerializeField] public Color _InnerBandColor = Color.white;
    [Range(0, 5)]
    [SerializeField] public float _InnerBandRange = 2.1f;
    [Range(0, 5)]
    [SerializeField] public float _InnerBandIntensity = 4f;

    [SerializeField] public Color _FrontColor = Color.white;
    [Range(0, 5)]
    [SerializeField] public float _FrontRange = 2.0f;
    [Range(0, 5)]
    [SerializeField] public float _FrontIntensity = 0.15f;
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "planetColorScheme", menuName = "planetColorScheme")]
public class PlanetColorScheme : ScriptableObject
{
    public Color _FlatColor = Color.white;
    public Color _FlatColorVariant = Color.white;
    public Color _SteepColor = Color.white;
    public Color _HighElevationColor = Color.white;
    public Color _HighElevationSteepColor = Color.white;

    [Range(0f, 1f)]
    public float _Metallic;
    [Range(0f, 1f)]
    public float _Smoothness;
    [Range(0f, 1f)]
    public float _VegetationLine = 0.1f;
    [Range (0f, 10f)]
    public float _VegetationLineHarshness = 7f;
    [Range(0f, 5f)]
    public float _SteepMultiplier = 7f;

    [Header("Optional Variables")]
    public OceanVariables oceanVariables = new OceanVariables();
    public AtmosphereVariables atmosphereVariables = new AtmosphereVariables();
    public FresnelVariables fresnelVariables = new FresnelVariables();
    public TreeVariables treeVariables = new TreeVariables();
}

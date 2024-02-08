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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenStaticReferences : ScriptableObject
{
    [SerializeField] public float minSize = 3;
    [SerializeField] public float maxSize = 9;
    [SerializeField] public const float sizeBias = 0.02f, sizeScale = 1, samplesBeforeRejection = 21, offsetScale = 59.5f;
    [SerializeField] public readonly float[] specialObjectMultipliers = new float[10]{
        0f, 0.65f, 0.9f, 1.65f, 1.9f, 2.25f, 2.65f, 2.9f, 3.65f, 3.9f, 
    };
    [SerializeField] public float planetScale; //Scale of planets multiplier
    [SerializeField] public float moonCutoffSize = 7, moonDstScale = 0.8f;
    [SerializeField] public Material moonLine;
    [SerializeField] public Material invisOcean; //Invisble ocean material used for imaging
    [SerializeField] public PhysicMaterial superBouncyMoon;
    [SerializeField] public GalaxyData[] possibleGalaxySchemes; //Filters which planet types are chosen
}

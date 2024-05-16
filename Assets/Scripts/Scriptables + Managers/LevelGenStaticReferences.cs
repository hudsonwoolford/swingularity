using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenStaticReferences : ScriptableObject
{
    [SerializeField] float minSize = 3;
    [SerializeField] float maxSize = 9;
    [SerializeField] const float sizeBias = 0.02f, sizeScale = 1, samplesBeforeRejection = 21, offsetScale = 59.5f;
    [SerializeField] readonly float[] specialObjectMultipliers = new float[10]{
        0f, 0.65f, 0.9f, 1.65f, 1.9f, 2.25f, 2.65f, 2.9f, 3.65f, 3.9f, 
    };
    [SerializeField] float planetScale; //Scale of planets multiplier
    [SerializeField] float moonCutoffSize = 1, moonDstScale = 0.8f;
    [SerializeField] Material moonLine;
    [SerializeField] Material invisOcean; //Invisble ocean material used for imaging
    [SerializeField] PhysicMaterial superBouncyMoon;
    [SerializeField] GalaxyData[] possibleGalaxySchemes; //Filters which planet types are chosen
}

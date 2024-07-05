using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyController : MonoBehaviour
{
    private struct GalaxyIcon{
        public string name;
        public ColorSchemeStaticReference.PlanetType planetType;
        public Color[] possibleColors;
    }

    void Update(){
        ApplyGalaxyIconScheme();
    }
    void ApplyGalaxyIconScheme(){}
}

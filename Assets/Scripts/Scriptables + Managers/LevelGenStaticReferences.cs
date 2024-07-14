using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenStaticReferences : ScriptableObject
{
    [System.Serializable]
    public struct RegionSizesWrapper
    {
        public float baseMultiplier;
        public List<Vector2> nestedRegionSizes;
    }
    [System.Serializable]
    public struct SpecialObjectMultWrapper
    {
        public LevelGenerator.SpecialType specialType;
        public Vector2 minMaxMultiplier;
    }


    [SerializeField] public float minSize = 3;
    [SerializeField] public float maxSize = 9;
    [SerializeField] public float sizeBias = 0.02f, sizeScale = 1, samplesBeforeRejection = 21, offsetScale = 59.5f;
    [SerializeField] public SpecialObjectMultWrapper[] inherentSpecialMultipliers;
    [SerializeField] public readonly float[] specialObjectMultipliers = new float[10]{
        0f, 0.65f, 0.9f, 1.65f, 1.9f, 2.25f, 2.65f, 2.9f, 3.65f, 3.9f, 
    };
    [SerializeField]
    public readonly int[] difficultyPars = new int[10]{
        2, 2, 2, 2, 2, 3, 3, 3, 3, 3
    };
    [SerializeField] public RegionSizesWrapper[] regionSizes = new RegionSizesWrapper[10];
    [SerializeField] public float planetScale; //Scale of planets multiplier
    [SerializeField] public float moonCutoffSize = 7, moonDstScale = 0.8f;
    [SerializeField] public Material moonLine;
    [SerializeField] public Material invisOcean; //Invisble ocean material used for imaging
    [SerializeField] public PhysicMaterial superBouncyMoon;
    [SerializeField] public GalaxyData[] possibleGalaxySchemes; //Filters which planet types are chosen
    public readonly string[] GalaxyWords = new string[128]{
        "nova",
        "photon",
        "Virgo",
        "spheroidal",
        "large",
        "Magellanic",
        "cloud",
        "small",
        "death",
        "star",
        "stars",
        "starry",
        "Sagittarius",
        "parsec",
        "irregular",
        "spheroid",
        "dwarf",
        "galaxy",
        "satellite",
        "way",
        "nebula",
        "globular",
        "Draco",
        "aurora",
        "Boss",
        "Wolf",
        "Alpha",
        "Gamma",
        "Delta",
        "Epsilon",
        "Perseus",
        "titan",
        "venusian",
        "zenith",
        "apex",
        "horizon",
        "graviton",
        "spectrum",
        "chromatic",
        "spiral",
        "lenticular",
        "Leo",
        "dark",
        "ultra",
        "compact",
        "Coma",
        "radius",
        "Fornax",
        "Ursa Major",
        "Phoenix",
        "Pegasus",
        "laser",
        "Sextans",
        "Tucana",
        "Carina",
        "Ursa Minor",
        "cluster",
        "omega",
        "Centauri",
        "triangulum",
        "local",
        "Hubble",
        "light-year",
        "nuclei",
        "barred spiral",
        "peculiar",
        "bulging",
        "ionization",
        "Hertz",
        "ecliptic",
        "Azimuth",
        "coaxial",
        "supergiant",
        "giant",
        "parallax",
        "corona",
        "ultraviolet",
        "accretion",
        "halo",
        "redshift",
        "wormhole",
        "binary",
        "core",
        "aphelion",
        "perihelion",
        "sidereal",
        "Sigma",
        "Theta",
        "Iota",
        "Kappa",
        "Lambda",
        "Mu",
        "Nu",
        "Xi",
        "Omicron",
        "Pi",
        "Rho",
        "Tau",
        "quark",
        "electron",
        "Chi",
        "Psi",
        "Zeta",
        "Eta",
        "galaxy",
        "ring",
        "metallic",
        "sphere",
        "oval",
        "ovoid",
        "remnants",
        "remant",
        "andromeda",
        "gaia",
        "Berenices",
        "Laniakea",
        "Heracles",
        "Hera",
        "whirlpool",
        "sombrero",
        "Kepler",
        "Herschel",
        "Rubin",
        "pinwheel",
        "flocculent",
        "dense",
        "object",
        "diffuse",
    };
}

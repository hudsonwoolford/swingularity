using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProceduralGalaxyGeneration : MonoBehaviour
{
    [SerializeField] int galaxySeed;
    [Range(1, 10)]
    [SerializeField] int galaxyRelativeDifficulty;
    [SerializeField] float orbitSizeScalar;
    [SerializeField] float ringDifference;
    [SerializeField] float basePlanetGap;
    [SerializeField] float planetGapVariance;
    [SerializeField] LevelGenStaticReferences levelStatic;
    [SerializeField] ColorSchemeStaticReference.PlanetType galaxyType;
    [SerializeField] LevelGenerator.SpecialType[] specialTypes;
    [SerializeField] SolarSystemGenerator[] solarSystems = new SolarSystemGenerator[5];

    public delegate void InitProceduralSolarSystem();
    public static InitProceduralSolarSystem initProceduralSolarSystem; 

    private void Awake()
    {
        SetSolarSystems();
    }
    void SetSolarSystems()
    {
        for (int i = 0; i < solarSystems.Length; i++)
        {
            UnityEngine.Random.InitState(i * 120523 + galaxySeed);
            int levelSeed = UnityEngine.Random.Range(-2147483647, 2147483647);
            int levelDifficulty = Mathf.Clamp(galaxyRelativeDifficulty + UnityEngine.Random.Range(-1, 1), 1, 10);

            LevelManager.LevelParams systemParams = new LevelManager.LevelParams(levelSeed, levelDifficulty, galaxyRelativeDifficulty, new ColorSchemeStaticReference.PlanetType[] { galaxyType }, specialTypes);
            solarSystems[i].InitSolarSystem(systemParams, "LevelGen", levelStatic, orbitSizeScalar, ringDifference, basePlanetGap, planetGapVariance);
            initProceduralSolarSystem?.Invoke();
        }
    }
}

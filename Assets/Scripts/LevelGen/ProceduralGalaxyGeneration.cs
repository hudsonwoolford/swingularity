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
    [SerializeField] LevelGenStaticReferences levelStatic;
    [SerializeField] LevelGenerator.SpecialType[] specialTypes;
    [SerializeField] ColorSchemeStaticReference.PlanetType galaxyType;
    [SerializeField] SolarSystemGenerator[] solarSystems = new SolarSystemGenerator[5];

    private void Start()
    {
        SetSolarSystems();
    }
    void SetSolarSystems()
    {
        for (int i = 0; i < solarSystems.Length; i++)
        {
            UnityEngine.Random.InitState(i + galaxySeed);
            int levelSeed = UnityEngine.Random.Range(-2147483647, 2147483647);
            int levelDifficulty = Mathf.Clamp(galaxyRelativeDifficulty + UnityEngine.Random.Range(-1, 1), 1, 10);

            LevelManager.LevelParams systemParams = new LevelManager.LevelParams(levelSeed, levelDifficulty, galaxyRelativeDifficulty, new ColorSchemeStaticReference.PlanetType[] { galaxyType }, specialTypes);
            solarSystems[i].InitSolarSystem(systemParams, "LevelGen", levelStatic, orbitSizeScalar, ringDifference);
        }
    }
}

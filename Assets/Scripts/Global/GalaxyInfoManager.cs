using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GalaxyInfoManager : MonoBehaviour
{
    [System.Serializable]
    struct UniverseTypeGalaxies
    {
        public LevelManager.UniverseParams.UniverseType universeType;
        public LevelManager.GalaxyParams[] galaxyParams;
    }
    [SerializeField] private UniverseTypeGalaxies[] universeTypeGalaxies;

    // Dictionary-like lambda function
    public Func<LevelManager.UniverseParams.UniverseType, LevelManager.GalaxyParams[]> _UniverseTypeGalaxies
        => key => universeTypeGalaxies
                   .Where(x => x.universeType == key)
                   .Select(x => x.galaxyParams)
                   .FirstOrDefault();
}


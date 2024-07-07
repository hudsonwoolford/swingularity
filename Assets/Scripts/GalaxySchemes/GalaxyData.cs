using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "galaxyData", menuName = "galaxyData")]
[System.Serializable]
public class GalaxyData : ScriptableObject
{
    public Color mainColor = Color.white;
    [ColorUsage(true, true)]
    [SerializeField] public Color portalColor = new Color(0.3f, 1, 1, 1);
    [SerializeField] public Color portalParticleColor = new Color(0.3f, 1, 1, 1);
    [ColorUsage(true, true)]
    [SerializeField] public Color ballColor = new Color(0.3f, 1, 1, 1);
    [SerializeField] public Color hackModeColor = new Color(0.3f, 1, 1, 1);
    [SerializeField] public Color[] nebulaColors = new Color[4]{
        new Color (1f, 1f, 1f, 1f),
        new Color (1f, 1f, 1f, 1f),
        new Color (1f, 1f, 1f, 1f),
        new Color (1f, 1f, 1f, 1f),
    };
    [SerializeField]public ColorSchemeStaticReference.PlanetType type;
}

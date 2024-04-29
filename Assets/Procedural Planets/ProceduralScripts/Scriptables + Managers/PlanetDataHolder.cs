using UnityEngine;

[ExecuteInEditMode]
public class PlanetDataHolder : MonoBehaviour
{
    public ForceField forceField;
    public PlanetColorScheme biomeA, biomeB;
    [ColorUsage(true, true)]
    public Color forceFieldColor;

    public bool oBiomeA, oBiomeB, aBiomeA, aBiomeB;
    PlanetColorScheme atmosphereBiome;

    private void Update()
    {
        atmosphereBiome = aBiomeA ? biomeA : biomeB;
        forceFieldColor = atmosphereBiome.atmosphereVariables._ForceFieldColor;
    }
}

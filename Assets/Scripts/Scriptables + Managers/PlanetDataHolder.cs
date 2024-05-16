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
    SpriteRenderer[] sprs = new SpriteRenderer[2];
    Material defaultFrontMat;
    private void Update()
    {
        atmosphereBiome = aBiomeA ? biomeA : biomeB;
        forceFieldColor = atmosphereBiome.atmosphereVariables._ForceFieldColor;
    }
    void HackModeEnabled(bool enabled, Material whiteMat)
    {
        if (sprs.Length <= 0 || sprs[0] == null)
        {
            sprs = GetComponentsInChildren<SpriteRenderer>();
            if(sprs.Length <= 0 || sprs[0] == null) { //moon
                MeshRenderer meshRender = GetComponent<MeshRenderer>();
                defaultFrontMat = enabled ? meshRender.material : defaultFrontMat;
                meshRender.material = enabled ? whiteMat : defaultFrontMat;
                return;
            }
        }
        defaultFrontMat = enabled ? sprs[0].material : defaultFrontMat;
        sprs[0].material = enabled ? whiteMat : defaultFrontMat;
    }
    private void OnEnable()
    {
        HackMode.onHackMode += HackModeEnabled;
    }
    private void OnDisable()
    {
        HackMode.onHackMode -= HackModeEnabled;
    }
}

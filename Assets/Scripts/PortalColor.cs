using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class PortalColor : MonoBehaviour
{
    [ColorUsage(true, true)]
    [SerializeField] Color portalColor = new Color(0.3f, 1, 1, 1);
    [ColorUsage(true, true)]
    [SerializeField] public Color ballColor = new Color(0.3f, 1, 1, 1);
    [SerializeField] MeshRenderer portalRenderer;
    [SerializeField] ParticleSystem portalSystem;
    MaterialPropertyBlock portalBlock;

    public void Init(){
        portalColor = GameManager.LevelManager.currentTheme.portalColor;
        ballColor = GameManager.LevelManager.currentTheme.ballColor;
    }
    // Update is called once per frame
    void Update()
    {
        if(portalRenderer == null) { return; }

        if(portalBlock == null) { portalBlock = new MaterialPropertyBlock(); }
        portalBlock.SetColor("_Color", portalColor);
        portalRenderer.SetPropertyBlock(portalBlock);

        if(portalSystem == null) { return; }
        ParticleSystem.MainModule main = portalSystem.main;
        Gradient particleGradient = new Gradient();
        particleGradient.SetKeys(new GradientColorKey[] { new GradientColorKey(portalColor, 0.0f), new GradientColorKey(portalColor * 1.1f, 1) }, new GradientAlphaKey[] {new GradientAlphaKey(1.0f,0.0f), new GradientAlphaKey(1.0f, 1.0f)});
        main.startColor = particleGradient;
    }
}

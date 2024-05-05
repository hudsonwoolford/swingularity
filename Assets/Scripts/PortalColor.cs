using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalColor : MonoBehaviour
{
    [ColorUsage(true, true)]
    [SerializeField] Color portalColor = new Color(0.3f, 1, 1, 1);
    [ColorUsage(true, true)]
    [SerializeField] Color portalParticleColor = new Color(0.3f, 1, 1, 1);
    [SerializeField] MeshRenderer portalRenderer;
    [SerializeField] ParticleSystem portalSystem;
    MaterialPropertyBlock portalBlock;

    void Start(){
        if(GameManager.LevelManager.currentTheme != null)
        {
            portalColor = GameManager.LevelManager.currentTheme.portalColor;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(portalRenderer == null) { return; }

        if(portalBlock == null) { portalBlock = new MaterialPropertyBlock(); }
        portalBlock.SetColor("_Color", portalColor);
        portalRenderer.SetPropertyBlock(portalBlock);

        if(portalSystem == null) { return; }
        ParticleSystem.MinMaxGradient grad = new ParticleSystem.MinMaxGradient(portalParticleColor, portalParticleColor*1.1f);
        if(grad.gradient != portalSystem.main.startColor.gradient)
        {
            ParticleSystem.MainModule main = portalSystem.main;
            main.startColor = grad;
            portalSystem.Clear();
            portalSystem.Play();
        }
    }
    void ProceduralInit(GalaxyData theme)
    {
        portalColor = theme.portalColor;
        portalParticleColor = theme.portalParticleColor;
    }
    private void OnEnable()
    {
        LevelGenerator.levelInit += ProceduralInit;
    }
    private void OnDisable()
    {
        LevelGenerator.levelInit -= ProceduralInit;
    }
}

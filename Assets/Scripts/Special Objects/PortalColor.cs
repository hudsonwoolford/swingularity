using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalColor : MonoBehaviour
{
    [System.Serializable]
    struct RotationPiece
    {
        public Transform piece;
        public float rotationSpeed;
    }
    [ColorUsage(true, true)]
    [SerializeField] Color portalColor = new Color(0.3f, 1, 1, 1);
    [ColorUsage(true, true)]
    [SerializeField] Color portalParticleColor = new Color(0.3f, 1, 1, 1);
    [SerializeField] MeshRenderer portalRenderer;
    [SerializeField] ParticleSystem portalSystem;
    [Header("Portal Ring")]
    [SerializeField] Material emissiveMat;
    [SerializeField] RotationPiece[] rotatePieces;
    [SerializeField] float rotateSpeed;
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
        for (int i = 0; i < rotatePieces.Length; i++)
        {
            rotatePieces[i].piece.eulerAngles += new Vector3(0, 0, 1) * rotatePieces[i].rotationSpeed * Time.deltaTime;
        }

        if(portalRenderer == null) { return; }

        if(portalBlock == null) { portalBlock = new MaterialPropertyBlock(); }
        emissiveMat.SetColor("_EmissionColor", portalColor * 0.4f);
        emissiveMat.EnableKeyword("_EMISSION");
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OceanColorManager : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color oceanColor;
    public float metallic, smoothness;
    public Texture2D normalTexture;

    private Material oceanMat;
    private MaterialPropertyBlock pBlock;
    private MeshRenderer meshRenderer;
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        oceanMat = meshRenderer.sharedMaterial;
        pBlock = new MaterialPropertyBlock();
    }
    void Update()
    {
        
    }
    public void UpdateOcean()
    {
        if (meshRenderer == null) { meshRenderer = GetComponent<MeshRenderer>(); }
        if (oceanMat == null) { oceanMat = meshRenderer.sharedMaterial; }
        if (pBlock == null) { pBlock = new MaterialPropertyBlock(); }

        //p Block
        pBlock.SetColor("_Color", oceanColor);
        pBlock.SetFloat("_Smoothness", smoothness);
        pBlock.SetFloat("_Metallic", metallic);
        if (normalTexture != null) { pBlock.SetTexture("_MainTex", normalTexture); }
       

        meshRenderer.SetPropertyBlock(pBlock);
    }
}
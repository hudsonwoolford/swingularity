using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CustomImageEffect : MonoBehaviour
{
    private Material material;
    [SerializeField] Color colA;
    [SerializeField] Color colB;
    [SerializeField] float depthMultiplier;
    [SerializeField] float alphaMultiplier;

    [SerializeField] Vector3 oceanCentre;
    [SerializeField] float oceanRadius;
    [SerializeField] float planetScale;
    private void Awake()
    {
        material = new Material(Shader.Find("Hidden/Ocean"));
        print(material.shader);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        ShaderValues();
        Graphics.Blit(source, destination, material);
    }
    void ShaderValues()
    {
        material.SetColor("colA", colA);
        material.SetColor("colA", colB);
        material.SetFloat("depthMultiplier", depthMultiplier);
        material.SetFloat("depthMultiplier", alphaMultiplier);
        material.SetVector("oceanCentre", oceanCentre);
        material.SetFloat("oceanRadius", oceanRadius);
        material.SetFloat("planetScale", planetScale);
    }
}

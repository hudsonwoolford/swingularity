using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthBufferCamera : MonoBehaviour
{
    [SerializeField] bool blit;
    [SerializeField] Material depthEffect;
    [Range(-5, 5)]
    [SerializeField] float depthPower = 1;
    [SerializeField] bool showNormals;
    Camera mainCam;

    private void Start()
    {
        if(depthEffect == null || !blit) { return; }
        if (Camera.main.depthTextureMode != DepthTextureMode.DepthNormals)
            Camera.main.depthTextureMode = DepthTextureMode.DepthNormals;
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(depthEffect == null || !blit) { return; }
        depthEffect.SetFloat("_DepthPower", depthPower);
        depthEffect.SetFloat("_ShowNormals", showNormals ? 1 : 0);
        Graphics.Blit(source, destination, depthEffect);
    }
}

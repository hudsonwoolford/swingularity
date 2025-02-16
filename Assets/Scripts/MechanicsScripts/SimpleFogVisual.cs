using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFogVisual : MonoBehaviour
{
    [SerializeField] Material fogMat;
    [SerializeField] bool blit;
    Transform ball;
    void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        fogMat.SetVector("_CamPos", Camera.main.transform.position);
        fogMat.SetVector("_CameraSize", CalculateCamsize(Camera.main.orthographicSize));
        fogMat.SetVector("_BallPosition", ball.position);
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(blit)
        {
            Graphics.Blit(source, destination, fogMat);
        }
    }
    Vector2 CalculateCamsize(float orthoSize)
    {
        float height = orthoSize * 2; //camera height in units
        float width = height * Camera.main.aspect; //camera width in units
        return new Vector2(width, height);
    }
}

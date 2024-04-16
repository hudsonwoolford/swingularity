using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BoundsVisual : MonoBehaviour
{
    [SerializeField] Material boundsMat;
    LineRenderer lr;
    Vector4[] boundPoints;
    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
        boundPoints = Array.ConvertAll<Vector2, Vector4>(CameraFollow.innerShape.GetPath(0), t => new Vector4(t.x, t.y, 0, 0));
        lr.positionCount = boundPoints.Length + 1;
        for (int i = 0; i < boundPoints.Length; i++)
        {
            lr.SetPosition(i, boundPoints[i]);
        }
        lr.SetPosition(boundPoints.Length, boundPoints[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(boundsMat == null) { return; }
        Camera mainCam = Camera.main;
        float height = mainCam.orthographicSize * 2; //camera height in units
        float width = height * mainCam.aspect;       //camera width in units

        Vector3 camSize = new Vector3(width, height, 0);
        boundsMat.SetVectorArray("polygonP", boundPoints);
        boundsMat.SetVector("_CamSize", new Vector2(width, height));
        Graphics.Blit(source, destination, boundsMat);
    }
}

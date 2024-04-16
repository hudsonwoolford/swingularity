using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsVisual : MonoBehaviour
{
    [SerializeField] Material boundsMat;
    Vector4[] boundPoints;
    // Start is called before the first frame update
    void Start()
    {
        boundPoints = Array.ConvertAll<Vector2, Vector4>(CameraFollow.innerShape.GetPath(0), t => new Vector4(t.x, t.y, 0, 0));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(boundsMat == null) { return; }
        boundsMat.SetVectorArray("polygonP", boundPoints);
        Graphics.Blit(source, destination, boundsMat);
    }
}

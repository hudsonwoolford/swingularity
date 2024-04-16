using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class BoundsVisual : MonoBehaviour
{
    [SerializeField] Material boundsMat;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(boundsMat == null) { return; }
        Graphics.Blit(source, destination, boundsMat);
    }
}

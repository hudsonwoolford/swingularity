using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class CameraSettings : MonoBehaviour
{
    void Update()
    {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
    }
}

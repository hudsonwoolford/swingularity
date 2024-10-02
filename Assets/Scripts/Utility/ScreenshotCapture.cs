using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotCapture : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A)){
            ScreenCapture.CaptureScreenshot(Application.dataPath + "/Screenshot" + ".png");
        }
    }
}

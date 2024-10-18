using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotCapture : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            ScreenCapture.CaptureScreenshot(Application.dataPath + "/Screenshot.png");
        }
    }
}

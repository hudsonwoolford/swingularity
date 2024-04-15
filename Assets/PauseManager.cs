using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    Canvas pausecanvas;
    private void Awake()
    {
        pausecanvas = GetComponent<Canvas>();
    }
    // Start is called before the first frame update
    void Start()
    {
        pausecanvas.renderMode = RenderMode.ScreenSpaceCamera;
        pausecanvas.worldCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

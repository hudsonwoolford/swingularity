using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class HackMode : MonoBehaviour
{
    [SerializeField] Transform env;
    [Header("Fabric Var")]
    [SerializeField] bool showInEditor = false, updateInPlaymode;
    [SerializeField] Transform lrParent;
    [SerializeField] int width = 38, height = 24;
    [SerializeField] float cellSize = 0.5f, maxMult = 5, divisor = 20;
    [SerializeField] Color gridColor = Color.green;
    [SerializeField] Vector3 origin = Vector3.zero;
    [Header("Post Var")]
    [SerializeField] bool blitInEditor;
    [SerializeField] Material blurEffect;
    [SerializeField] Material edgeEffect;
    bool hackMode = false;
    Rigidbody ball;
    LineRenderer ylr, xlr;
    float lrOpacity;
    private void Awake()
    {
        ball = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if(lrParent == null) { return; }
        if (ylr == null || xlr == null) { ylr = lrParent.GetComponentsInChildren<LineRenderer>()[0]; xlr = lrParent.GetComponentsInChildren<LineRenderer>()[1]; }
        Rigidbody[] planets = env.GetComponentsInChildren<Rigidbody>();
        lrOpacity = xlr.startColor.a;

        ResetFabric();
        EnableFabric(false);
        UpdateFabric(planets);
    }
    private void Update()
    {
        if(env==null) { return; }
        if(lrParent == null) { return; }
        if (ylr == null || xlr == null) { ylr = lrParent.GetComponentsInChildren<LineRenderer>()[0]; xlr = lrParent.GetComponentsInChildren<LineRenderer>()[1]; }
        Rigidbody[] planets = env.GetComponentsInChildren<Rigidbody>();
        if (!Application.isPlaying)
        {
            if(!showInEditor) { EnableFabric(false); return; }

            UpdateFabric(planets);
            EnableFabric(true);
            return;
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            hackMode = !hackMode;
        }
        EnableFabric(hackMode);
        if(hackMode && updateInPlaymode) { UpdateFabric(planets); }
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (blurEffect == null || edgeEffect == null) { Graphics.Blit(source, destination); return; }
        if (!blitInEditor) { if (!hackMode) { Graphics.Blit(source, destination); return; } }
        RenderTexture blur = new RenderTexture(source.width, source.height, source.depth);
        Graphics.Blit(source, blur, blurEffect);
        Graphics.Blit(blur, destination, edgeEffect);
        blur.Release();
    }
    void ColorFabric(Color color)
    {
        xlr.startColor = color;
        xlr.endColor = color;
        ylr.startColor = color;
        ylr.endColor = color;
    }
    void ResetFabric()
    {
        ylr.positionCount = 0;
        xlr.positionCount = 0;
    }
    void EnableFabric(bool enable)
    {
        xlr.enabled = enable;
        ylr.enabled = enable;
    }
    void UpdateFabric(Rigidbody[] planets)
    {
        Grid grid = new Grid(width, height, cellSize, 1, maxMult, divisor, origin, planets, ball, xlr, ylr);
    }
}

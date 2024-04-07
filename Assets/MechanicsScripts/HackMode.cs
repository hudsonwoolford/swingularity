using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode()]
public class HackMode : MonoBehaviour
{
    [SerializeField] Transform env;
    [Header("Fabric Var")]
    [SerializeField] Transform lrParent;
    [SerializeField] bool showInEditor = false, updateInPlaymode;
    [SerializeField] int width = 38, height = 24;
    [SerializeField] float cellSize = 0.5f, maxMult = 5, divisor = 20;
    [SerializeField] Vector3 origin = Vector3.zero;
    [Header("Post Var")]
    [SerializeField] float fadeTime;
    [SerializeField] bool blitInEditor;
    [SerializeField] Material blurEffect;
    [SerializeField] Material edgeEffect;
    bool hackMode = false;
    Rigidbody ball;
    LineRenderer ylr, xlr;
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

        ResetFabric();
        EnableFabric(false);
        UpdateFabric(planets);
    }
    private void Update()
    {
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
            //StopAllCoroutines();
            //StartCoroutine(FadeHackMode())
        }
        EnableFabric(hackMode);
        if(hackMode && updateInPlaymode) { UpdateFabric(planets); }
    }
    IEnumerator FadeHackMode(bool enable)
    {

        yield return null;
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

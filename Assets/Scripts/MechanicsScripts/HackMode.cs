using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.VirtualTexturing;

public class HackMode : MonoBehaviour
{
    [SerializeField] Transform env;
    [Header("Fabric Var")]
    [SerializeField] bool updateInPlaymode;
    [SerializeField] int width = 38, height = 24;
    [SerializeField] float cellSize = 0.5f;
    [SerializeField] Color gridColor = Color.green;
    [SerializeField] Vector3 origin = Vector3.zero;
    [Header("Post Var")]
    [SerializeField] bool blitInEditor;
    [SerializeField] Material blurEffect;
    [SerializeField] Material edgeEffect;
    [SerializeField] Material gravityGrid;
    [SerializeField] Material whiteMat;
    bool hackMode = false;
    Rigidbody ball;
    public delegate void OnHackMode(bool enable, Material whiteMaterial);
    public static OnHackMode onHackMode;

    private Mesh gridMesh;
    private MeshFilter gridMeshFilter;
    private MeshRenderer gridMeshRend;
    private void Awake()
    {
        ball = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody[] planets = env.GetComponentsInChildren<Rigidbody>();

        InitialiseMesh();
        UpdateFabric(planets);
        RegenFabric();
        EnableFabric(false);
    }
    void LevelGenerated(){
        if(GameManager.LevelManager.currentTheme == null || edgeEffect == null){return;}
        edgeEffect.SetColor("_EdgeColor", GameManager.LevelManager.currentTheme.hackModeColor);
    }
    private void Update()
    {
        if(env==null) { return; }
        Rigidbody[] planets = env.GetComponentsInChildren<Rigidbody>();

        if (Input.GetKeyDown(KeyCode.H))
        {
            hackMode = !hackMode;
            RegenFabric();
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
    void EnableFabric(bool enable)
    {
        onHackMode?.Invoke(enable, whiteMat);
        gridMeshRend.enabled = enable;
    }                       
    void UpdateFabric(Rigidbody[] planets)
    {
        Vector4[] planetInfo = new Vector4[512];
        Vector4[] realPlanets = Array.ConvertAll(env.GetComponentsInChildren<CelestialMass>(), r => new Vector4(r.transform.position.x, r.transform.position.y, r.mass, 0));
        for (int i = 0; i < realPlanets.Length; i++)
        {
            planetInfo[i] = realPlanets[i];
        }
        if (planetInfo.Length > 0)
        {
            gravityGrid.SetVectorArray("_Planets", planetInfo);
        }
        else
        {

        }
        gravityGrid.SetInt("_PlanetCount", realPlanets.Length);
        gravityGrid.SetFloat("_GConstant", ArtificialGravity.G);
    }
    void RegenFabric()
    {
        Vector3 size = CameraFollow.innerShape == null ? new Vector3(width, height, 0) : CameraFollow.innerShape.bounds.extents * 2;
        Vector3 centre = CameraFollow.innerShape == null ? Vector3.one : CameraFollow.innerShape.bounds.center;
        GenerateMesh(GenerateVerts(Mathf.CeilToInt(size.x / cellSize), Mathf.CeilToInt(size.y / cellSize), cellSize, centre), GenerateIndicies(Mathf.CeilToInt(size.x / cellSize), Mathf.CeilToInt(size.y / cellSize)));
    }

    #region Procedural Grid
    private Vector3[] GenerateVerts(int xSeg, int ySeg, float cellSize, Vector3 centre)
    {
        List<Vector3> verts = new List<Vector3>();
        float xOffset = xSeg * cellSize / 2;
        float yOffset = ySeg * cellSize / 2;
        for (float y = 0; y <= ySeg; y++)
        {
            for (float x = 0; x <= xSeg; x++)
            {
                verts.Add(centre + new Vector3(x * cellSize - xOffset, y * cellSize - yOffset));
            }
        }
        return verts.ToArray();
    }

    private int[] GenerateIndicies(int xSeg, int ySeg)
    {
        List<int> indicies = new List<int>();
        for (int vert = 0, y = 0; y < ySeg; y++)
        {

            for (int x = 0; x < xSeg; x++)
            {
                indicies.Add(vert + 0);
                indicies.Add(vert + 1 + xSeg);

                indicies.Add(vert + 1 + xSeg);
                indicies.Add(vert + 2 + xSeg);

                indicies.Add(vert + 2 + xSeg);
                indicies.Add(vert + 1);

                indicies.Add(vert + 1);
                indicies.Add(vert + 0);

                vert++;
            }
            vert++;
        }
        return indicies.ToArray();
    }
    void InitialiseMesh()
    {
        GameObject gridObject = new GameObject("Gravity Grid");
        gridObject.transform.position += new Vector3(0, 0, 10);
        gridMeshFilter = gridObject.AddComponent<MeshFilter>();
        gridMeshRend = gridObject.AddComponent<MeshRenderer>();
    }
    private void GenerateMesh(Vector3[] verts, int[] indices)
    {
        gridMesh = new Mesh();
        gridMesh.Clear();
        gridMesh.name = "Grid Mesh";
        gridMesh.vertices = verts;
        gridMesh.SetIndices(indices, MeshTopology.Lines, 0);
        gridMeshFilter.mesh = gridMesh;
        gridMeshRend.material = gravityGrid == null ? new Material(Shader.Find("Sprites/Default")) { color = Color.magenta } : gravityGrid;
    }
    void OnEnable(){
        LevelGenerator.levelGenerated += LevelGenerated;
    }   
    void OnDisable(){
        LevelGenerator.levelGenerated -= LevelGenerated;
    }

    #endregion
}

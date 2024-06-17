using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NebulaGenerator : MonoBehaviour
{
    [SerializeField] ParticleSystem nebula1;
    [SerializeField] ParticleSystem nebula2;
    Vector2[] points = null;
    // Start is called before the first frame update
    void Start()
    {
        if(CameraFollow.innerShape != null) { CalculateNebulaSize(); }
        if(GameManager.LevelManager.currentTheme != null) { ApplyParticleColors(); }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            ApplyParticleColors();
        }
    }
    void CalculateNebulaSize()
    {
        points = CameraFollow.innerShape.points;
        if(points.Length <= 0) { Debug.LogWarning("CameraFollow inner shape contains no points. This is likely due to it not being initialised before this function was called."); return; } //safeguard
        Vector3[] vertices = new Vector3[12]; //6 * 2 points
        for (int i = 0; i < points.Length; i++)
        {
            vertices[i] = new Vector3(points[i].x, points[i].y, 0.5f);
            vertices[i + points.Length] = new Vector3(points[i].x, points[i].y, -0.5f);
        }
        int[] tris = new int[12 * 3]{
            0, 6, 1,
            6, 7, 1,
            1, 7, 2,
            7, 8, 2,
            2, 8, 3,
            8, 9, 3,
            3, 9, 4,
            9, 10, 4,
            4, 10, 5,
            10, 11, 5,
            5, 11, 0,
            11, 6, 0
        };

        Mesh nebulaMesh = new Mesh();
        nebulaMesh.vertices = vertices;
        nebulaMesh.triangles = tris;
        var shape1 = nebula1.shape;
        var shape2 = nebula2.shape;
        shape1.mesh = nebulaMesh;
        shape2.mesh = nebulaMesh;
        shape1.meshShapeType = ParticleSystemMeshShapeType.Edge;
        shape2.meshShapeType = ParticleSystemMeshShapeType.Edge;
        transform.position = Vector3.zero;
        nebula1.Clear();
        nebula1.Play();
        nebula2.Clear();
        nebula2.Play();
    }
    void ApplyParticleColors()
    {
        ParticleSystem.MainModule main1 = nebula1.main;
        ParticleSystem.MainModule main2 = nebula2.main;
        GalaxyData theme = GameManager.LevelManager.currentTheme;
        if(theme == null) { Debug.LogWarning("Galaxy theme is null. If this is in procedural check how level generator is setting it, otherwise check GalaxyLevels in Solar Select screens."); return; } //safeguard
        main1.startColor = new ParticleSystem.MinMaxGradient(theme.nebulaColors[0], theme.nebulaColors[1]);
        main2.startColor = new ParticleSystem.MinMaxGradient(theme.nebulaColors[2], theme.nebulaColors[3]);
        nebula1.Clear();
        nebula1.Play();
        nebula2.Clear();
        nebula2.Play();
    }
    private void OnEnable()
    {
        LevelGenerator.levelGenerated += CalculateNebulaSize;
        LevelGenerator.levelGenerated += ApplyParticleColors;
    }
    private void OnDisable()
    {
        LevelGenerator.levelGenerated -= CalculateNebulaSize;
        LevelGenerator.levelGenerated -= ApplyParticleColors;
    }
}

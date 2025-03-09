using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NebulaGenerator : MonoBehaviour
{
    [SerializeField] protected ParticleSystem nebula1;
    [SerializeField] protected ParticleSystem nebula2;
    Vector2[] points = null;

    void Start()
    {
        if(CameraFollow.nebulaShape != null) { CalculateNebulaSize(); }
        if(GameManager.LevelManager.currentTheme != null) { ApplyParticleColors(); }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            ApplyParticleColors();
        }
    }
    protected virtual void CalculateNebulaSize()
    {
        points = CameraFollow.nebulaShape;
        if(points.Length <= 0) { Debug.LogWarning("CameraFollow nebula shape contains no points. This is likely due to it not being initialised before this function was called."); return; } //safeguard
        Vector3[] vertices = new Vector3[8]; //6 * 2 points
        for (int i = 0; i < points.Length; i++)
        {
            vertices[i] = new Vector3(points[i].x, points[i].y, 0.5f);
            vertices[i + points.Length] = new Vector3(points[i].x, points[i].y, -0.5f);
        }

        Mesh nebulaMesh = new Mesh();
        nebulaMesh.vertices = vertices;
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
    protected virtual void ApplyParticleColors()
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

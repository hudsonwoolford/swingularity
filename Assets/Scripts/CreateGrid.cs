using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

public class CreateGrid : MonoBehaviour
{
    public Transform env;
    [Space]
    public int xSegments = 8;
    public int ySegments = 8;
    [Space]
    public float xStep = 1.0f;
    public float yStep = 1.0f;
    [Space]
    public Material material;
    [Space]
    public bool isCentered = true;

    private float _xOffset = 0.0f;
    private float _zOffset = 0.0f;

    private readonly List<Vector3> _verticies = new List<Vector3>();
    private readonly List<int> _indicies = new List<int>();

    private MeshFilter _meshFilter;
    private Mesh _mesh;
    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        if (isCentered)
        {
            _xOffset = xSegments * xStep / 2;
            _zOffset = ySegments * yStep / 2;
        }

        _mesh = new Mesh();
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }


 
}
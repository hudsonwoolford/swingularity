using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class MoonManager : MonoBehaviour
{
    [SerializeField] Transform orbitPlanet;
    [SerializeField] float radiusOffset = 0.5f;
    [SerializeField] float speed = 1;
    [SerializeField] float progress;
    [Header("Line Renderer")]
    [SerializeField] int points = 360;
    [SerializeField] float width = 0.1f;
    [SerializeField] Color lrColor = new Color(1,1,1,0.1f);
    [SerializeField] Material mat;
    float orbitRadius;
    LineRenderer lr;


    // Update is called once per frame
    void Update()
    {
        if (orbitPlanet == null)
        {
            return;
        }
        if(lr == null) { lr = GetComponent<LineRenderer>(); }
        if(lr == null) { lr = gameObject.AddComponent<LineRenderer>(); }

        if(Application.isPlaying)
        {
            progress += Time.deltaTime * speed;
            if(progress >= Mathf.PI * 2)
            {
                progress -= Mathf.PI * 2;
            }
        }
        orbitRadius = orbitPlanet.localScale.magnitude * 2 + radiusOffset;
        Vector3 dir = new Vector3(Mathf.Cos(progress), Mathf.Sin(progress));
        transform.position = orbitPlanet.position + dir * orbitRadius;

        DrawOrbit();
    }

    private void DrawOrbit()
    {
        lr.positionCount = points;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.textureMode = LineTextureMode.Tile;
        lr.textureScale = new Vector2(3, 1);
        lr.material = mat;
        lr.startColor = lrColor;
        lr.endColor = lrColor;
        float mockProgress = 0;
        for (int i = 0; i < points; i++)
        {
            mockProgress += Mathf.PI * 2 / points;
            Vector3 dir = new Vector3(Mathf.Cos(mockProgress), Mathf.Sin(mockProgress));
            lr.SetPosition(i, orbitPlanet.position + dir * orbitRadius);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (orbitPlanet == null)
        {
            return;
        }
        Transform t = orbitPlanet != null ? orbitPlanet : transform;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(t.position, t.localScale.magnitude * 2 + radiusOffset);
    }
}

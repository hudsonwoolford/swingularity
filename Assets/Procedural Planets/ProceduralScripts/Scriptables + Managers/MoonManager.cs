using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MoonManager : MonoBehaviour
{
    [SerializeField] Transform orbitPlanet;
    [SerializeField] float radiusOffset = 0.5f;
    [SerializeField] float speed = 1;
    [SerializeField] float progress;
    [SerializeField] float semiMajorAxis, gravitationalConstant, eccentricity;
    [Header("Line Renderer")]
    [SerializeField] int points = 360;
    [SerializeField] float width = 0.1f;
    [SerializeField] Color lrColor = new Color(1,1,1,0.1f);
    [SerializeField] Material mat;
    float orbitRadius;
    LineRenderer lr;

    float periapsis, apoapsis;
    Rigidbody rb;

    void Start(){
        rb = gameObject.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
        periapsis = semiMajorAxis * (1 - eccentricity);
        apoapsis = semiMajorAxis * (1 + eccentricity);
        float semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1-Mathf.Pow(eccentricity, 2));
        float gravitationalParameter = gravitationalConstant * (rb.mass + orbitPlanet.GetComponent<Rigidbody>().mass);
        float velocity = Mathf.Sqrt(gravitationalParameter * ((2/apoapsis) - (1/semiMajorAxis)));
        Vector2 pos = new Vector2 (apoapsis, 0f) + new Vector2 (orbitPlanet.position.x, orbitPlanet.position.y);
        float tangentGradient = -(pos.x * Mathf.Pow(semiMajorAxis, 2)) / (pos.y * Mathf.Pow(semiMajorAxis, 2));
        transform.position = pos;
        Vector3 dir = new Vector3 (0, 1.5f, 0);
        rb.AddForce(velocity * dir, ForceMode.VelocityChange);
    }
    // Update is called once per frame
    void Update()
    {
        rb.AddForce(ArtificialGravity.AttractForce(rb, orbitPlanet.GetComponent<Rigidbody>()));
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
        // orbitRadius = orbitPlanet.localScale.magnitude * 2 + radiusOffset;
        // Vector3 dir = new Vector3(Mathf.Cos(progress), Mathf.Sin(progress));
        // transform.position = orbitPlanet.position + dir * orbitRadius;

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

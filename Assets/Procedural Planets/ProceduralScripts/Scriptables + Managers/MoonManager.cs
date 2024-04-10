using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteInEditMode]
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

    float periapsis, apoapsis, semiMinorAxis;
    Vector2 centre;
    Rigidbody rb;

    void Start(){
        if(rb == null) { rb = GetComponent<Rigidbody>(); }
        if(rb == null) { rb = gameObject.AddComponent<Rigidbody>(); }
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = false;
        periapsis = semiMajorAxis * (1 - eccentricity);
        apoapsis = semiMajorAxis * (1 + eccentricity);
        semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1-Mathf.Pow(eccentricity, 2));
        float centerDistance = semiMajorAxis * eccentricity;
        Vector2 focus1 = orbitPlanet.position;
        Vector2 focus2 = focus1 + new Vector2 (centerDistance * 2, 0f);
        centre = (focus1 + focus2)/2f;
        float gravitationalParameter = gravitationalConstant * (rb.mass + orbitPlanet.GetComponent<Rigidbody>().mass);
        float velocity = Mathf.Sqrt(gravitationalParameter * ((2/apoapsis) - (1/semiMajorAxis)));
        Vector2 pos = new Vector2 (semiMajorAxis, 0f) + centre;
        float tangentGradient = -(pos.x * Mathf.Pow(semiMajorAxis, 2)) / (pos.y * Mathf.Pow(semiMinorAxis, 2));
        transform.position = pos;
        Vector3 dir = new Vector3 (0, 1f, 0);
        rb.AddForce(velocity * dir, ForceMode.VelocityChange);
    }
    // Update is called once per frame
    void Update()
    {
        if (orbitPlanet == null)
        {
            return;
        }
        if(lr == null) { lr = GetComponent<LineRenderer>(); }
        if(lr == null) { lr = gameObject.AddComponent<LineRenderer>(); }

        DrawOrbit();
    }

    void FixedUpdate(){
         rb.AddForce(ArtificialGravity.AttractForce(rb, orbitPlanet.GetComponent<Rigidbody>()), ForceMode.Force);
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
            Vector3 dir = new Vector3(centre.x + semiMajorAxis * Mathf.Cos(mockProgress), centre.y + semiMinorAxis * Mathf.Sin(mockProgress));
            lr.SetPosition(i, dir);
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

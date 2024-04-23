using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof (Rigidbody), typeof(LineRenderer))]
[ExecuteInEditMode()]
public class MoonManager : MonoBehaviour
{
    [SerializeField] Transform orbitPlanet;
    [SerializeField] Transform associatedPlanet;
    [SerializeField] float semiMajorAxis, gravitationalConstant, eccentricity, speed;
    [Header("Line Renderer")]
    [SerializeField] int points = 360;
    [SerializeField] float width = 0.1f;
    [SerializeField] Color lrColor = new Color(1,1,1,0.1f);
    [SerializeField] Material mat;
    float orbitRadius;
    LineRenderer lr;

    float periapsis, apoapsis, semiMinorAxis, currentAnomaly;
    Vector2 centre;

    void Start(){
        periapsis = semiMajorAxis * (1 - eccentricity);
        apoapsis = semiMajorAxis * (1 + eccentricity);
        semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1-Mathf.Pow(eccentricity, 2));
        centre = orbitPlanet.position;
    }
    // Update is called once per frame
    void Update()
    {
        periapsis = semiMajorAxis * (1 - eccentricity);
        apoapsis = semiMajorAxis * (1 + eccentricity);
        semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2));
        centre = orbitPlanet.position;
        if (orbitPlanet == null)
        {
            return;
        }
        DrawOrbit();
        if(Application.isPlaying)
        {
            MoveMoon();
        }
    }
    private void MoveMoon(){
        float currentDistance = semiMajorAxis * (1 - eccentricity * eccentricity) / (1 + eccentricity * -Mathf.Cos(currentAnomaly));
        Vector3 dir = new Vector3(centre.x + semiMajorAxis * Mathf.Cos(currentAnomaly), centre.y + semiMinorAxis * Mathf.Sin(currentAnomaly));

        transform.position = dir;
        float gravitationalParameter = gravitationalConstant * (associatedPlanet.GetComponent<Rigidbody>().mass + orbitPlanet.GetComponent<Rigidbody>().mass);
        float velocity = Mathf.Sqrt(gravitationalParameter * ((2/apoapsis) - (1/semiMajorAxis)));

        float angularVelocity = velocity/currentDistance;
        currentAnomaly += angularVelocity * Time.deltaTime * speed;
        //Wrap around
        if (currentAnomaly >= Mathf.PI * 2)
        {
            currentAnomaly -= Mathf.PI * 2;
        }
    }

    private void DrawOrbit()
    {
        if(lr == null) { lr = GetComponent<LineRenderer>(); }
        if(lr == null) { lr = gameObject.AddComponent<LineRenderer>(); }
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
}

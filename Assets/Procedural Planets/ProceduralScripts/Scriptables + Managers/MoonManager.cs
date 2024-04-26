using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
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
    LineRenderer lr;

    float periapsis, apoapsis, semiMinorAxis, currentAnomaly;
    Vector2 centre;
    Rigidbody rb;

    float bigMass, moonMass;
    bool moonEnabled = false;

    public void Init(float semiMajorAxis, float gravitationalConstant, float eccentricity, float speed, Transform associatedPlanet, Transform orbitPlanet, Material moonLine){
        this.semiMajorAxis = semiMajorAxis;
        this.gravitationalConstant = gravitationalConstant;
        this.eccentricity = eccentricity;
        this.speed = speed;
        this.associatedPlanet = associatedPlanet;
        this.orbitPlanet = orbitPlanet;
        this.mat = moonLine;
        
        periapsis = semiMajorAxis * (1 - eccentricity);
        apoapsis = semiMajorAxis * (1 + eccentricity);
        semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1-Mathf.Pow(eccentricity, 2));
        centre = orbitPlanet.position;
        bigMass = orbitPlanet.GetComponent<CelestialMass>().mass;
        moonMass = associatedPlanet.GetComponent<CelestialMass>().mass;
        if(rb == null){
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.drag = 0;
        rb.angularDrag = 0;
        rb.useGravity = false;
        semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2));
        float centerDistance = semiMajorAxis * eccentricity;
        Vector2 focus1 = orbitPlanet.position;
        Vector2 focus2 = focus1 + new Vector2 (centerDistance * 2, 0f);
        centre = (focus1 + focus2)/2f;
        float gravitationalParameter = gravitationalConstant * (moonMass + bigMass);
        rb.position = focus1 + new Vector2(apoapsis, 0f);

        float velocity = Mathf.Sqrt(gravitationalParameter * ((2/apoapsis) - (1/semiMajorAxis)));
        rb.velocity = transform.up * velocity * 10f;
        DrawOrbit();
        moonEnabled = true;
    }

    void Start(){
        if(!moonEnabled){return;}

        // Init(this.semiMajorAxis, this.gravitationalConstant, this.eccentricity, this.speed, this.associatedPlanet, this.orbitPlanet, this.mat);
    }

    void FixedUpdate()
    {
        if(!moonEnabled){return;}

        if (orbitPlanet == null)
        {
            return;
        }
        
        MoveMoon();
    }
    private void MoveMoon(){
        transform.position = centre + new Vector2(semiMajorAxis, 0f);

        float currentDistance = semiMajorAxis * (1 - eccentricity * eccentricity) / (1 + eccentricity * -Mathf.Cos(currentAnomaly));
        Vector3 dir = new Vector3(centre.x + semiMajorAxis * Mathf.Cos(currentAnomaly), centre.y + semiMinorAxis * Mathf.Sin(currentAnomaly));

        float gravitationalParameter = gravitationalConstant * (moonMass + bigMass);
        float velocity = Mathf.Sqrt(gravitationalParameter * ((2/apoapsis) - (1/semiMajorAxis)));
        rb.AddForce(ArtificialGravity.AttractForce(bigMass / 100f, moonMass / 100f, orbitPlanet, associatedPlanet));

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

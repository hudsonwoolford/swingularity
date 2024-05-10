using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[ExecuteInEditMode()]
public class MoonManager : MonoBehaviour
{
    [SerializeField] CelestialMass orbitPlanet;
    [SerializeField] CelestialMass associatedPlanet;
    [SerializeField] float semiMajorAxis = 5, gravitationalConstant = 1, speed = 1;
    [Range(0, 1)]
    [SerializeField] float eccentricity = 0;
    [Header("Line Renderer")]
    [SerializeField] int points = 360;
    [SerializeField] float width = 0.1f;
    [SerializeField] Color lrColor = new Color(1,1,1,0.1f);
    [SerializeField] Material mat;
    LineRenderer lr;

    float periapsis, apoapsis, semiMinorAxis, currentAnomaly, rotation;
    Vector3 startingVelocity = Vector3.zero;
    Vector2 centre;
    Rigidbody rb;
    bool moonEnabled = false;
    

    public void Init(float semiMajorAxis, float gravitationalConstant, float eccentricity, float speed, CelestialMass associatedPlanet, CelestialMass orbitPlanet, Material moonLine, float rotation){
        this.semiMajorAxis = semiMajorAxis;
        this.gravitationalConstant = gravitationalConstant;
        this.eccentricity = 0;
        this.speed = speed;
        this.associatedPlanet = associatedPlanet;
        this.orbitPlanet = orbitPlanet;
        this.mat = moonLine;
        this.rotation = rotation;

        GetComponent<MeshRenderer>().enabled = false; //prevent shadows on planets
        associatedPlanet.gameObject.layer = LayerMask.NameToLayer("IgnoreSimulation");
        rb = GetComponent<Rigidbody>();
        if(rb == null){
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.drag = 0;
        rb.angularDrag = 0;
        rb.velocity = Vector3.zero;
        rb.useGravity = false;

        CalculateVariables();
        float gravitationalParameter = gravitationalConstant * (orbitPlanet.mass);
        rb.position = RotateAround(rotation, orbitPlanet.transform.position + new Vector3(apoapsis, 0f, 0f), orbitPlanet.transform.position);
        float velocity = Mathf.Sqrt(gravitationalParameter * ((2/apoapsis) - (1/semiMajorAxis)));
        startingVelocity = Quaternion.Euler(new Vector3(0, 0, rotation)) * transform.up * velocity;
        
        DrawOrbit();
    }
    void StartingForce()
    {
        StartCoroutine(StartingDelay());
    }
    IEnumerator StartingDelay()
    {
        GetComponent<MeshRenderer>().enabled = true;

        rb.position = RotateAround(rotation, orbitPlanet.transform.position + new Vector3(apoapsis, 0f, 0f), orbitPlanet.transform.position);
        rb.angularVelocity = new Vector3(0f, 2f, 1f);
        yield return new WaitForFixedUpdate();
        moonEnabled = true;
    }
    void CalculateVariables()
    {
        periapsis = semiMajorAxis * (1 - eccentricity);
        apoapsis = semiMajorAxis * (1 + eccentricity);
        semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2));
        centre = orbitPlanet.transform.position;
        semiMinorAxis = semiMajorAxis * Mathf.Sqrt(1 - Mathf.Pow(eccentricity, 2));
        float centerDistance = semiMajorAxis * eccentricity;
        Vector2 focus1 = orbitPlanet.transform.position;
        Vector2 focus2 = focus1 + new Vector2(centerDistance * 2, 0f);
        centre = (focus1 + focus2) / 2f;
    }
    static Vector3 RotateAround(float angleDegrees, Vector3 pointA, Vector3 pointB){
        float angleRadians = angleDegrees * Mathf.Deg2Rad;

        // Calculate the new position of the point after rotation
        float rotatedX = Mathf.Cos(angleRadians) * (pointA.x - pointB.x) - Mathf.Sin(angleRadians) * (pointA.y - pointB.y) + pointB.x;
        float rotatedY = Mathf.Sin(angleRadians) * (pointA.x - pointB.x) + Mathf.Cos(angleRadians) * (pointA.y - pointB.y) + pointB.y;

        // Update the position of the rotated point
        pointA = new Vector3(rotatedX, rotatedY, pointA.z);
        return pointA;
    }

    void Start(){

        if(moonEnabled){return;}
        if (orbitPlanet == null || associatedPlanet == null) { return; }
        Init(this.semiMajorAxis, this.gravitationalConstant, this.eccentricity, this.speed, this.associatedPlanet, this.orbitPlanet, this.mat, this.rotation);
    }
    private void Update()
    {
        if (orbitPlanet == null || associatedPlanet == null) { return; }

        if (!Application.isPlaying)
        {
            CalculateVariables();
            EditModeUpdate();
            return;
        }
    }
    void FixedUpdate()
    {
        if (orbitPlanet == null || associatedPlanet == null) { return; }

        MoveMoon();
    }
    private void EditModeUpdate()
    {
        transform.position = RotateAround(rotation, orbitPlanet.transform.position + new Vector3(apoapsis, 0f, 0), orbitPlanet.transform.position);
        DrawOrbit();
    }
    private void MoveMoon(){
        // rb.AddForce(ArtificialGravity.AttractForce(orbitPlanet.mass, associatedPlanet.mass, orbitPlanet.transform, associatedPlanet.transform, gravitationalConstant) / associatedPlanet.mass, ForceMode.Force);


        float currentDistance = semiMajorAxis * (1 - eccentricity * eccentricity) / (1 + eccentricity * -Mathf.Cos(currentAnomaly));
        Vector3 dir = new Vector3(centre.x + semiMajorAxis * Mathf.Cos(currentAnomaly), centre.y + semiMinorAxis * Mathf.Sin(currentAnomaly));

        float gravitationalParameter = gravitationalConstant * (associatedPlanet.mass + orbitPlanet.mass);
        float velocity = Mathf.Sqrt(gravitationalParameter * ((2/apoapsis) - (1/semiMajorAxis)));

        float angularVelocity = velocity/currentDistance;
        currentAnomaly += angularVelocity * Time.deltaTime * speed;
        //Wrap around
        if (currentAnomaly >= Mathf.PI * 2)
        {
            currentAnomaly -= Mathf.PI * 2;
        }

        rb.position = new Vector3(centre.x + semiMajorAxis * Mathf.Cos(currentAnomaly), centre.y + semiMinorAxis * Mathf.Sin(currentAnomaly));
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
            Vector3 dir = RotateAround(rotation, new Vector3(centre.x + semiMajorAxis * Mathf.Cos(mockProgress), centre.y + semiMinorAxis * Mathf.Sin(mockProgress)), orbitPlanet.transform.position);
            lr.SetPosition(i, dir);
        }
    }
    void OnEnable(){
        LevelGenerator.levelGenerated += StartingForce;
    }
    void OnDisable(){
        LevelGenerator.levelGenerated -= StartingForce;
    }
}

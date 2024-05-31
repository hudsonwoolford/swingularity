using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

[ExecuteInEditMode()]
public class SolarSystemGenerator : MonoBehaviour
{
    [System.Serializable]
    struct OrbitPlanet
    {
        public float size;
        public float radius;
        public float speed;
        [Range(-1f, 1f)]
        public float progress;
        public int seed;
        public MockPlanet script;
    }
    [SerializeField] Transform sun;
    [SerializeField] float sunSize = 1;
    [SerializeField] Transform mockPlanetParent;
    [SerializeField] GameObject mockPlanet;
    [SerializeField] OrbitPlanet[] orbitPlanets;
    [Header("Animation")]
    [SerializeField] Animator sunAnimator;
    [Header("Line Variables")]
    [SerializeField] Gradient lineColor;
    [SerializeField] float lineWidth = 0.2f;
    [SerializeField] Material lineMat;
    bool levelGen;
    float planetSpeed, planetVel;
    //animation
    bool hovering;
    Transform allPlanetParent;

    private void Start()
    {
        if (Application.isPlaying) { DrawAllCircles(); }
    }
    // Update is called once per frame
    void Update()
    {
        if(mockPlanet == null || mockPlanetParent == null || sun == null || orbitPlanets.Length <= 0) { return; }
        if(!Application.isPlaying) //EDITOR
        {
            MatchPlanets();
            CalibrateSystemVar();
        }
        else if(levelGen) //PLAYMODE + LEVEL HAS BEEN CREATED
        {
            if(allPlanetParent == null) { allPlanetParent = GameObject.Find("Planets").transform; }
            ButtonBehaviour();
            MovePlanets();
        }
    }
    void ButtonBehaviour()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float maxRad = 0;
        for (int i = 0; i < orbitPlanets.Length; i++)
        {
            maxRad = Mathf.Max(maxRad, orbitPlanets[i].radius);
        }
        hovering = Vector2.Distance(mousePos, sun.position) < maxRad;
    }
    void MovePlanets()
    {
        for (int i = 0; i < orbitPlanets.Length; i++)
        {
            if (orbitPlanets[i].script == null) { orbitPlanets[i].script = mockPlanetParent.GetChild(i).GetComponent<MockPlanet>(); }
            //Incrementing rotation
            float totalRadius = (orbitPlanets[i].script.transform.position - sun.transform.position).magnitude;
            float target = hovering ? orbitPlanets[i].speed * Time.deltaTime / 5 : orbitPlanets[i].speed * Time.deltaTime / 30;
            planetSpeed = Mathf.SmoothDamp(planetSpeed, target, ref planetVel, 1f);
            orbitPlanets[i].progress += planetSpeed;
            orbitPlanets[i].progress %= 1;

            //Rotating and setting position
            Vector3 dir = new Vector3(Mathf.Cos(orbitPlanets[i].progress * Mathf.PI * 2), Mathf.Sin(orbitPlanets[i].progress * Mathf.PI * 2), 0);
            allPlanetParent.GetChild(i).position = sun.transform.position + dir * totalRadius;
            allPlanetParent.GetChild(i).eulerAngles += new Vector3(0, 0, planetSpeed * 360);
        }
    }
    void CalibrateSystemVar()
    {
        //Sun variables
        sun.transform.localScale = Vector3.one * sunSize;
        //Calibrating variables on mock planets, position setting, seed setting, etc
        for (int i = 0; i < orbitPlanets.Length; i++)
        {
            if (orbitPlanets[i].script == null) { orbitPlanets[i].script = mockPlanetParent.GetChild(i).GetComponent<MockPlanet>(); }
            orbitPlanets[i].script.seed = orbitPlanets[i].seed;
            orbitPlanets[i].script.transform.localScale = Vector3.one * orbitPlanets[i].size;
            Vector3 dir = new Vector3(Mathf.Cos(orbitPlanets[i].progress * Mathf.PI * 2), Mathf.Sin(orbitPlanets[i].progress * Mathf.PI * 2), 0);
            orbitPlanets[i].script.transform.position = sun.transform.position + dir * orbitPlanets[i].radius;
        }
    }
    void MatchPlanets()
    {
        //Spawning and deleting mock planets to match orbit planet array size
        if (mockPlanetParent.childCount != orbitPlanets.Length)
        {
            while (orbitPlanets.Length < mockPlanetParent.childCount)
            {
                DestroyImmediate(mockPlanetParent.GetChild(0).gameObject);
            }
            while (orbitPlanets.Length > mockPlanetParent.childCount)
            {
                MockPlanet mp = Instantiate(mockPlanet, mockPlanetParent).GetComponent<MockPlanet>();
                orbitPlanets[mockPlanetParent.childCount - 1].script = mp;
            }
        }
    }
    //Draws a circle for every level
    void DrawAllCircles()
    {
        for (int i = 0; i < orbitPlanets.Length; i++)
        {
            DrawCircle(sun.position, (orbitPlanets[i].script.transform.position - sun.transform.position).magnitude, 90);
        }
    }
    //Draws a circle by spawning a linerenderer
    void DrawCircle(Vector3 centre, float radius, int subDivs)
    {
        centre.z = 0; // make sure on z = 0
        Transform obj = new GameObject("Circle").transform;
        obj.parent = transform;
        LineRenderer l = obj.AddComponent<LineRenderer>();
        UpdateLineRenderer(l, subDivs + 1);

        float radianJump = Mathf.PI * 2 / (float)subDivs; //radian jump per point depending on subdivs

        for (int i = 0; i < subDivs + 1; i++) //goes around circle and adds point
        {
            Vector3 dir = new Vector3(Mathf.Cos(i * radianJump), Mathf.Sin(i * radianJump), 10);
            Vector3 pos = centre + dir * radius;
            l.SetPosition(i, pos);
        }
    }
    // Updates all the linerender values to inspector values
    void UpdateLineRenderer(LineRenderer l, int posCount)
    {
        l.positionCount = posCount;
        l.startWidth = lineWidth;
        l.endWidth = lineWidth;
        l.colorGradient = lineColor;
        l.material = lineMat;
        l.sortingLayerName = "Background";

        l.textureScale = new Vector2(3, 1);
        l.textureMode = LineTextureMode.Tile;
    }
    void LevelGenerated()
    {
        levelGen = true;
    }
    private void OnEnable()
    {
        LevelGenerator.levelGenerated += LevelGenerated;
    }
    private void OnDisable()
    {
        LevelGenerator.levelGenerated += LevelGenerated;
    }
    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < orbitPlanets.Length; i++)
        {
            Gizmos.DrawWireSphere(sun.position, orbitPlanets[i].radius);
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] ColorSchemeStaticReference.PlanetType systemType;
    [SerializeField] int sunSeed;
    [SerializeField] SunGenerator sun;
    [SerializeField] float sunSize = 1;
    [SerializeField] Transform mockPlanetParent;
    [SerializeField] GameObject mockPlanet;
    [SerializeField] OrbitPlanet[] orbitPlanets;
    [SerializeField] Transform allPlanetParent;
    [Header("Button")]
    [SerializeField] bool unlocked = false;
    [SerializeField] string targetScene, requiredScene;
    [Header("Animation")]
    [SerializeField] float hoverSmoothness;
    [Header("Line Variables")]
    [SerializeField] Gradient lineColor;
    [SerializeField] float lineWidth = 0.2f;
    [SerializeField] float lineTiling = 3f;
    [SerializeField] Material lineMat;
    float[] planetVel;
    float[] planetSpeed;
    float sunVel;
    //animation
    bool hovering;
    bool clickDown; //used to get click behaviour - click only goes through if clickdown while hovering and clickup while hovering

    private void Start()
    {
        planetVel = new float[orbitPlanets.Length];
        planetSpeed = new float[orbitPlanets.Length];
        sun.RandomSun(sunSeed, systemType);
        CalibrateSystemVar();
        if (Application.isPlaying) { DrawAllCircles(); 
            if (GameManager.LevelManager.LevelCompleted(requiredScene))
            {
                unlocked = true;
            }
        }
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
        else if(allPlanetParent.childCount == orbitPlanets.Length) //PLAYMODE + LEVEL HAS BEEN CREATED
        {
            if(allPlanetParent == null) { allPlanetParent = GameObject.Find("Planets").transform; }
            if (unlocked) { ButtonBehaviour(); }
            MovePlanets();
        }
    }
    void ButtonBehaviour()
    {
        //Hovering animation
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float maxRad = 0;
        for (int i = 0; i < orbitPlanets.Length; i++)
        {
            maxRad = Mathf.Max(maxRad, orbitPlanets[i].radius);
        }
        bool dryHover = Vector2.Distance(mousePos, sun.transform.position) < maxRad;
        hovering = clickDown && dryHover ? false : dryHover;

        if(!unlocked) { return; } //The following behaviour loads scenes
        //Check for clicks
        if(Input.GetMouseButtonDown(0) && dryHover) { clickDown = true; }
        if(Input.GetMouseButtonUp(0))
        {
            if(!dryHover) { clickDown = false; }
            if(clickDown)
            {
                clickDown = false;
                SceneManager.LoadScene(targetScene);
            }
        }
    }
    void MovePlanets()
    {
        //Sun variables
        float targetSize = hovering ? sunSize : sunSize * 0.8f;
        sun.transform.localScale = Mathf.SmoothDamp(sun.transform.localScale.x, targetSize, ref sunVel, hoverSmoothness) * Vector3.one;
        for (int i = 0; i < orbitPlanets.Length; i++)
        {
            if (orbitPlanets[i].script == null) { orbitPlanets[i].script = mockPlanetParent.GetChild(i).GetComponent<MockPlanet>(); }
            //Incrementing rotation
            float totalRadius = (orbitPlanets[i].script.transform.position - sun.transform.position).magnitude * transform.localScale.x;
            float target = hovering ? orbitPlanets[i].speed : orbitPlanets[i].speed / 20;
            planetSpeed[i] = Mathf.SmoothDamp(planetSpeed[i], target, ref planetVel[i], hoverSmoothness);
            orbitPlanets[i].progress += planetSpeed[i] * Time.deltaTime;
            orbitPlanets[i].progress %= 1;

            //Rotating and setting position
            Vector3 dir = new Vector3(Mathf.Cos(orbitPlanets[i].progress * Mathf.PI * 2), Mathf.Sin(orbitPlanets[i].progress * Mathf.PI * 2), 0);
            allPlanetParent.GetChild(i).position = sun.transform.position + dir * totalRadius;
            allPlanetParent.GetChild(i).eulerAngles += new Vector3(0, 0, planetSpeed[i] * Time.deltaTime * 360);
        }
    }
    void CalibrateSystemVar()
    {
        sun.transform.localScale = sunSize * Vector3.one;
        //Calibrating variables on mock planets, position setting, seed setting, etc
        for (int i = 0; i < orbitPlanets.Length; i++)
        {
            if (orbitPlanets[i].script == null) { orbitPlanets[i].script = mockPlanetParent.GetChild(i).GetComponent<MockPlanet>(); }
            orbitPlanets[i].script.planetType = systemType;
            orbitPlanets[i].script.seed = orbitPlanets[i].seed;
            orbitPlanets[i].script.transform.localScale = Vector3.one * orbitPlanets[i].size;
            Vector3 dir = new Vector3(Mathf.Cos(orbitPlanets[i].progress * Mathf.PI * 2), Mathf.Sin(orbitPlanets[i].progress * Mathf.PI * 2), 0);
            orbitPlanets[i].script.transform.position = sun.transform.position + dir * orbitPlanets[i].radius * transform.localScale.x;
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
                mp.planetType = systemType;
                orbitPlanets[mockPlanetParent.childCount - 1].script = mp;
            }
        }
    }
    public string SystemLevel()
    {
        return targetScene;
    }
    #region Line Rendering
    //Draws a circle for every level
    void DrawAllCircles()
    {
        for (int i = 0; i < orbitPlanets.Length; i++)
        {
            DrawCircle(sun.transform.position, (orbitPlanets[i].script.transform.position - sun.transform.position).magnitude, 90);
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

        l.textureScale = new Vector2(lineTiling, 1);
        l.textureMode = LineTextureMode.Tile;
    }
    #endregion
    private void OnEnable()
    {
    }
    private void OnDisable()
    {
    }
    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < orbitPlanets.Length; i++)
        {
            Gizmos.DrawWireSphere(sun.transform.position, orbitPlanets[i].radius * transform.localScale.x);
        }
    }
}


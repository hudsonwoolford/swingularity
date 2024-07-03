using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GalaxyMenu : MonoBehaviour
{
    [SerializeField] ColorSchemeStaticReference.PlanetType[] galaxyTypes;
    [SerializeField] Animator galaxy1, galaxy2;
    [SerializeField] GalaxyGenerator gGen1, gGen2;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Button right, left;
    LevelManager.GalaxyParams[] proceduralGalaxyList;
    EventSystem eventSystem;
    //List<LineRenderer> hexagons = new List<LineRenderer>();
    int currentGalaxy;
    bool swap;
    // Start is called before the first frame update
    void Start()
    {
        eventSystem = FindObjectOfType<EventSystem>();
        currentGalaxy = 0;
        swap = false;
        galaxy1.Play("GalaxyDisplay");
        galaxy2.Play("GalaxyIdle");
        nameText.text = gGen1.SetScheme(galaxyTypes[currentGalaxy]).name;
        right.onClick.AddListener(RightGalaxy);
        left.onClick.AddListener(LeftGalaxy);

        if(GameManager.LevelManager.procedural)
        {
            CreateProceduralGalaxies();
            LevelManager.GalaxyParams galaxyParams = proceduralGalaxyList[0];
            gGen1.SetProceduralScheme(galaxyParams);
            nameText.text = galaxyParams.name;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(eventSystem.currentSelectedGameObject != null) { eventSystem.SetSelectedGameObject(null); }

    }
    void RightGalaxy()
    {
        currentGalaxy += 1;
        currentGalaxy %= !GameManager.LevelManager.procedural ? galaxyTypes.Length : proceduralGalaxyList.Length;
        Animator x = swap ? galaxy2 : galaxy1; //Enter galaxy
        Animator y = swap ? galaxy1 : galaxy2; //Exit galaxy
        GalaxyGenerator gen = swap ? gGen1 : gGen2;
        if(GameManager.LevelManager.procedural)
        {
            LevelManager.GalaxyParams galaxyParams = proceduralGalaxyList[currentGalaxy];
            gen.SetProceduralScheme(galaxyParams);
            nameText.text = galaxyParams.name;
        }
        else
        {
            Galaxy.GalaxyScheme scheme = gen.SetScheme(galaxyTypes[currentGalaxy]);
            nameText.text = scheme.name;
        }

        y.Play("GalaxyEnter");
        x.Play("GalaxyExit");
        swap = !swap;
    }
    void LeftGalaxy()
    {
        currentGalaxy += -1;
        if(!GameManager.LevelManager.procedural && currentGalaxy < 0) { currentGalaxy = galaxyTypes.Length-1; }
        if(GameManager.LevelManager.procedural && currentGalaxy < 0) { currentGalaxy = proceduralGalaxyList.Length-1; }
        Animator x = swap ? galaxy2 : galaxy1; //Exit galaxy
        Animator y = swap ? galaxy1 : galaxy2; //Enter galaxy
        GalaxyGenerator gen = swap ? gGen1 : gGen2;
        if (GameManager.LevelManager.procedural)
        {
            LevelManager.GalaxyParams galaxyParams = proceduralGalaxyList[currentGalaxy];
            gen.SetProceduralScheme(galaxyParams);
            nameText.text = galaxyParams.name;
        }
        else
        {
            Galaxy.GalaxyScheme scheme = gen.SetScheme(galaxyTypes[currentGalaxy]);
            nameText.text = scheme.name;
        }

        x.Play("GalaxyEnterR");
        y.Play("GalaxyExitR");
        swap = !swap;
    }
    void CreateProceduralGalaxies()
    {
        string st = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        LevelManager.UniverseParams universe = GameManager.LevelManager.currentUniverse; //retrieves current universe
        UnityEngine.Random.InitState(universe.seed);
        LevelManager.GalaxyParams[] galaxies = new LevelManager.GalaxyParams[universe.numGalaxies]; //initialize galaxy array
        for (int i = 0; i < galaxies.Length; i++) //for each galaxy index, create one
        {
            UnityEngine.Random.InitState(universe.seed + i); //different random values
            //random name, temporary
            string name = "";
            for (int c = 0; c < UnityEngine.Random.Range(3,10); c++) 
            {
                name += st[UnityEngine.Random.Range(0, st.Length)];
            }
            //random planet types
            ColorSchemeStaticReference.PlanetType[] possibleTypes = new ColorSchemeStaticReference.PlanetType[UnityEngine.Random.Range(1,3)];
            for (int t = 0; t < possibleTypes.Length; t++) 
            {
                possibleTypes[t] = galaxyTypes[UnityEngine.Random.Range(0, galaxyTypes.Length)];
            }
            galaxies[i] = new LevelManager.GalaxyParams(name, UnityEngine.Random.Range(-2147483647, 2147483647), universe.relativeDifficulty, possibleTypes, universe.levelStatic); //setting galaxy at index
        }
        LevelManager.setProceduralGalaxies(galaxies);
        proceduralGalaxyList = galaxies;
    }
}

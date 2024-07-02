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
    }

    // Update is called once per frame
    void Update()
    {
        if(eventSystem.currentSelectedGameObject != null) { eventSystem.SetSelectedGameObject(null); }

    }
    void RightGalaxy()
    {
        currentGalaxy += 1;
        currentGalaxy %= galaxyTypes.Length;
        Animator x = swap ? galaxy2 : galaxy1; //Enter galaxy
        Animator y = swap ? galaxy1 : galaxy2; //Exit galaxy
        GalaxyGenerator gen = swap ? gGen1 : gGen2;
        Galaxy.GalaxyScheme scheme = gen.SetScheme(galaxyTypes[currentGalaxy]);
        nameText.text = scheme.name;

        y.Play("GalaxyEnter");
        x.Play("GalaxyExit");
        swap = !swap;
    }
    void LeftGalaxy()
    {
        currentGalaxy += -1;
        if(currentGalaxy < 0) { currentGalaxy = galaxyTypes.Length-1; }
        Animator x = swap ? galaxy2 : galaxy1; //Exit galaxy
        Animator y = swap ? galaxy1 : galaxy2; //Enter galaxy
        GalaxyGenerator gen = swap ? gGen1 : gGen2;
        Galaxy.GalaxyScheme scheme = gen.SetScheme(galaxyTypes[currentGalaxy]);
        nameText.text = scheme.name;

        x.Play("GalaxyEnterR");
        y.Play("GalaxyExitR");
        swap = !swap;
    }
}

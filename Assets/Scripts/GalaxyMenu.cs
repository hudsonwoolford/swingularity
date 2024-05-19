using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GalaxyMenu : MonoBehaviour
{
    [SerializeField] ColorSchemeStaticReference.PlanetType[] galaxyTypes;
    [SerializeField] Animator galaxy1, galaxy2;
    [SerializeField] GalaxyGenerator gGen1, gGen2;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Button right, left;
    int currentGalaxy;
    bool swap;
    // Start is called before the first frame update
    void Start()
    {
        currentGalaxy = 0;
        swap = false;
        galaxy1.Play("GalaxyDisplay");
        galaxy2.Play("GalaxyIdle");
        nameText.text = gGen1.SetScheme(galaxyTypes[currentGalaxy]);
        right.onClick.AddListener(RightGalaxy);
        left.onClick.AddListener(LeftGalaxy);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void RightGalaxy()
    {
        currentGalaxy += 1;
        currentGalaxy %= galaxyTypes.Length;
        Animator x = swap ? galaxy2 : galaxy1; //Enter galaxy
        Animator y = swap ? galaxy1 : galaxy2; //Exit galaxy
        GalaxyGenerator gen = swap ? gGen1 : gGen2;
        nameText.text = gen.SetScheme(galaxyTypes[currentGalaxy]);

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
        nameText.text = gen.SetScheme(galaxyTypes[currentGalaxy]);
        
        x.Play("GalaxyEnterR");
        y.Play("GalaxyExitR");
        swap = !swap;
    }

}

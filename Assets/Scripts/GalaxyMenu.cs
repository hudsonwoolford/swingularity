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
    //[Header("Lines")]
    //[SerializeField] Material lrMat;
    //[SerializeField] float lrWidth;
    //[SerializeField] float lrRadius;
    [SerializeField] float hexagonRangeMult;
    GalaxyGenerator currentGenerator;
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
        currentGenerator = swap ? gGen1 : gGen2;
        if(eventSystem.currentSelectedGameObject != null) { eventSystem.SetSelectedGameObject(null); }
        if(Input.GetKeyDown(KeyCode.Space))
        {
            string targetScene = currentGenerator.SetScheme(galaxyTypes[currentGalaxy]).levelScene;
            if (!string.IsNullOrEmpty(targetScene))
            {
                SceneManager.LoadScene(targetScene);
            }
            else
            {
                Debug.LogWarning("LevelScene from '" + currentGenerator.SetScheme(galaxyTypes[currentGalaxy]).name +"' galaxy struct references an invalid scene: '" + targetScene+"'.");
            }
        }
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
    //#region Drawing Hexagons
    //void DrawGalaxyHexagons(int seed)
    //{
    //    for (int i = 0; i < 5; i++)
    //    {
    //        UnityEngine.Random.InitState(seed + i * 10);
    //        DrawHexagon(UnityEngine.Random.insideUnitCircle * hexagonRangeMult, lrRadius, i);
    //    }
    //}
    //void DrawHexagon(Vector3 center, float rad, int index)
    //{
    //    if (hexagons.Count <= index) { hexagons.Add(new GameObject("Hexagon " + index).AddComponent<LineRenderer>()); }
    //    LineRenderer lr = hexagons[index];
    //    lr.positionCount = 8;
    //    lr.material = lrMat;
    //    lr.startWidth = lrWidth;
    //    lr.endWidth = lrWidth;
    //    Vector3 bearing30 = new Vector3(Mathf.Cos(Mathf.PI/3), Mathf.Sin(Mathf.PI / 3), 0);
    //    Vector3 bearing330 = new Vector3(Mathf.Cos(2 * Mathf.PI / 3), Mathf.Sin(2 * Mathf.PI / 3), 0);

    //    lr.SetPosition(0, center + (bearing330) * rad);
    //    lr.SetPosition(1, center + (bearing30) * rad);
    //    lr.SetPosition(2, center +  Vector3.right * rad);
    //    lr.SetPosition(3, center + (-bearing330) * rad);
    //    lr.SetPosition(4, center + (-bearing30) * rad);
    //    lr.SetPosition(5, center +  Vector3.left * rad);
    //    lr.SetPosition(6, center + (bearing330) * rad);
    //    lr.SetPosition(7, center + (bearing30) * rad);
    //}
    //#endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using Unity.VisualScripting;
using System.Linq;

[ExecuteInEditMode()]
public class GalaxyLevels : MonoBehaviour
{
    [Header("System Variables")]
    [SerializeField] Transform systemPlanets;
    [SerializeField] GalaxyData galaxyTheme;
    //[Header("Line Variables")]
    //[SerializeField] Gradient lineColor;
    //[SerializeField] float lineWidth = 0.2f;
    //[SerializeField] float lineDist = 1f;
    //[SerializeField] float circleRad = 1f;
    //[SerializeField] Material lineMat;
    //LineRenderer[] lr = new LineRenderer[5];
    public delegate void InitGalaxy(string[] galaxyLevels, GalaxyData theme);
    public static InitGalaxy initGalaxy;
    private void Awake()
    {
    }
    // Start is called before the first frame update
    void Start()
    {
        if(!Application.isPlaying) { return; } //only do function in game
        //calling functions
        SolarSystemGenerator[] ssGens = systemPlanets.GetComponentsInChildren<SolarSystemGenerator>();
        string[] ssLevels = new string[ssGens.Length];
        for (int i = 0; i < ssGens.Length; i++)
        {
            ssLevels[i] = ssGens[i].SystemLevel();
        }
        initGalaxy?.Invoke(ssLevels, galaxyTheme);
    }
    private void Update()
    {
    }
    private void OnDrawGizmosSelected()
    {
    }
}

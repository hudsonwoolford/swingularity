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
    [SerializeField] Transform buttonsCanvas;
    SceneButton[] systemButtons; //solar system scenes
    [Header("Line Variables")]
    [SerializeField] Gradient lineColor;
    [SerializeField] float lineWidth = 0.2f;
    [SerializeField] float lineDist = 1f;
    [SerializeField] float circleRad = 1f;
    [SerializeField] Material lineMat;
    [SerializeField] GalaxyData galaxyTheme;
    LineRenderer[] lr = new LineRenderer[5];
    public delegate void InitGalaxy(string[] galaxyLevels, GalaxyData theme);
    public static InitGalaxy initGalaxy;
    private void Awake()
    {
        systemButtons = buttonsCanvas.GetComponentsInChildren<SceneButton>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if(!Application.isPlaying) { return; } //only do function in game
        if (buttonsCanvas == null) { return; } //crucial variable
        initGalaxy?.Invoke(System.Array.ConvertAll(systemButtons, s => s.sceneName), galaxyTheme);
        //calling functions
        SystemPositions();
        Linerendering();
        DrawAllCircles();
    }
    private void Update()
    {
        if (!Application.isPlaying) //Updates system positions in inspector
        {
            SystemPositions();
        }
    }
    void SystemPositions() //sets the positions of the buttons to be equally spaced on the x-axis
    {
        systemButtons = buttonsCanvas.GetComponentsInChildren<SceneButton>(); //all of the buttons

        int levelAmount = systemButtons.Length; //level amount
        float xJump = Screen.width / (levelAmount + 1); //calculating space between each level

        for (int i = 0; i < levelAmount; i++) //rendering each level
        {
            systemButtons[i].transform.position = new Vector3(xJump * (i + 1), systemButtons[i].transform.position.y); //setting system position according to xjump
        }

    }
    #region LineRenderers
    void Linerendering() //draws lines to connect each button
    {
        int levelAmount = systemButtons.Length; // level amount

        lr = new LineRenderer[levelAmount-1]; //line renderer array to connect them
        for (int i = 0; i < lr.Length; i++) //foreach
        {
            if (!systemButtons[i+1].unlocked) { break; } //will not draw line if locked
            //creating linerenderer object
            Transform obj = new GameObject("Line").transform;
            obj.parent = transform;
            lr[i] = obj.AddComponent<LineRenderer>();
        }

        //drawing every line renderer
        for (int i = 0; i < systemButtons.Length-1; i++)
        {
            if (!systemButtons[i + 1].unlocked) { return; } //will not draw line if locked

            Vector3 p1 = Camera.main.ScreenToWorldPoint(systemButtons[i].transform.position);   //first button
            p1.z = 0;
            Vector3 p2 = Camera.main.ScreenToWorldPoint(systemButtons[i+1].transform.position);//second button
            p2.z = 0;

            Vector3 pos1 = p1 + (p2 - p1).normalized * lineDist; //button 1 pos, taking into account offset
            Vector3 pos2 = p2 + (p1 - p2).normalized * lineDist;//button 2 pos, taking into account offset

            UpdateLineRenderer(lr[i], 2); //updating values
            lr[i].SetPosition(0, pos1); //Assign pos1
            lr[i].SetPosition(1, pos2); //Assign pos2
        }
    }
    //Draws a circle for every level
    void DrawAllCircles()
    {
        for (int i = 0; i < systemButtons.Length; i++)
        {
            if (!systemButtons[i].unlocked) { return; }
            DrawCircle(Camera.main.ScreenToWorldPoint(systemButtons[i].transform.position), circleRad, 180);
        }
    }
    //Draws a circle by spawning a linerenderer
    void DrawCircle(Vector3 centre, float radius, int subDivs)
    {
        centre.z = 0; // make sure on z = 0
        Transform obj = new GameObject("Circle").transform;
        obj.parent = transform;
        LineRenderer l =  obj.AddComponent<LineRenderer>();
        UpdateLineRenderer(l, subDivs + 1);

        float radianJump = Mathf.PI * 2 / (float)subDivs; //radian jump per point depending on subdivs

        for (int i = 0; i < subDivs + 1; i++) //goes around circle and adds point
        {
            Vector3 dir = new Vector3(Mathf.Cos(i * radianJump), Mathf.Sin(i * radianJump), 0);
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

        l.textureScale = new Vector2(3, 1);
        l.textureMode = LineTextureMode.Tile;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
    }
}

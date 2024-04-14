using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using Unity.VisualScripting;
using System.Linq;

[ExecuteInEditMode()]
public class LevelSelect : MonoBehaviour
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
    LineRenderer[] lr = new LineRenderer[5];
    private void Awake()
    {
        systemButtons = buttonsCanvas.GetComponentsInChildren<SceneButton>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if(!Application.isPlaying) { return; } //only do function in game
        if (buttonsCanvas == null) { return; } //crucial variable
        //calling functions
        SystemPositions();
        Linerendering();
        DrawAllCircles();
    }
    private void Update()
    {
        if (!Application.isPlaying)
        {
            SystemPositions();
        }
    }
    void SystemPositions()
    {
        systemButtons = buttonsCanvas.GetComponentsInChildren<SceneButton>();

        int levelAmount = systemButtons.Length;

        float height = Camera.main.orthographicSize * 2; //camera height in units
        float width = height * Camera.main.aspect;       //camera width in units

        float xJump = width / (levelAmount + 1); //space between each level
        float leftEdge = 0 - width / 2; //left edge of screen x pos

        for (int i = 0; i < levelAmount; i++) //rendering each level
        {
            systemButtons[i].transform.position = new Vector3(leftEdge + xJump * (i + 1), systemButtons[i].transform.position.y); //setting system position
        }

    }
    void Linerendering()
    {
        int levelAmount = systemButtons.Length;

        lr = new LineRenderer[levelAmount-1];
        for (int i = 0; i < lr.Length; i++)
        {
            if (!systemButtons[i+1].unlocked) { break; }
            Transform obj = new GameObject().transform;
            obj.parent = transform;
            lr[i] = obj.AddComponent<LineRenderer>();
            obj.name = "Line";
        }

        //drawing every line renderer
        for (int i = 0; i < systemButtons.Length-1; i++)
        {
            if (!systemButtons[i + 1].unlocked) { return; }
            Transform t = systemButtons[i].transform;   //first button
            Transform t2 = systemButtons[i+1].transform;//second button

            Vector3 pos1 = t.position + (t2.position - t.position).normalized * lineDist; //button 1 pos, taking into account offset
            Vector3 pos2 = t2.position + (t.position - t2.position).normalized * lineDist;//button 2 pos, taking into account offset

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
            DrawCircle(systemButtons[i].transform.position, circleRad, 180);
        }
    }
    //Draws a circle by spawning a linerenderer
    void DrawCircle(Vector3 centre, float radius, int subDivs)
    {

        Transform obj = new GameObject().transform;
        obj.parent = transform;
        obj.name = "Circle";
        LineRenderer l =  obj.AddComponent<LineRenderer>();
        UpdateLineRenderer(l, subDivs + 1);

        float radianJump = Mathf.PI * 2 / (float)subDivs;

        for (int i = 0; i < subDivs + 1; i++)
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

    private void OnDrawGizmosSelected()
    {
    }
}

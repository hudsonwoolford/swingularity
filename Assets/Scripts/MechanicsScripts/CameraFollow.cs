using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Boundaries")]
    [SerializeField] Vector2 startPos; //startpos of path
    [SerializeField] Vector2 endPos; //endpos of path
    [Header("Camera Following")]
    [SerializeField] bool lockCamera;
    [SerializeField] Vector2 cameraMinMax = new Vector2(3, 10);
    [SerializeField] float smoothness = 0.3f;  //smoothness of camera movement
    [SerializeField] bool focusBall = true;
    PolygonCollider2D cameraPath;              //inner boundaries
    Transform ball;                            //ball transform
    Vector3 vel;
    float sizeVelocity;                              //camera velocity
    float cameraSize;
    public float defaultCameraSize;
    float targetCameraSize;
    public static PolygonCollider2D innerShape { get; private set; }
    public static bool panning { get; private set; } = false;

    public void InitPositions(Vector2 startPosSent, Vector2 endPosSent, float maxSizeSent){
        startPos = startPosSent;
        endPos = endPosSent;
        cameraMinMax.y = maxSizeSent;
        cameraSize = defaultCameraSize;
        targetCameraSize = cameraSize;

        transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);
        innerShape.points = PolygonPositions(startPos, endPos, 2f, 10); //to scale version
        if (cameraPath == null)
        {
            cameraPath = new GameObject("Camera Bounds")
            {
                tag = "CameraBounds"
            }.AddComponent<PolygonCollider2D>();
        }
        cameraPath.points = PolygonPositions(startPos, endPos, 0.5f, 0); //scaled down version of inner shape
    }   
    private void Awake()
    {
        ball = GameObject.FindGameObjectWithTag("Player").transform;
        innerShape = new GameObject("Inner Bounds").AddComponent<PolygonCollider2D>();
        innerShape.points = PolygonPositions(startPos, endPos, 2f, 10); //to scale version
    }
    // Start is called before the first frame update
    void Start()
    {
        cameraSize = defaultCameraSize;
        targetCameraSize = cameraSize;

        transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);
       
        if(cameraPath == null)
        {
            cameraPath = new GameObject("Camera Bounds"){
                tag = "CameraBounds"
            }.AddComponent<PolygonCollider2D>();
            cameraPath.points = PolygonPositions(startPos, endPos, 0.5f, 0); //scaled down version of inner shape
        }
    }

    // Update is called once per frame
    public void LateUpdate()
    {
        if (Input.GetMouseButtonDown(2))
        {
            StartCoroutine(Panning());
        }
        if ((Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(0)) && focusBall) 
        {
            targetCameraSize = defaultCameraSize;
        }
        if (!Input.GetMouseButton(0)){
            targetCameraSize -= Input.mouseScrollDelta.y;
        }
        targetCameraSize = Mathf.Clamp(targetCameraSize, cameraMinMax.x, cameraMinMax.y);
        Camera.main.orthographicSize = cameraSize;
        cameraSize = Mathf.SmoothDamp(cameraSize, targetCameraSize, ref sizeVelocity, smoothness);
        if (panning) { return; }
        if(lockCamera) { transform.position = Vector3.SmoothDamp(transform.position, new Vector3(startPos.x, startPos.y, -10), ref vel, smoothness); return; }
        Vector3 targetPos = Vector3.zero;
        if (!cameraPath.OverlapPoint(new Vector2(ball.position.x, ball.position.y))) //out of boundaries
        {
            Vector2 closestPoint = cameraPath.ClosestPoint(new Vector2(ball.position.x, ball.position.y)); //closest point on boundary perimeter to position
            targetPos = new Vector3(closestPoint.x, closestPoint.y, transform.position.z); //smoothdamp function for smoothness
        }
        else
        {
            targetPos = new Vector3(ball.position.x, ball.position.y, -10); //ball position in camera z
        }
        bool focusingBall = focusBall == true ? Input.GetMouseButton(0) : false;       
        if (cameraSize > defaultCameraSize && !focusingBall){
            float camLerpVal = cameraSize.Remap(defaultCameraSize, cameraMinMax.y, 0, 1);
            Vector2 pos = Vector2.Lerp(targetPos, (endPos + startPos)/2, camLerpVal);
            targetPos = new Vector3(pos.x, pos.y, -10);
        }                   
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref vel, smoothness);//smoothdamp function for smoothness
    }
    IEnumerator Panning()
    {
        panning = true;
        Vector2 startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        while (Input.GetMouseButton(2) && panning)
        {
            Vector2 mousPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 difference = mousPos - startMousePos;

            transform.position += -new Vector3(difference.x, difference.y);
            yield return null;
        }
        panning = false;
    }
    //Returns array of positions that makes up a polygon for the boundaries
    Vector2[] PolygonPositions(Vector2 start, Vector2 end, float scale, float feather)
    {
        if(start.x == end.x || start.y == end.y)
        {
            end += Vector2.one * 0.001f;
        }
        float height = Camera.main.orthographicSize * 2; //camera height in units
        float width = height * Camera.main.aspect;       //camera width in units
        height += feather;
        width += feather;
        Vector3 camSize = new Vector3(width, height);
        camSize *= scale; //scaling
        Vector2 dir = (end - start).normalized; //direction from start to end

        Vector2[] startCorners = { start + new Vector2(camSize.x / 2, camSize.y / 2), start + new Vector2(camSize.x / 2, -camSize.y / 2), start + new Vector2(-camSize.x / 2, -camSize.y / 2), start + new Vector2(-camSize.x / 2, camSize.y / 2) }; //start pos camera square vertices
        Vector2[] endCorners   = { end + new Vector2(camSize.x / 2, camSize.y / 2), end + new Vector2(camSize.x / 2, -camSize.y / 2), end + new Vector2(-camSize.x / 2, -camSize.y / 2), end + new Vector2(-camSize.x / 2, camSize.y / 2) }; //end pos camera square vertices

        Vector2[] finalPoints = new Vector2[6]; //returning 6 pointed polygon

        int i = 0; //i is chosen depending on the direction of the end square, as 1 vertice needs to be skipped in the polygon. There is 6 vertices not 8.
        if (dir.x > 0 && dir.y > 0) { i=1; }
        if (dir.x > 0 && dir.y < 0) { i=2; }
        if (dir.x < 0 && dir.y < 0) { i=3; }
        if (dir.x < 0 && dir.y > 0) { i=0; }
        for (int u = 0; u < 3; u++)
        {
            finalPoints[u] = startCorners[i];
            i++;
            if (i > 3) { i = 0; }
        }

        i = 0;//i is chosen depending on the direction of the start square, as 1 vertice needs to be skipped in the polygon. There is 6 vertices not 8.                                   
        dir = (start - end).normalized;
        if (dir.x > 0 && dir.y > 0) { i = 1; }
        if (dir.x > 0 && dir.y < 0) { i = 2; }
        if (dir.x < 0 && dir.y < 0) { i = 3; }
        if (dir.x < 0 && dir.y > 0) { i = 0; }
        for (int u = 3; u < 6; u++)
        {
            finalPoints[u] = endCorners[i];
            i++;
            if (i > 3) { i = 0; }
        }

        return finalPoints; //final points
    }
    void OnReset()
    {
        panning = false;
    }
    void ZoomIn(){
        targetCameraSize = cameraMinMax.x;
    }
    private void OnEnable()
    {
        ShootBall.triggerReset += OnReset;
        ShootBall.onHole += OnReset;
        ShootBall.holeAnim += ZoomIn;
    }
    private void OnDisable()
    {
        ShootBall.triggerReset -= OnReset;
        ShootBall.onHole -= OnReset;
        ShootBall.holeAnim += ZoomIn;
    }
    private void OnDrawGizmos()
    {
        //scene view visuals
        Gizmos.color = Color.green;
        Vector2[] polygon = PolygonPositions(startPos, endPos, 1, 0);
        for (int i = 0; i < polygon.Length-1; i++)
        {
            Gizmos.DrawLine(polygon[i], polygon[i + 1]);
        }
        Gizmos.DrawLine(polygon[polygon.Length - 1], polygon[0]);

        Gizmos.color = Color.white;
    }
}
public static class Toolkit {
    public static float Remap (this float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(CameraFollow))]
public class CameraFollowEditor : Editor
{
    SerializedProperty startPos, endPos;
    public void OnSceneGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        CameraFollow c = target as CameraFollow;
        Vector2 sp = Handles.FreeMoveHandle(startPos.vector2Value, 1, Vector3.one, Handles.DotHandleCap); //Handle for start pos
        Vector2 ep = Handles.FreeMoveHandle(endPos.vector2Value, 1, Vector3.one, Handles.DotHandleCap);   //Handle for end pos

        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(c, "Change Pos");
            startPos.vector2Value = sp;
            endPos.vector2Value = ep;

        }

        serializedObject.ApplyModifiedProperties();
    }
    private void OnEnable()
    {
        startPos = serializedObject.FindProperty("startPos");
        endPos = serializedObject.FindProperty("endPos");
    }
    
}
#endif

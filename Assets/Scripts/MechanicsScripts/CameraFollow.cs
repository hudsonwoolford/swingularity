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
    [SerializeField] Vector2 cameraMinMax = new Vector2(3, 10);
    [SerializeField] float bufferBox = 1;
    [SerializeField] float smoothness = 0.3f;  //smoothness of camera movement
    [SerializeField] float resistance = 1.3f;  //resistance when going out of bounds
    PolygonCollider2D cameraPath;              //inner boundaries
    public static PolygonCollider2D innerShape;
    Transform ball;                            //ball transform
    Vector3 vel;                               //camera velocity
    float cameraSize, defaultCameraSize;
    //Information event
    public static bool panning = false;
    private void Awake()
    {
        ball = GameObject.FindGameObjectWithTag("Player").transform;
        innerShape = new GameObject("Inner Bounds").AddComponent<PolygonCollider2D>();
        innerShape.points = PolygonPositions(startPos, endPos, 1f); //to scale version
    }
    // Start is called before the first frame update
    void Start()
    {
        defaultCameraSize = Camera.main.orthographicSize;
        cameraSize = defaultCameraSize;

        transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);
       

        cameraPath = new GameObject("Camera Bounds").AddComponent<PolygonCollider2D>();
        cameraPath.points = PolygonPositions(startPos, endPos, 0.4f); //scaled down version of inner shape
    }

    // Update is called once per frame
    public void Update()
    {
        if(Input.GetMouseButtonDown(2)) { cameraSize = defaultCameraSize;}
        cameraSize -= Input.mouseScrollDelta.y;
        cameraSize = Mathf.Clamp(cameraSize, cameraMinMax.x, cameraMinMax.y);
        Camera.main.orthographicSize = cameraSize;

        if(Input.GetMouseButtonDown(1))
        {
            StartCoroutine(Panning());
        }
        if(panning) { return; }
        if (!cameraPath.OverlapPoint(new Vector2(ball.position.x, ball.position.y))) //out of boundaries
        {
            Vector2 closestPoint = cameraPath.ClosestPoint(new Vector2(ball.position.x, ball.position.y));                     //closest point on boundary perimeter to position
            transform.position = Vector3.SmoothDamp(transform.position, new Vector3(closestPoint.x, closestPoint.y, transform.position.z), ref vel, smoothness);//smoothdamp function for smoothness
            return;
        }
        Vector3 ballFocus = new Vector3(ball.position.x, ball.position.y, -10);                     //ball position in camera z
        transform.position = Vector3.SmoothDamp(transform.position, ballFocus, ref vel, smoothness);//smoothdamp function for smoothness
    }
    IEnumerator Panning()
    {
        panning = true;
        Vector2 startPos = transform.position;
        Vector2 startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        while (Input.GetMouseButton(1))
        {
            Vector2 mousPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 difference = mousPos - startMousePos;

            transform.position += -new Vector3(difference.x, difference.y);
            yield return null;
        }
        panning = false;
    }
    //Returns array of positions that makes up a polygon for the boundaries
    Vector2[] PolygonPositions(Vector2 start, Vector2 end, float scale)
    {
        if(start.x == end.x || start.y == end.y)
        {
            end += Vector2.one * 0.001f;
        }
        float height = Camera.main.orthographicSize * 2; //camera height in units
        float width = height * Camera.main.aspect;       //camera width in units
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
    private void OnDrawGizmosSelected()
    {
        //scene view visuals
        Gizmos.color = Color.green;
        Vector2[] polygon = PolygonPositions(startPos, endPos, 1);
        for (int i = 0; i < polygon.Length-1; i++)
        {
            Gizmos.DrawLine(polygon[i], polygon[i + 1]);
        }
        Gizmos.DrawLine(polygon[polygon.Length - 1], polygon[0]);

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, Vector2.one * bufferBox*2);
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

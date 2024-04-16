using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Boundaries")]
    [SerializeField] Vector2 startPos, endPos; //start and endpos of path
    [Header("Camera Following")]
    [SerializeField] float feather = 1;        //feather of outermost boundaries
    [SerializeField] float smoothness = 0.3f;  //smoothness of camera movement
    [SerializeField] float resistance = 1.3f;  //resistance when going out of bounds
    PolygonCollider2D innerShape, cameraPath;              //inner boundaries
    Transform ball;                            //ball transform
    Vector3 vel;                               //camera velocity
    //Information event
    public delegate void Bounds(PolygonCollider2D bounds);
    public static Bounds bounds;
    private void Awake()
    {
        ball = GameObject.FindGameObjectWithTag("Player").transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        transform.position = startPos;
        innerShape = new GameObject("Inner Bounds").AddComponent<PolygonCollider2D>();
        innerShape.points = PolygonPositions(startPos, endPos, 1, 1f); //to scale version
        bounds?.Invoke(innerShape);

        cameraPath = new GameObject("Camera Bounds").AddComponent<PolygonCollider2D>();
        cameraPath.points = PolygonPositions(startPos, endPos, 1, 0.01f); //scaled down version of inner shape
    }

    // Update is called once per frame
    void Update()
    {
        if(!cameraPath.bounds.Contains(transform.position)) //out of boundaries
        {
            Vector2 closestPoint = cameraPath.ClosestPoint(transform.position);                     //closest point on boundary perimeter to position
            Vector2 dir = new Vector2(transform.position.x, transform.position.y) - closestPoint;   //direction from point to position
            float dist = dir.magnitude;                                                             //distance to point
            float progress = Mathf.Clamp01(Mathf.Pow(dist / feather, resistance));                  //0 to 1 depending on how close to feathered boundary
            dir = dir.normalized;                                                                   //normalized direction
            float dot = Vector3.Dot(dir, vel.normalized);                                           //dot product of camera velocity and direction
            if (dot > 0)                                                                            //you are going out of bounds
            {
                vel = vel - (progress * vel.magnitude) * vel.normalized * dot;                      //subtracting velocity depending on how far out of bounds
            }
        }
        Vector3 ballFocus = new Vector3(ball.position.x, ball.position.y, -10);                     //ball position in camera z
        transform.position = Vector3.SmoothDamp(transform.position, ballFocus, ref vel, smoothness);//smoothdamp function for smoothness
    }
    //Returns array of positions that makes up a polygon for the boundaries
    Vector2[] PolygonPositions(Vector2 start, Vector2 end, float f, float scale)
    {
        float height = Camera.main.orthographicSize * 2; //camera height in units
        float width = height * Camera.main.aspect;       //camera width in units
        Vector3 camSize = new Vector3(width, height);
        camSize.x += f; //feathering
        camSize.y += f; //feathering
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
        float height = Camera.main.orthographicSize * 2; //camera height in units
        float width = height * Camera.main.aspect;       //camera width in units
        Vector3 camSize = new Vector3(width, height);


        Gizmos.color = Color.green;
        Vector2[] polygon = PolygonPositions(startPos, endPos, 1, 1);
        for (int i = 0; i < polygon.Length-1; i++)
        {
            Gizmos.DrawLine(polygon[i], polygon[i + 1]);
        }
        Gizmos.DrawLine(polygon[polygon.Length - 1], polygon[0]);
        Gizmos.color = Color.red;
        polygon = PolygonPositions(startPos, endPos, feather, 1);
        for (int i = 0; i < polygon.Length - 1; i++)
        {
            Gizmos.DrawLine(polygon[i], polygon[i + 1]);
        }
        Gizmos.DrawLine(polygon[polygon.Length - 1], polygon[0]);
    }
}
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

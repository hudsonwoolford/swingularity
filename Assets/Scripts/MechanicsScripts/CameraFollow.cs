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
    [SerializeField] Vector2 feather;
    [Header("Camera Following")]
    [SerializeField] bool lockCamera;
    [SerializeField] bool zoomOnHoleEnd;
    [SerializeField] bool debugCam;
    [SerializeField] float moveSmoothness = 0.25f, zoomSmoothness = 0.5f;  //smoothness of camera movement
    [SerializeField] float sizeScalar = 1, zoomSensitivity = 0.75f;  //smoothness of camera movement
    //[SerializeField] bool focusBall = true;
    Camera mainCam;
    PolygonCollider2D cameraPath, panningPath;              //inner boundaries
    Transform ball, flag;                            //ball transform
    Vector3 vel;
    float sizeVelocity;                              //camera velocity
    float cameraSize;
    float targetCameraSize;
    public float defaultCameraSize;
    public Vector2 cameraMinMax = new Vector2(3, 10);
    public static PolygonCollider2D innerShape { get; private set; }
    public static bool panning { get; private set; } = false;
    bool finalZoom = false;

    bool camLock = false;

    public void InitPositions(Vector2 startPosSent, Vector2 endPosSent, float maxSizeSent)
    {
        startPos = startPosSent;
        endPos = endPosSent;
        cameraMinMax.y = maxSizeSent;
        cameraSize = defaultCameraSize;
        targetCameraSize = cameraSize;
        innerShape.points = InnerPolygonPositions(startPos, endPos, defaultCameraSize, sizeScalar, feather.x, feather.y); //scaled down version of inner shape


        MasterStart();
    }
    public void SetTargetZoom(float targetZoom) //this should be only called from tutorial and recursive mapping handling
    {
        targetCameraSize = targetZoom;
    }
    public void SetTargetZoom(bool max) //this should be only called from tutorial and recursive mapping handling
    {
        targetCameraSize = max ? cameraMinMax.y : cameraMinMax.x;
    }
    private void Awake()
    {
        mainCam = GetComponent<Camera>();
        ball = GameObject.FindGameObjectWithTag("Player").transform;
        innerShape = new GameObject("Inner Bounds").AddComponent<PolygonCollider2D>();
        if(GameManager.LevelManager != null && GameManager.LevelManager.currentUniverse.universeType == LevelManager.UniverseParams.UniverseType.recursive)
        {
            feather.y = (Mathf.Abs(startPos.x - endPos.x) + CalculateCamsize(defaultCameraSize).x) / mainCam.aspect;
            innerShape.points = InnerPolygonPositions(startPos, endPos, defaultCameraSize, sizeScalar, feather.x, feather.y); //scaled down version of inner shape
        }
        else{
            feather.x /= mainCam.aspect;
        }
        innerShape.points = InnerPolygonPositions(startPos, endPos, defaultCameraSize, sizeScalar, feather.x, feather.y); //scaled down version of inner shape
    }
    // Start is called before the first frame update
    void Start()
    {
        if(GameManager.LevelManager.currentUniverse.universeType == LevelManager.UniverseParams.UniverseType.recursive)
        {
            cameraMinMax.y = Mathf.Max((innerShape.bounds.extents.x) / mainCam.aspect, innerShape.bounds.extents.y) - 0.25f;
        }
        else
        {
            cameraMinMax.y = Mathf.Max((innerShape.bounds.extents.x) / (2 * mainCam.aspect), innerShape.bounds.extents.y / 2) * 1.3f;
        }
        cameraSize = defaultCameraSize;
        targetCameraSize = cameraSize;
        MasterStart();
    }
    void MasterStart()
    {
        mainCam.orthographicSize = defaultCameraSize;
        flag = GameObject.FindGameObjectWithTag("Hole").transform;
        transform.position = new Vector3(ball.position.x, ball.position.y, -10);

        if (cameraPath == null)
        {
            cameraPath = new GameObject("Camera Bounds")
            {
                tag = "CameraBounds"
            }.AddComponent<PolygonCollider2D>();

        }
        if (panningPath == null)
        {
            panningPath = new GameObject("Panning Path").AddComponent<PolygonCollider2D>();
        }

        //parallel universe - swapping start positions
        if(GameManager.LevelManager.currentUniverse.universeType == LevelManager.UniverseParams.UniverseType.parallel)
        {
            Vector3 cachePos = startPos;
            startPos = endPos;
            endPos = cachePos;
        }
        ////recursive universe - shrinking boundaries
        //if (GameManager.LevelManager.currentUniverse.universeType == LevelManager.UniverseParams.UniverseType.recursive)
        //{
        //    sizeScalar = 0.65f;
        //    cameraMinMax.y = Mathf.Max((innerShape.bounds.extents.x) / (2 * mainCam.aspect), innerShape.bounds.extents.y / 2) * 1.5f;
        //}

        cameraPath.points = InnerPolygonPositions(startPos, endPos, cameraMinMax.x, sizeScalar, 0, 0); //scaled down version of inner shape

        Vector2 targetPos = ball.position; //camera focuses ball by default
        float showLevelThreshold = cameraMinMax.x;
        if (targetCameraSize > showLevelThreshold) //start focusing the level instead of the ball
        {
            float camLerpVal = targetCameraSize.Remap(showLevelThreshold, cameraMinMax.y, 0, 1);

            Vector2 ballProgressPos = new Vector2(targetPos.x, startPos.y); //ball position along x-axis of level
            Vector2 flagProgressPos = new Vector2(flag.position.x, startPos.y); //flag position along x-axis of level
            Vector2 midpoint = (startPos + endPos) / 2; //midpoint of the level

            Vector2 pathDir = (endPos - startPos).normalized;
            float moveMag = Mathf.Clamp(CalculateCamsize(cameraMinMax.y).x / 5f, -(midpoint - ballProgressPos).magnitude, (midpoint - ballProgressPos).magnitude);
            if (((midpoint - flagProgressPos).magnitude > (ballProgressPos - flagProgressPos).magnitude))
            {
                moveMag = -moveMag;
            }

            Vector2 movePoint = pathDir * moveMag + ballProgressPos;
            Vector2 alteredTargetpos = Vector2.Lerp(targetPos, movePoint, camLerpVal);
            targetPos = alteredTargetpos;

        }
        transform.position = new Vector3(targetPos.x, targetPos.y, -10);//position
    }

    void Update()
    {
        if (debugCam)
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                camLock = !camLock;
            }
        }
    }
    // Update is called once per frame
    public void LateUpdate()
    {   
        if(Time.timeScale == 0) { return; }
        if(SceneHandling.loadingScreen) { return; }
        if (LevelManager.levelComplete) { return; }
        if(!camLock){
            CamBehaviour();
        }
    }
    void CamBehaviour()
    {
        Vector2 camSize = CalculateCamsize(mainCam.orthographicSize);
        panningPath.points = InnerPolygonPositions(startPos, endPos, 8, sizeScalar, feather.x - camSize.x, feather.y - camSize.y);

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space)) //right click to pan
        {
            StartCoroutine(Panning());
        }
        if (Input.GetMouseButton(2) || Input.GetKeyDown(KeyCode.E)) //middle click to reset
        {
            panning = false;
        }
        if (!Input.GetMouseButton(0)) //not shooting (left click)
        {
            targetCameraSize = Mathf.Clamp(targetCameraSize - Input.mouseScrollDelta.y * zoomSensitivity * Time.timeScale, cameraMinMax.x, cameraMinMax.y); //adjust camera size with scroll
        }

        Vector2 targetPos = ball.position; //camera focuses ball by default
        if (!panning) { LevelPositioningLogic(); }
        ZoomingAndPositioningLogic();

        //END OF FUNCTION
        void LevelPositioningLogic()
        {
            if (!panningPath.OverlapPoint(new Vector2(ball.position.x, ball.position.y)) && !finalZoom) //out of boundaries
            {
                Vector2 closestPoint = panningPath.ClosestPoint(new Vector2(ball.position.x, ball.position.y)); //closest point on boundary perimeter to position
                targetPos = closestPoint; //adjust target position to boundary position
            }

            float showLevelThreshold = cameraMinMax.x;
            if (targetCameraSize > showLevelThreshold) //start focusing the level instead of the ball
            {
                float camLerpVal = targetCameraSize.Remap(showLevelThreshold, cameraMinMax.y, 0, 1);

                Vector2 ballProgressPos = new Vector2(targetPos.x, startPos.y); //ball position along x-axis of level
                Vector2 flagProgressPos = new Vector2(flag.position.x, startPos.y); //flag position along x-axis of level
                Vector2 midpoint = (startPos + endPos) / 2; //midpoint of the level

                Vector2 pathDir = (endPos - startPos).normalized;
                float moveMag = Mathf.Clamp(CalculateCamsize(cameraMinMax.y).x / 5f, -(midpoint - ballProgressPos).magnitude, (midpoint - ballProgressPos).magnitude);
                if (((midpoint - flagProgressPos).magnitude > (ballProgressPos - flagProgressPos).magnitude))
                {
                    moveMag = -moveMag;
                }

                Vector2 movePoint = pathDir * moveMag + ballProgressPos;
                Vector2 alteredTargetpos = Vector2.Lerp(targetPos, movePoint, camLerpVal);
                targetPos = alteredTargetpos;

            }
            transform.position = Vector3.SmoothDamp(transform.position, new Vector3(targetPos.x, targetPos.y, -10), ref vel, moveSmoothness);//smoothdamp function for smoothness
        }
        void ZoomingAndPositioningLogic()
        {
            //cache old positions
            Vector2 mousePos = GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
            Vector2 ballPos = targetPos;

            //adjust camera size to target
            cameraSize = Mathf.Clamp(Mathf.SmoothDamp(cameraSize, targetCameraSize, ref sizeVelocity, zoomSmoothness, float.PositiveInfinity, Mathf.Max(0.000001f, Time.deltaTime)), cameraMinMax.x, cameraMinMax.y);
            mainCam.orthographicSize = cameraSize;

            //dertermine which way the camera should move while zooming
            Vector2 zoomDir = panning ? ZoomPos(mousePos, cameraSize) : ZoomPos(ballPos, cameraSize); //if panning zoom in on mouse, else ball
            Vector2 closestZoomPos = panningPath.ClosestPoint((Vector2)transform.position + zoomDir);

            if (lockCamera) { transform.position = Vector3.SmoothDamp(transform.position, new Vector3(startPos.x, startPos.y, -10), ref vel, moveSmoothness); return; }
            //set canera position with position adjusted for zoom
            transform.position = new Vector3(closestZoomPos.x, closestZoomPos.y, -10);
        }
    }
    Vector3 ZoomPos(Vector2 oldPos, float oldZoom)
    {
        //VIEW FRUSTRUM BUGS OCCURED HERE DUE TO DIVIDE BY ZERO, THIS AREA MAY STILL BE AFFECTED, BUT I THINK IT IS FIXED
        Vector2 length = oldPos - (Vector2)transform.position;
        float zoomScaleFactor = oldZoom / mainCam.orthographicSize;
        Vector2 scaledLength = length / zoomScaleFactor;
        //Debug.LogFormat("Camera Orthagraphic Size: {0} | Old Zoom {1} | Zoom Scale Factor {2}", mainCam.orthographicSize, oldZoom, zoomScaleFactor);
        Vector2 deltaLength = length - scaledLength;
        return deltaLength;
    }
    IEnumerator Panning()
    {
        panning = true;
        Vector2 startMousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        while (Input.GetMouseButton(1) && panning)
        {
            Vector2 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 difference = mousePos - startMousePos;

            Vector2 nextPos = new Vector2(transform.position.x - difference.x, transform.position.y - difference.y);

            float buffer = 0.1f;
            Vector2 bufferedNextPos = Vector2.zero;
            if (nextPos.x < panningPath.bounds.min.x + buffer)
            {
                bufferedNextPos.x = panningPath.bounds.min.x + buffer;
            }
            else if (nextPos.x > panningPath.bounds.max.x - buffer)
            {
                bufferedNextPos.x = panningPath.bounds.max.x - buffer;
            }
            else
            {
                bufferedNextPos.x = nextPos.x;
            }

            if (nextPos.y < panningPath.bounds.min.y + buffer)
            {
                bufferedNextPos.y = panningPath.bounds.min.y + buffer;
            }
            else if (nextPos.y > panningPath.bounds.max.y - buffer)
            {
                bufferedNextPos.y = panningPath.bounds.max.y - buffer;
            }
            else
            {
                bufferedNextPos.y = nextPos.y;
            }

            transform.position = new Vector3(bufferedNextPos.x, bufferedNextPos.y, transform.position.z);
            startMousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            yield return null;
        }
    }
    #region Polygon Stuff
    Vector2[] InnerPolygonPositions(Vector2 start, Vector2 end, float camSizeVar, float scale, float featherX, float featherY)
    {
        if (start.x == end.x || start.y == end.y)
        {
            end += Vector2.one * 0.001f; // Avoid zero-length line issues
        }

        float height = camSizeVar * 2; // Camera height in world units
        float width = height * mainCam.aspect; // Camera width in world units
        height += featherY;
        width += featherX;
        Vector2 camSize = new Vector2(width, height) * scale; // Apply scaling

        Vector2 dir = (end - start).normalized; // Direction from start to end
        Vector2 perp = new Vector2(-dir.y, dir.x); // Perpendicular vector for width expansion

        // Define the rectangle points (expanding outward from the path)
        Vector2[] polygonPoints = new Vector2[]
        {
        start - perp * (camSize.y / 2) - dir * (camSize.x / 2), // Bottom-left
        start + perp * (camSize.y / 2) - dir * (camSize.x / 2), // Top-left
        end   + perp * (camSize.y / 2) + dir * (camSize.x / 2), // Top-right
        end   - perp * (camSize.y / 2) + dir * (camSize.x / 2)  // Bottom-right
        };

        return polygonPoints;
    }


    #endregion
    void OnReset()
    {
        panning = false;
    }
    void ZoomIn()
    {
        if (zoomOnHoleEnd && !camLock)
        {
            lockCamera = false;
            targetCameraSize = Mathf.Clamp(targetCameraSize - 5, cameraMinMax.x, cameraMinMax.y);
            finalZoom = true;
        }

    }
    void FocusBall()
    {
        panning = false;
    }
    //used to update camera to new ball position after level is generated on a parralel universe
    void OnLevelGenerated()
    {
        if(GameManager.LevelManager.currentUniverse.universeType == LevelManager.UniverseParams.UniverseType.parallel)
        {
            transform.position = new Vector3(ball.position.x, ball.position.y, -10);
            vel = Vector2.zero;
        }
    }
    Vector2 CalculateCamsize(float orthoSize)
    {
        float height = orthoSize * 2; //camera height in units
        float width = height * mainCam.aspect; //camera width in units
        return new Vector2(width, height);
    }
    private void OnEnable()
    {
        LevelGenerator.levelGenerated += OnLevelGenerated;
        ShootBall.onShoot += FocusBall;
        ShootBall.triggerReset += OnReset;
        ShootBall.onHole += OnReset;
        ShootBall.holeAnim += ZoomIn;
    }
    private void OnDisable()
    {
        LevelGenerator.levelGenerated -= OnLevelGenerated;
        ShootBall.onShoot -= FocusBall;
        ShootBall.triggerReset -= OnReset;
        ShootBall.onHole -= OnReset;
        ShootBall.holeAnim -= ZoomIn;
    }
    private void OnDrawGizmos()
    {
        if (mainCam == null) { mainCam = Camera.main; }
        //scene view visuals
        //feather.y = (Mathf.Abs(startPos.x - endPos.x) + CalculateCamsize(defaultCameraSize).x) / mainCam.aspect;
        Vector2[] polygon = InnerPolygonPositions(startPos, endPos, cameraMinMax.x, 1 * sizeScalar, 0, 0);
        Vector2[] outerPol = InnerPolygonPositions(startPos, endPos, defaultCameraSize, 1 * sizeScalar, feather.x, feather.y);
        for (int i = 0; i < polygon.Length - 1; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(polygon[i], polygon[i + 1]);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(outerPol[i], outerPol[i + 1]);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawLine(polygon[polygon.Length - 1], polygon[0]);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(outerPol[outerPol.Length - 1], outerPol[0]);


        Gizmos.color = Color.white;
    }
}
public static class Toolkit
{
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
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

        if (EditorGUI.EndChangeCheck())
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

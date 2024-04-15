using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Vector2 startPos, endPos;
    [SerializeField] float feather = 1;
    [SerializeField] float resistance = 1;
    PolygonCollider2D innerShape;
    Transform ball;
    private void Awake()
    {
        ball = GameObject.FindGameObjectWithTag("Player").transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        transform.position = startPos;
        innerShape = GetComponent<PolygonCollider2D>();
        innerShape.points = PolygonPositions(startPos, endPos);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(ball.position.x, ball.position.y, -10);
    }
    Vector2[] PolygonPositions(Vector2 start, Vector2 end)
    {
        float height = Camera.main.orthographicSize * 2; //camera height in units
        float width = height * Camera.main.aspect;       //camera width in units
        Vector3 camSize = new Vector3(width, height);
        Vector2 dir = (end - start).normalized;
        print(dir);

        Vector2[] startCorners = { start + new Vector2(camSize.x / 2, camSize.y / 2), start + new Vector2(camSize.x / 2, -camSize.y / 2), start + new Vector2(-camSize.x / 2, -camSize.y / 2), start + new Vector2(-camSize.x / 2, camSize.y / 2) };
        Vector2[] endCorners   = { end + new Vector2(camSize.x / 2, camSize.y / 2), end + new Vector2(camSize.x / 2, -camSize.y / 2), end + new Vector2(-camSize.x / 2, -camSize.y / 2), end + new Vector2(-camSize.x / 2, camSize.y / 2) };

        Vector2[] finalPoints = new Vector2[6];

        int i = 0;
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
        i = 0;                                   
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
        return finalPoints;
    }
    private void OnDrawGizmosSelected()
    {
        float height = Camera.main.orthographicSize * 2; //camera height in units
        float width = height * Camera.main.aspect;       //camera width in units
        Vector3 camSize = new Vector3(width, height);


        //Gizmos.color = Color.green;
        //Gizmos.DrawWireCube(startPos, camSize);
        //Gizmos.DrawWireCube(endPos, camSize);
        //Gizmos.DrawLine(new Vector3(startPos.x - camSize.x / 2, startPos.y + camSize.y / 2, 0), new Vector3(endPos.x - camSize.x / 2, endPos.y + camSize.y / 2, 0));
        //Gizmos.DrawLine(new Vector3(startPos.x + camSize.x / 2, startPos.y - camSize.y / 2, 0), new Vector3(endPos.x + camSize.x / 2, endPos.y - camSize.y / 2, 0));
        Gizmos.color = Color.red;
        camSize.x += feather;
        camSize.y += feather;
        Gizmos.DrawWireCube(startPos, camSize);
        Gizmos.DrawWireCube(endPos, camSize);
        Gizmos.DrawLine(new Vector3(startPos.x - camSize.x / 2, startPos.y + camSize.y / 2, 0), new Vector3(endPos.x - camSize.x / 2, endPos.y + camSize.y / 2, 0));
        Gizmos.DrawLine(new Vector3(startPos.x + camSize.x / 2, startPos.y - camSize.y / 2, 0), new Vector3(endPos.x + camSize.x / 2, endPos.y - camSize.y / 2, 0));
    }
}

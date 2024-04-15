using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Vector3 startPos, endPos;
    [SerializeField] float feather = 1;
    [SerializeField] float resistance = 1;
    Transform ball;
    private void Awake()
    {
        ball = GameObject.FindGameObjectWithTag("Player").transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        transform.position = startPos;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(ball.position.x, ball.position.y, -10);
    }
    private void OnDrawGizmosSelected()
    {
        float height = Camera.main.orthographicSize * 2; //camera height in units
        float width = height * Camera.main.aspect;       //camera width in units
        Vector3 camSize = new Vector3(width, height);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(startPos, camSize);
        Gizmos.DrawWireCube(endPos, camSize);
        Gizmos.DrawLine(new Vector3(startPos.x - camSize.x / 2, startPos.y + camSize.y / 2, 0), new Vector3(endPos.x - camSize.x / 2, endPos.y + camSize.y / 2, 0));
        Gizmos.DrawLine(new Vector3(startPos.x + camSize.x / 2, startPos.y - camSize.y / 2, 0), new Vector3(endPos.x + camSize.x / 2, endPos.y - camSize.y / 2, 0));
        Gizmos.color = Color.red;
        camSize.x += feather;
        camSize.y += feather;
        Gizmos.DrawWireCube(startPos, camSize);
        Gizmos.DrawWireCube(endPos, camSize);
        Gizmos.DrawLine(new Vector3(startPos.x - camSize.x / 2, startPos.y + camSize.y / 2, 0), new Vector3(endPos.x - camSize.x / 2, endPos.y + camSize.y / 2, 0));
        Gizmos.DrawLine(new Vector3(startPos.x + camSize.x / 2, startPos.y - camSize.y / 2, 0), new Vector3(endPos.x + camSize.x / 2, endPos.y - camSize.y / 2, 0));
    }
}

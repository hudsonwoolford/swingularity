using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBall : MonoBehaviour
{
    [SerializeField] bool stayStillTillShot = false;
    [Header("Power")]
    [SerializeField] float maxPower; //max power the player can input
    [SerializeField] float moveScale; //how much moving your mouse can affect the power
    [Header("Arrow")]
    [SerializeField] LineRenderer lr;
    [SerializeField] float maxLength; //how big can the arrow get
    [SerializeField] LayerMask playerLayer; //the layer that the player and only the player is on
    [SerializeField] int verts; //how detailed is the arrow / how many vertices
    Vector3 mousePos;
    Vector3 origin;
    Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        origin = transform.position;
        if (stayStillTillShot) { rb.constraints = RigidbodyConstraints.FreezePosition; } //will stay still, no gravity
        lr.positionCount = 0; //no line
    }
    // Update is called once per frame
    void Update()
    {
        Camera main = Camera.main;
        mousePos = Input.mousePosition; //screen position of mouse
        mousePos = main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, main.nearClipPlane)); //world position, z value is camera near clip plane
        if(Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = new Ray(main.transform.position, mousePos - main.transform.position); //the ray from the camera to mouse pos
            RaycastHit hitData; //can be acessed later if needed
            if(Physics.Raycast(mouseRay, out hitData, 20, playerLayer)) //if raycast hits the ball, eg player clicks the ball
            {
                StartCoroutine(LineUpShot());
            }
        }
        if (Input.GetKeyDown(KeyCode.R)) { BallReset(); } //reset
    }
    IEnumerator LineUpShot() //starts when the player clicks, ends when the unclick. Works in vector 2s
    {
        Vector2 startingMousePos = mousePos; //stores the initial mousepos
        //lr setup
        lr.positionCount = verts;
        //local variable declaration
        Vector2 dir; //direction to the initial mouse pos, also the direction of the ball
        float dist; //distance from initial mouse pos
        while(Input.GetMouseButton(0)) 
        {
            dir = startingMousePos - new Vector2(mousePos.x, mousePos.y);
            dist = dir.magnitude;
            
            dist = Mathf.Clamp(dist * moveScale, -maxPower, maxPower); //stops the player from doing insanely powerful shots, is also where it is scaled

            //percentage
            //max length
            //verts
            float powerPercent = dist / maxPower; //percentage of max power
            float lengthPercent = powerPercent * maxLength; //percentage of max length
            Vector2 vertStep = lengthPercent / verts * dir.normalized; //the translation of each vert
            for (int i = 0; i < verts; i++)
            {
                lr.SetPosition(i, new Vector2(transform.position.x,transform.position.y) + i * vertStep); //setting positions relative to ball
            }
            yield return null;
        }

        lr.positionCount = 0; //deleting line

        dir = startingMousePos - new Vector2(mousePos.x, mousePos.y);
        dist = dir.magnitude;
        
        dist = Mathf.Clamp(dist * moveScale, -maxPower, maxPower); //stops the player from doing insanely powerful shots, is also where it is scaled

        Vector2 force = dir.normalized * dist; //the force

        //unfreeze position (if neeeded)
        if (stayStillTillShot && rb.constraints == RigidbodyConstraints.FreezePosition) { rb.constraints = RigidbodyConstraints.FreezePositionZ; rb.constraints = RigidbodyConstraints.FreezeRotationX; } 

        rb.AddForce(force, ForceMode.Impulse); //adding force
    }
    void BallReset()
    {
        transform.position = origin; //back to start
        if (stayStillTillShot) { rb.constraints = RigidbodyConstraints.FreezePosition; } //will stay still, no gravity
        rb.angularVelocity = Vector2.zero; //wont keep spinning
        lr.positionCount = 0; //no line
    }
}

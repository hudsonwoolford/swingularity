using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShootBall : MonoBehaviour
{
    [SerializeField] PhysicMaterial ballMat;
    [Header("Power")]
    [SerializeField] float maxPower; //max power the player can input
    [SerializeField] float moveScale; //how much moving your mouse can affect the power
    [Header("Straight Arrow")]
    [SerializeField] bool straight;
    [SerializeField] LineRenderer lr; //line renderer
    [SerializeField] float maxLength; //how big can the arrow get (straight)
    [SerializeField] LayerMask playerLayer; //the layer that the player and only the player is on
    [SerializeField] int verts; //how detailed is the arrow / how many vertices
    [Header("Simulated arrow")]
    [SerializeField] bool simulated;
    [SerializeField] LineRenderer slr; //line renderer, simulated line renderer
    [SerializeField] GameObject simulationBall; //ball prefab with no scripts attached, just for physics
    [SerializeField] int maxstep; //how many time steps taken (simulated);
    [SerializeField] float maxLengthSim; //line can become pretty long, this is the limiter
    [SerializeField] Transform simulatedEnv; //the env that will be in the simulated physics scene
    List<Rigidbody> attractors = new List<Rigidbody>();
    Vector3 lastFramePos;
    Scene simulationScene;
    PhysicsScene physicsScene;
    GameObject simulatedObj = null;
    Rigidbody srb;
    Vector3 mousePos;
    Vector3 origin;
    Rigidbody rb;
    float bounciness;
    bool still = true;
    Camera main;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        main = Camera.main;
        lastFramePos = transform.position;
        attractors.Clear();

        origin = transform.position;
        bounciness = ballMat.bounciness;

        rb.constraints = RigidbodyConstraints.FreezeAll; //stops any movement

        slr.positionCount = 0; //no line
        lr.positionCount = 0; //no line
        if(simulated) {PhysicsScene();}
    }
    // Update is called once per frame
    void Update()
    {
        mousePos = Input.mousePosition; //screen position of mouse
        mousePos = main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, main.nearClipPlane)); //world position, z value is camera near clip plane
        if(Input.GetMouseButtonDown(0))
        {
            Ray mouseRay = new Ray(mousePos, main.transform.forward); //the ray from the camera to mouse pos
            RaycastHit hitData; //can be acessed later if needed
            if(Physics.Raycast(mouseRay, out hitData, 20, playerLayer) && rb.velocity.magnitude <0.1f) //if raycast hits the ball, eg player clicks the ball
            {
                StartCoroutine(LineUpShot());
            }
        }
        if (Input.GetKeyDown(KeyCode.R)) { BallReset(); } //reset1ity
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

            if(simulated)
            {
                SimulateScene(dir.normalized * dist);   
            }

            if(!straight) {yield return null; continue;}

            float powerPercent = dist / maxPower; //percentage of max power
            float lengthPercent = powerPercent * maxLength; //percentage of max length
            Vector2 vertStep = lengthPercent / verts * dir.normalized; //the translation of each vert
            for (int i = 0; i < verts; i++)
            {
                lr.SetPosition(i, new Vector2(transform.position.x,transform.position.y) + i * vertStep); //setting positions relative to ball
            }
            yield return null;
        }
        if(simulatedObj!=null)
        {
            Destroy(simulatedObj);
        }
        slr.positionCount = 0;//deleting line
        lr.positionCount = 0; //deleting line

        dir = startingMousePos - new Vector2(mousePos.x, mousePos.y);
        dist = dir.magnitude;
        
        dist = Mathf.Clamp(dist * moveScale, -maxPower, maxPower); //stops the player from doing insanely powerful shots, is also where it is scaled
        
        Vector2 force = dir.normalized * dist; //the force

        //unfreeze position
        still = false; rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        rb.AddForce(force, ForceMode.Impulse); //adding force
    }
    void StoppingBall()
    {
        if(still) { return; }

        if(Vector3.Distance(lastFramePos, transform.position) < 0.25f)
        {
            still = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        lastFramePos = transform.position;
    }
    void BallReset()
    {
        transform.position = origin; //back to start
        rb.constraints = RigidbodyConstraints.FreezeAll;//will stay still, no gravity
        rb.angularVelocity = Vector2.zero; //wont keep spinning
        lr.positionCount = 0; //no line
        slr.positionCount =0; //no line
    }
    public void Attract(Rigidbody rbToAttract, Rigidbody rbAttracting, bool sim) //brackeys gravity tutorial
    {
        if (still&&!sim) { return; } //no need for extra forces that cause jitter
        Vector3 direction = rbAttracting.position - rbToAttract.position; //direction vector
        float distance = direction.magnitude; //distance

        float forceMagnitude = 0.75f * (rbAttracting.mass * rbToAttract.mass) / Mathf.Pow(distance, 2); //newton equation, uses mass from rigid bodies
        Vector3 force = direction.normalized * forceMagnitude; //storing force

        rbToAttract.AddForce(force); //applying force
    }
    #region Simulation
    void PhysicsScene()
    {
        simulationScene = SceneManager.CreateScene("Simulation");
        physicsScene = simulationScene.GetPhysicsScene(); //gets the physics scene
        foreach (Transform tr in simulatedEnv)
        {
            GameObject obj = Instantiate(tr.gameObject, tr.position, tr.rotation);
            obj.GetComponent<Renderer>().enabled = false;
            Attractor attractor;
            obj.layer = LayerMask.NameToLayer("Simulation");
            if(obj.TryGetComponent<Attractor>(out attractor))
            {
                attractor.enabled = false;
                attractors.Add(obj.GetComponent<Rigidbody>());
            }
            SceneManager.MoveGameObjectToScene(obj, simulationScene);
        }
    }
    void SimulateScene(Vector3 forceVar)
    {
        Physics.simulationMode = SimulationMode.Script; //manual control over time
        if(simulatedObj==null) //instantiation new ball if it doesn't exist
        {
            simulatedObj = Instantiate(simulationBall, transform.position, transform.rotation);
            srb = simulatedObj.GetComponent<Rigidbody>();
        }

        SceneManager.MoveGameObjectToScene(simulatedObj, simulationScene); //moves ball to simulation scene
        
        srb.velocity = rb.velocity;
        srb.AddForce(forceVar, ForceMode.Impulse); //force that would happen if mouse release

        slr.positionCount = 0;
        slr.positionCount = maxstep; //setting up lr
        
        Vector3 originSim = transform.position; //for some reason the ball on our side of the simulation moves when simulated so it must be reset  
        float length = 0; //will keep track of line length to be compared against maxlength
        for (int i = 0; i < maxstep; i++) //for each step
        {
            if(i>0){physicsScene.Simulate(Time.fixedDeltaTime); length += (simulatedObj.transform.position - slr.GetPosition(i-1)).magnitude;} //actual simulation and length addition
            if (length >= maxLengthSim)
            {
                Vector3 dir = simulatedObj.transform.position - slr.GetPosition(i - 1).normalized; //dir from prev point to current point
                float distToFinal = maxLengthSim - (length -= (simulatedObj.transform.position - slr.GetPosition(i - 1)).magnitude); //dist from prev point to maxlength
                Vector3 finalPoint = slr.GetPosition(i - 1) + dir * distToFinal; //final point in the line
                slr.SetPosition(i, finalPoint); 
                slr.positionCount = i;
                break;
            }
            foreach (Rigidbody r in attractors) //since simulation does not account for gravity, we manually do it
            {
                Attract(srb, r, true); //gravity
            }
            slr.SetPosition(i, simulatedObj.transform.position); //lr position of the ball at current step point
        }
        Destroy(simulatedObj); //not performant must be object pooled later
        transform.position = originSim;
        // // srb.velocity = oVelo;
        // // srb.angularVelocity = oAlo;
        // // simulatedObj.transform.position = pos;
        // // simulatedObj.transform.rotation = rot;
        Physics.simulationMode = SimulationMode.FixedUpdate; //going back to normal fixed update
    }

    #endregion
    #region OnTrueFunc
    private void OnTriggerEnter(Collider other) {
         if(other.gameObject.tag == "Hole")
        {
            BallReset();
        }
    }
    private void OnDrawGizmosSelected() {
        Gizmos.DrawCube(mousePos, Vector3.one);
    }
    private void OnApplicationQuit()
    {
        ballMat.bounciness = bounciness;
    }
    #endregion
}

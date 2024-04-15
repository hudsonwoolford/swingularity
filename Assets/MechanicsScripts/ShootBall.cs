using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShootBall : MonoBehaviour
{
    [SerializeField] PhysicMaterial ballMat;
    [SerializeField] Transform planetTransform;
    [Header("Power")]
    [SerializeField] float maxPower; //max power the player can input
    [SerializeField] float moveScale; //how much moving your mouse can affect the power
    [Header("Straight Arrow")]
    [SerializeField] bool straight;
    [SerializeField] LineRenderer lr; //line renderer
    [SerializeField] float maxLength; //how big can the arrow get (straight)
    [SerializeField] LayerMask playerLayer, obstacles; //the layer that the player and only the player is on
    [SerializeField] int verts; //how detailed is the arrow / how many vertices
    [Header("Simulated arrow")]
    [SerializeField] bool simulated;
    [SerializeField] bool colIcons = true, showBallPos = false;
    [SerializeField] LineRenderer slr; //line renderer, simulated line renderer
    [SerializeField] GameObject simulationBall, collisionVisual; //ball prefab with no scripts attached, just for physics
    [Range(0, 500)]
    [SerializeField] int maxstep; //how many time steps taken (simulated);
    [SerializeField] float maxLengthSim; //line can become pretty long, this is the limiter
    [SerializeField] Transform simulatedEnv; //the env that will be in the simulated physics scene
    List<Rigidbody> attractors = new List<Rigidbody>();
    Scene simulationScene;
    PhysicsScene physicsScene;
    GameObject simulatedObj = null;
    Rigidbody srb;
    List<Rigidbody> planets = new List<Rigidbody>();
    Vector3 mousePos;
    Vector3 origin;
    Rigidbody rb;
    float bounciness;
    Camera main;
    TrailRenderer tr;
    bool liningUpShot;
    public delegate void OnShoot();
    public static OnShoot onShoot;
    public delegate void OnStill();
    public static OnStill onStill;
    public delegate void OnHole();
    public static OnHole onHole;
    public delegate void TriggerReset();
    public static TriggerReset triggerReset;

    private void Awake()
    {
        tr = GetComponent<TrailRenderer>();
        rb = GetComponent<Rigidbody>();
        int i = 0;
        foreach (Transform t in planetTransform)
        {
            planets.Add(t.gameObject.GetComponent<Rigidbody>());
            i++;
        }
    }
    private void Start()
    {
        main = Camera.main;
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
            
            if(Vector2.Distance(mousePos, transform.position) < 0.5f * 2&& rb.velocity.magnitude <0.1f) //if raycast hits the ball, eg player clicks the ball
            {
                StartCoroutine(LineUpShot());
                if(showBallPos)
                { StartCoroutine(ShowBallSpeed()); }
            }
        }
        if (Input.GetKeyDown(KeyCode.R)) { BallReset(); } //reset1ity
    }
    void FixedUpdate()
    {
        rb.AddForce(ArtificialGravity.GravityForceAtPoint(rb.position, planets.ToArray(), rb.mass));
    }
    IEnumerator LineUpShot() //starts when the player clicks, ends when the unclick. Works in vector 2s
    {
        liningUpShot = true;
        Vector2 startingMousePos = mousePos; //stores the initial mousepos
        //lr setup
        lr.positionCount = verts;
        //local variable declaration
        Vector2 dir = Vector2.zero; //direction to the initial mouse pos, also the direction of the ball
        float dist = 0; //distance from initial mouse pos
        while(Input.GetMouseButton(0)) 
        {
            dir = startingMousePos - new Vector2(mousePos.x, mousePos.y);
            dist = dir.magnitude;
            
            dist = Mathf.Clamp(dist * moveScale, -maxPower, maxPower); //stops the player from doing insanely powerful shots, is also where it is scaled
            if(Physics.Raycast(transform.position, dir, 0.75f, obstacles)) //this happens if the player is about to shoot directly onto a planet
            {
                dir = Vector2.zero;
                dist = 0;
            }

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
            yield return null; //wait for next frame
        }
        if(simulatedObj!=null)
        {
            Destroy(simulatedObj);
        }
        slr.positionCount = 0;//deleting line
        lr.positionCount = 0; //deleting line
        
        Vector2 force = dir.normalized * dist; //the force

        //unfreeze position
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        onShoot?.Invoke(); //invoke shoot event
        rb.AddForce(force, ForceMode.Impulse); //adding force

        foreach (GameObject c in cols)
        {
            Destroy(c);
        }
        cols.Clear();
        liningUpShot = false;
    }
    void BallReset()
    {
        transform.position = origin; //back to start
        tr?.Clear();
        rb.constraints = RigidbodyConstraints.FreezeAll;//will stay still, no gravity
        rb.angularVelocity = Vector2.zero; //wont keep spinning
        lr.positionCount = 0; //no line
        slr.positionCount =0; //no line
    }
    #region Simulation
    void PhysicsScene()
    {
        simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        physicsScene = simulationScene.GetPhysicsScene(); //gets the physics scene
        foreach (Transform tr in simulatedEnv) //children of environment
        {
            GameObject obj = Instantiate(tr.gameObject, tr.position, tr.rotation); //instantiates a clone
            obj.GetComponent<Renderer>().enabled = false; //hides it
            obj.layer = LayerMask.NameToLayer("Simulation"); //sets layer to simulation
             attractors.Add(obj.GetComponent<Rigidbody>()); //adds attractor to a list to later be used in simulation
            SceneManager.MoveGameObjectToScene(obj, simulationScene); //moves the object to simulation scene
        }
    }
    void SimulateScene(Vector3 forceVar)
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;
        Physics.simulationMode = SimulationMode.Script; //manual control over time
        if(simulatedObj==null) //instantiation new ball if it doesn't exist
        {
            simulatedObj = Instantiate(simulationBall, transform.position, transform.rotation);
            srb = simulatedObj.GetComponent<Rigidbody>();
            srb.GetComponent<SimulationBall>().Init(this);
        }

        SceneManager.MoveGameObjectToScene(simulatedObj, simulationScene); //moves ball to simulation scene
        
        srb.velocity = rb.velocity;
        srb.AddForce(forceVar, ForceMode.Impulse); //force that would happen if mouse release

        slr.positionCount = 0;
        slr.positionCount = maxstep; //setting up lr
        
        float length = 0; //will keep track of line length to be compared against maxlength
        for (int i = 0; i < maxstep; i++) //for each step
        {
            if (i>0){physicsScene.Simulate(Time.fixedDeltaTime); length += (simulatedObj.transform.position - slr.GetPosition(i-1)).magnitude;} //actual simulation and length addition
            if (length >= maxLengthSim)
            {
                Vector3 dir = simulatedObj.transform.position - slr.GetPosition(i - 1).normalized; //dir from prev point to current point
                float distToFinal = maxLengthSim - (length -= (simulatedObj.transform.position - slr.GetPosition(i - 1)).magnitude); //dist from prev point to maxlength
                Vector3 finalPoint = slr.GetPosition(i - 1) + dir * distToFinal; //final point in the line
                slr.SetPosition(i, finalPoint); 
                slr.positionCount = i;
                break;
            }
            srb.AddForce(ArtificialGravity.GravityForceAtPoint(srb.position, planets.ToArray(), rb.mass));
            slr.SetPosition(i, simulatedObj.transform.position); //lr position of the ball at current step point
        }
        Destroy(simulatedObj); //not performant must be object pooled later
        // // srb.velocity = oVelo;
        // // srb.angularVelocity = oAlo;
        // // simulatedObj.transform.position = pos;
        // // simulatedObj.transform.rotation = rot;
        Physics.simulationMode = SimulationMode.FixedUpdate; //going back to normal fixed update
        if (colIcons) { DrawCollisionIcons(); } //After fully simulated for the frame, draws the icons for collisions
    }
    #endregion
    #region CollisionIcons
    List<GameObject> cols = new List<GameObject>();
    List<Vector3> colPos = new List<Vector3>();
    public void BallCollides(Vector3 pos) //called by the simulated ball when it collides with something
    {
        if(Vector3.Distance(transform.position, pos) < 0.5f) return; //this prevents an object that is on top of the ball
        colPos.Add(pos + new Vector3(0,0,-5)); //adds the position of the collision
    }
    void DrawCollisionIcons()
    {
        //if statement used to make collider positions and num of collision objects the same
        if (cols.Count < colPos.Count) //not enough collider objects
        {
            cols.Add(Instantiate(collisionVisual, Vector3.zero, Quaternion.identity));
        }
        else if (cols.Count > colPos.Count) //too many collider objects
        {
            for (int x = cols.Count; x > colPos.Count; x--) //iterates to destroy extra collider objects
            {
                GameObject extraCol = cols[x - 1];
                cols.Remove(extraCol);
                Destroy(extraCol);
            }
        }
        for (int s = 0; s < cols.Count; s++) //iterates through colliders
        {
            cols[s].transform.position = colPos[s]; //sets the positions of the objects
        }
        colPos.Clear(); //clears all collision positions
    }
    IEnumerator ShowBallSpeed()
    {
        float simTime = 0;
        float endTime = 20 / maxstep;
        if(endTime == 0)
        {
            endTime = 1;
        }
        GradientAlphaKey[] alphas = slr.colorGradient.alphaKeys;
        while(liningUpShot)
        {
            GradientColorKey[] color = new GradientColorKey[5];
            color[0] = new GradientColorKey(Color.red, 0f);
            color[1] = new GradientColorKey(Color.red, simTime / endTime-0.1f);
            color[2] = new GradientColorKey(Color.white, simTime / endTime);
            color[3] = new GradientColorKey(Color.red, simTime / endTime+0.1f);
            color[4] = new GradientColorKey(Color.red, 1f);
            Gradient gradient = new Gradient();
            gradient.SetKeys(color, slr.colorGradient.alphaKeys);
            slr.colorGradient = gradient;
            simTime += Time.deltaTime;
            print(simTime);
            if(simTime > endTime)
            {
                color[4] = new GradientColorKey(Color.white, 1f);
                gradient.SetKeys(color, slr.colorGradient.alphaKeys);
                simTime = 0;
            }
            yield return null;
        }
        
        yield return new WaitForFixedUpdate();
    }
    #endregion
    #region OnTrueFunc
    private void OnTriggerEnter(Collider other) {
         if(other.gameObject.tag == "Hole")
        {
            onHole?.Invoke();
            triggerReset?.Invoke();
        }
    }
    private void OnDrawGizmosSelected() {
        Gizmos.DrawCube(mousePos, Vector3.one); //mousepos cube
    }
    private void OnApplicationQuit()
    {
        ballMat.bounciness = bounciness; //makes sure the bounciness is retained, THIS MAY CAUSE ISSUES IN THE FUTURE
    }
    #endregion
    private void OnEnable()
    {
        triggerReset += BallReset;
    }
    private void OnDisable()
    {
        triggerReset -= BallReset;
    }
}

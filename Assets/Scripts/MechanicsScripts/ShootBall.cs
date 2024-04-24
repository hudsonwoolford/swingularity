using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ShootBall : MonoBehaviour
{
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
    [SerializeField] LayerMask simulationLayer;
    [SerializeField] GameObject simulationBall, collisionVisual; //ball prefab with no scripts attached, just for physics
    [Range(0, 2000)]
    [SerializeField] int maxstep; //how many time steps taken (simulated);
    [SerializeField] float maxLengthSim; //line can become pretty long, this is the limiter
    [SerializeField] float animLength; //Length of animation when hit the hole
    [SerializeField] float animSmoothing; //Smoothinh of animation when hit the hole
    [SerializeField] float animScale; //Scale of animation when hit the hole

    Scene simulationScene;
    PhysicsScene physicsScene;
    GameObject simulatedObj = null;
    Rigidbody srb;
    List<CelestialMass> planets = new List<CelestialMass>();
    Vector3 mousePos;
    Vector3 origin;
    Vector3 previousDir;
    Rigidbody rb;
    Camera main;
    TrailRenderer tr;
    bool liningUpShot;
    bool ballStill;
    public delegate void OnShoot();
    public static OnShoot onShoot;
    public delegate void OnStill();
    public static OnStill onStill;
    public delegate void OnHole();
    public static OnHole onHole;
    public delegate void HoleAnim();
    public static HoleAnim holeAnim;
    public delegate void TriggerReset();
    public static TriggerReset triggerReset;
    public delegate void BoundsReset();
    public static BoundsReset boundsReset;

    public void Init(Transform env, Vector3 originPos){
        planetTransform = env;
        transform.position = originPos;
        origin = originPos;
        foreach (Transform t in planetTransform)
        {
            planets.Add(t.gameObject.GetComponent<CelestialMass>());
        }
        if(simulationScene != null)
        {
            StartCoroutine(RecreatePhysicsScene());
        }
        tr.Clear();
    }
    IEnumerator RecreatePhysicsScene()
    {
        if (simulationScene != null)
        {
            AsyncOperation removingScene = SceneManager.UnloadSceneAsync(simulationScene);
            yield return removingScene;
            PhysicsScene();
        }
    }

    private void Awake()
    {
        origin = transform.position;
        tr = GetComponent<TrailRenderer>();
        rb = GetComponent<Rigidbody>();
        planetTransform = GameObject.FindGameObjectWithTag("Environment").transform;
        if (simulated) { PhysicsScene(); }
    }
    private void Start()
    {
        main = Camera.main;

        rb.constraints = RigidbodyConstraints.FreezeAll; //stops any movement

        slr.positionCount = 0; //no line
        lr.positionCount = 0; //no line

    }
    // Update is called once per frame
    void Update()
    {
        if(planets.Count != planetTransform.childCount){
            planets.Clear();
            foreach (Transform t in planetTransform)
            {
                planets.Add(t.gameObject.GetComponent<CelestialMass>());
            }
        }
        mousePos = Input.mousePosition; //screen position of mouse
        mousePos = main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, main.nearClipPlane)); //world position, z value is camera near clip plane
        if (rb.velocity.magnitude <0.1f){
            if(!ballStill){
                ballStill = true;   
                onStill?.Invoke();
            }
            if(Input.GetMouseButtonDown(0))
            {
            
                if(Vector2.Distance(mousePos, transform.position) < 0.5f * 2&& rb.velocity.magnitude <0.1f) //if raycast hits the ball, eg player clicks the ball
                {
                    StartCoroutine(LineUpShot());
                    if(showBallPos)
                    { StartCoroutine(ShowBallSpeed()); }
                }
            }
        }
        else{
            ballStill = false;
        }

        if (Input.GetKeyDown(KeyCode.R)) { BallReset(); } //reset1ity
    }
    void FixedUpdate(){
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
        Vector3 forceLastFrame = Vector3.zero;
        Vector3 force = Vector3.zero;
        while(Input.GetMouseButton(0)) 
        {
            dir = startingMousePos - new Vector2(mousePos.x, mousePos.y);
            dist = dir.magnitude;
            
            dist = Mathf.Clamp(dist * moveScale, -maxPower, maxPower); //stops the player from doing insanely powerful shots, is also where it is scaled
            force = dist * dir.normalized;
            //RaycastHit hit;
            //if(Physics.Raycast(transform.position, dir, out hit, 1f,  obstacles)) //this happens if the player is about to shoot directly onto a planet
            //{
            //    if(!hit.collider.isTrigger)
            //    {
            //        Vector2 normal = (transform.position - hit.transform.position).normalized;
            //        Vector2 mouseNormal = (mousePos - hit.transform.position).normalized;
            //        dir = new Vector2(-normal.y, normal.x + 0.1f);
            //        dir *= -Vector3.Dot(mouseNormal, dir) / Mathf.Abs(Vector3.Dot(mouseNormal, dir));
            //        dist = Mathf.Clamp(dist, 0, 20 * Mathf.Pow(hit.transform.localScale.x, 0.5f) * (1-Vector3.Dot(mouseNormal, normal)));
            //    }
            //}
            RaycastHit hit;
            if (Physics.Raycast(transform.position, ArtificialGravity.GravityForceAtPoint(rb.position, planets.ToArray(), rb.mass), out hit, 1f, obstacles)) //this happens if the player is about to shoot directly onto a planet
            {
                Vector2 normal = (transform.position - hit.transform.position).normalized;
                Vector2 mouseNormal = (mousePos - (transform.position- (new Vector3(normal.x, normal.y) * 0.22f))).normalized;
                if (Vector2.Dot(mouseNormal, normal) > 0&& HitsPlanet(rb.position, force, rb.velocity, hit.collider))
                {
                    mouseNormal = (mousePos - hit.transform.position).normalized;
                    dir = new Vector2(-normal.y, normal.x + 0.1f);
                    dir *= -Vector3.Dot(mouseNormal, dir) / Mathf.Abs(Vector3.Dot(mouseNormal, dir));

                }

            }
            force = dist * dir.normalized;

            if (simulated && Vector3.Distance(force, forceLastFrame)>0.01f)
            {
                PerformSimulation(rb.position, force, rb.velocity);
                //SimulateScene(dir.normalized * dist);   
            }

            forceLastFrame = force;
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
       
        if(force.magnitude < 0.2f)
        {
            yield break;
        }
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
        GetComponent<MeshRenderer>().enabled = true;

        transform.position = origin; //back to start
        tr?.Clear();
        rb.constraints = RigidbodyConstraints.FreezeAll;//will stay still, no gravity

        rb.angularVelocity = Vector2.zero; //wont keep spinning
        lr.positionCount = 0; //no line
        slr.positionCount =0; //no line
    }
    void DeleteSimulation()
    {
        SceneManager.UnloadSceneAsync(simulationScene);
    }
    #region Performant Simulation
    bool HitsPlanet(Vector3 startPos, Vector3 forceVar, Vector3 currentVelocity, Collider currentPlanet)
    {
        Vector3 trackPos = startPos;
        Vector3 prevPos = startPos;
        Vector3 velocity = forceVar / rb.mass + currentVelocity;
        Vector3 gravForce = Vector3.zero;
        float dist = 0;
        if (simulatedObj == null) //instantiate new ball if it doesn't exist
        {
            simulatedObj = Instantiate(simulationBall, transform.position, transform.rotation);
            srb = simulatedObj.GetComponent<Rigidbody>();
            srb.GetComponent<SimulationBall>().Init(this);
        }
        SceneManager.MoveGameObjectToScene(simulatedObj, simulationScene); //moves ball to simulation scene
        Physics.simulationMode = SimulationMode.Script;
        float rad = GetComponent<SphereCollider>().radius * transform.localScale.x;
        for (int i = 0; i < maxstep; i++)
        {
            if (velocity.magnitude < 0.1f)
            {
                break;
            }

            //fixed update
            gravForce = ArtificialGravity.GravityForceAtPoint(trackPos, planets.ToArray(), rb.mass);
            velocity += gravForce * Time.fixedDeltaTime / rb.mass;


            //physics update
            trackPos += velocity * Time.fixedDeltaTime;
            RaycastHit raycastHit;
            if (Physics.Raycast(prevPos, (trackPos - prevPos).normalized, out raycastHit, (trackPos - prevPos).magnitude))
            {
                if(raycastHit.collider == currentPlanet)
                {
                    Physics.simulationMode = SimulationMode.FixedUpdate;

                    return true;
                }
            }

            //end physics update
            if (dist + (trackPos - prevPos).magnitude > maxLengthSim)
            {
                float distDifference = maxLengthSim - dist;
                Vector3 finalPos = prevPos + (trackPos - prevPos).normalized * distDifference;
                break;
            }
            dist += (trackPos - prevPos).magnitude;
            prevPos = trackPos;
        }
        Physics.simulationMode = SimulationMode.FixedUpdate;
        return false;
    }
    void PerformSimulation(Vector3 startPos, Vector3 forceVar, Vector3 currentVelocity)
    {
        bool respawn = false;
        Vector3 trackPos = startPos;
        Vector3 prevPos = startPos;
        Vector3 velocity = forceVar / rb.mass + currentVelocity;
        Vector3 gravForce = Vector3.zero;
        float dist = 0;
        if (simulatedObj == null) //instantiate new ball if it doesn't exist
        {
            simulatedObj = Instantiate(simulationBall, transform.position, transform.rotation);
            srb = simulatedObj.GetComponent<Rigidbody>();
            srb.GetComponent<SimulationBall>().Init(this);
        }
        SceneManager.MoveGameObjectToScene(simulatedObj, simulationScene); //moves ball to simulation scene
        Physics.simulationMode = SimulationMode.Script;

        slr.positionCount = maxstep +1;
        slr.SetPosition(0, startPos);

        float rad = GetComponent<SphereCollider>().radius * transform.localScale.x;
        for (int i = 0; i < maxstep; i++)
        {
            if (velocity.magnitude < 0.1f)
            {
                slr.positionCount = i + 2;
                slr.SetPosition(i + 1, trackPos);
                break;
            }
            
            //fixed update
            gravForce = ArtificialGravity.GravityForceAtPoint(trackPos, planets.ToArray(), rb.mass);
            velocity += gravForce * Time.fixedDeltaTime / rb.mass;


            //physics update
            bool blackHole = false;
            trackPos += velocity * Time.fixedDeltaTime;
            foreach (CelestialMass r in planets)
            {
                if (Vector3.Distance(r.transform.position, trackPos) <= rad + r.transform.localScale.x) //may cause issues with scale not matching collider scale
                {
                    if(r.gameObject.GetComponent<BlackHoleGenerator>() != null)
                    {
                        blackHole = true;
                    }
                    if (r.gameObject.tag == "Respawn")
                    {
                        respawn = true;
                        break;
                    }
                    srb.position = prevPos;
                    srb.velocity = velocity;
                    physicsScene.Simulate(Time.fixedDeltaTime);
                    trackPos = srb.position;
                    velocity = srb.velocity;
                    //print("Simulation at step #" + i + " | Total Simulations: " + simCounter);
                }
            }
            //end physics update
            if(respawn)
            {
                slr.positionCount = i + 2;
                slr.SetPosition(i + 1, trackPos);
                break;
            }
            if (dist + (trackPos - prevPos).magnitude > maxLengthSim)
            {
                float distDifference = maxLengthSim - dist;
                Vector3 finalPos = prevPos + (trackPos-prevPos).normalized * distDifference;
                slr.positionCount = i + 2;
                slr.SetPosition(i+1, finalPos);
                break;
            }
            dist += (trackPos - prevPos).magnitude;

            prevPos = trackPos;
            slr.SetPosition(i + 1, trackPos);
        }
        Physics.simulationMode = SimulationMode.FixedUpdate;
        if(simulated) { DrawCollisionIcons(); }
    }
    
    #endregion
    #region Simulation
    void PhysicsScene()
    {
        simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        physicsScene = simulationScene.GetPhysicsScene(); //gets the physics scene
        foreach (Transform tr in planetTransform) //children of environment
        {
            GameObject obj = Instantiate(tr.gameObject, tr.position, tr.rotation); //instantiates a clone
            obj.GetComponent<Renderer>().enabled = false; //hides it
            obj.layer = LayerMask.NameToLayer("Simulation"); //sets layer to simulation
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
        Vector3 oVelo = srb.velocity;
        Vector3 oAlo  = srb.angularVelocity;
        Vector3 oPos  = simulatedObj.transform.position;
        Quaternion oRot  = simulatedObj.transform.rotation;
        
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
        srb.velocity = oVelo;
        srb.angularVelocity = oAlo;
        simulatedObj.transform.position = oPos;
        simulatedObj.transform.rotation = oRot;
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
            other.enabled = false;
            StartCoroutine(FinishedLevel());
        }

    }
    IEnumerator FinishedLevel(){
        StartCoroutine(StartAnimation());
        yield return new WaitForSeconds(animLength);
        onHole?.Invoke();
        triggerReset?.Invoke();
    }
    IEnumerator StartAnimation(){
        float timeCounter = 0;
        holeAnim?.Invoke();
        Material mat = GetComponent<MeshRenderer>().material;
        Transform hole = GameObject.FindGameObjectWithTag("Hole").GetComponent<Transform>();
        Vector3 velocity = rb.velocity;
        rb.constraints = RigidbodyConstraints.FreezePosition;
        bool clockwise = false;
        float distance = Vector3.Distance(transform.position, hole.position);
        mat.SetVector("_Offset", new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value));
        if(CheckSpiralDir(hole)){
            clockwise = true;
        }
        while (timeCounter < animLength){
            timeCounter += Time.deltaTime;
            float progress = timeCounter/animLength;
            if(clockwise){
                transform.position = Vector3.SmoothDamp(transform.position, hole.position + Quaternion.Euler(new Vector3 (0, 0, (1-progress) * 360)) * Vector3.up * (1-progress) * distance * animScale, ref velocity, animSmoothing);
            }
            else{
                transform.position = Vector3.SmoothDamp(transform.position, hole.position + Quaternion.Euler(new Vector3 (0, 0, (progress-1) * 360)) * Vector3.up * (1-progress) * distance * animScale, ref velocity, animSmoothing);
            }
            
            mat.SetFloat("_Cutoff", 1 - progress);
            yield return new WaitForFixedUpdate();
        }
    } 
    private bool CheckSpiralDir(Transform hole){
        float dotProduct = Vector3.Dot((hole.position - transform.position).normalized, hole.rotation * Vector3.up);
        if(dotProduct > 0){
            return true;
        }
        else{
            return false;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Respawn")
        {
            StartCoroutine(ResetDelay(0.3f));

        }
    }
    IEnumerator ResetDelay(float delay)
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;//will stay still, no gravity
        GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(delay);
        rb.constraints = RigidbodyConstraints.None;//will stay still, no gravity

        BallReset();

    }
    private void OnDrawGizmosSelected() {
        Gizmos.DrawCube(mousePos, Vector3.one); //mousepos cube
    }
    private void OnApplicationQuit()
    {
    }
    #endregion
    private void OnEnable()
    {
        onHole += DeleteSimulation;
        triggerReset += BallReset;
        boundsReset += BallReset;
    }
    private void OnDisable()
    {
        onHole -= DeleteSimulation;
        triggerReset -= BallReset;
        boundsReset -= BallReset;
    }
}

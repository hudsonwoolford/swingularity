using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ShootBall : MonoBehaviour
{
    [SerializeField] Transform planetTransform;
    [SerializeField] public float maxPower { get; private set; } = 30f; //max power the player can input
    [HideInInspector] public float swingDelay = 0.5f; //max power the player can input
    [HideInInspector] public bool pullBack = false; //max power the player can input
    [Header("Power")]
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
    [SerializeField] LayerMask excludeLayer;
    [SerializeField] GameObject simulationBall, collisionVisual; //ball prefab with no scripts attached, just for physics
    [Range(0, 2000)]
    [SerializeField] int maxstep; //how many time steps taken (simulated);
    [SerializeField] float maxLengthSim; //line can become pretty long, this is the limiter
    [SerializeField] float animLength; //Length of animation when hit the hole
    [SerializeField] float animSmoothing; //Smoothinh of animation when hit the hole
    [SerializeField] float astronautAnimLength; //Astronaut fade animationlength
    [SerializeField] float ballSpinSpeed;
    [SerializeField] Material ballHackModeMat;

    [SerializeField] LayerMask ignoreStill;

    [SerializeField] GameObject astronaut;

    [SerializeField] Material[] astronautFadeMats;
    [SerializeField] Shader emmisionShader;

    [Header("Sound")]
    [SerializeField] AudioObj enterPortal;
    [SerializeField] AudioObj hitBall;
    [SerializeField] AudioObj quasarDeath;
    [SerializeField] AudioSource ballAudioSrc;
    [HideInInspector] public float shootPower { get; private set; }
    [HideInInspector] public float shootAngle { get; private set; }

    public bool liningUpShot { get; private set; }
    public bool astronautFading { get; private set; } = false;
    private const float animScale = 0.75f;

    AstronautMatController astronautMatController;
    MeshRenderer ballRenderer;
    Material defaultBallMat;
    Scene simulationScene;
    PhysicsScene physicsScene;
    GameObject simulatedObj = null;
    Rigidbody srb;
    List<CelestialMass> planets = new List<CelestialMass>();
    Vector3 mousePos;
    Vector3 origin, trueOrigin;
    Vector3 posLastFrame;
    Rigidbody rb;
    Camera main;
    TrailRenderer tr;
    bool ballStill = false;
    bool deathDelay;
    float ballStillTimer = 0f;
    float ballStillTimeVal = 0.2f;
    bool inPortal;

    bool swingHit;
    bool collidedWithNoZone = false;

    bool movedAstronaut = false;
    GameObject childObj;

    public delegate void OnShoot();
    public static OnShoot onShoot;
    public delegate void OnSwing();
    public static OnSwing onSwing;
    public delegate void OnStill();
    public static OnStill onStill;
    public delegate void OnHole();
    public static OnHole onHole;
    public delegate void HoleAnim();
    public static HoleAnim holeAnim;
    public delegate void TriggerReset();
    public static TriggerReset triggerReset;
    public delegate void StopGravity();
    public static StopGravity stopGravity;
    public delegate void Death(int index);
    public static Death death;

    public void Init(Transform env, Vector3 originPos){
        planetTransform = env;
        transform.position = originPos;
        origin = originPos;
        trueOrigin = origin;
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
        PhysicsScene();
        yield return null;
    }

    private void Awake()
    {
        origin = transform.position;
        trueOrigin = origin;
        tr = GetComponent<TrailRenderer>();
        rb = GetComponent<Rigidbody>();
        astronautMatController = GameObject.FindGameObjectWithTag("Astronaut").GetComponent<AstronautMatController>();
        ballRenderer = GetComponentInChildren<MeshRenderer>(true);
        planetTransform = GameObject.FindGameObjectWithTag("Environment").transform;
        // if (simulated) { PhysicsScene(); }
    }
    private void Start()
    {
        main = Camera.main;
        defaultBallMat = ballRenderer.material;
        slr.positionCount = 0; //no line
        lr.positionCount = 0; //no line

        childObj = transform.GetChild(2).gameObject;
        onStill?.Invoke();
        ballStill = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        ballStillTimer = 0f;
        if(astronaut == null)
        {
            astronaut = new GameObject("AstronautTemp");
            astronaut.AddComponent<MeshRenderer>().material = defaultBallMat;
        }
        StartCoroutine(BallStillTiming(0.5f));
    }


    // Update is called once per frame
    void Update()
    {
        //Debug.LogFormat("Portal: {0} | Level Complete: {1} | Input: {2} | deathDelay: {3} | ballStill {4} | Astronaut Fade: {5}", inPortal, LevelManager.levelComplete, LevelManager.ballInput, deathDelay, ballStill, astronautFading);
        if(LevelManager.levelComplete || inPortal) { return; }
        if(planets.Count != planetTransform.childCount){
            planets.Clear();
            foreach (Transform t in planetTransform)
            {
                planets.Add(t.gameObject.GetComponent<CelestialMass>());
            }
        }
        mousePos = Input.mousePosition; //screen position of mouse
        mousePos = main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0)); //world position, z value is camera near clip plane

        if (ballStill){
            rb.constraints = RigidbodyConstraints.FreezeAll;
            if(Input.GetMouseButtonDown(0) && LevelManager.ballInput && !astronautFading && astronautMatController.isIdle)
            {
                if(Vector2.Distance(mousePos, transform.position) < 0.5f * 4 && rb.velocity.magnitude <0.1f) //if raycast hits the ball, eg player clicks the ball
                {
                    StartCoroutine(LineUpShot());
                    if(showBallPos)
                    { StartCoroutine(ShowBallSpeed()); }
                }
            }
        }
        if (rb.velocity.magnitude < 0.1f && Physics.OverlapSphere(rb.position, 0.25f, ignoreStill).Length > 1){
            if(!ballStill){
                ballStillTimer = 0f;
                StartCoroutine(BallStillTiming(ballStillTimeVal));
            }
        }
        else{
            if(rb.constraints == RigidbodyConstraints.FreezeAll) //if the ball is frozen while it shouldnt be, implemented because the ball would be frozen at the start after loading a level, but not colliding with anything
            {
                rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            }
            ballStill = false;
            childObj.transform.up =
            Vector3.Slerp(childObj.transform.up, rb.velocity.normalized, Time.deltaTime * 15f * ballSpinSpeed);
            childObj.transform.forward =
            Vector3.Slerp(childObj.transform.forward, rb.velocity.normalized, Time.deltaTime * 3.5f * ballSpinSpeed);
        }

        if (Input.GetKeyDown(KeyCode.R) && !deathDelay && LevelManager.ballInput) { triggerReset?.Invoke(); } //reset
    }
    void FixedUpdate(){
        if(!deathDelay) { rb.AddForce(ArtificialGravity.GravityForceAtPoint(rb.position, planets.ToArray(), rb.mass)); }
        posLastFrame = transform.position;
    }
    IEnumerator BallStillTiming(float ballStillTime){
        while(ballStillTimer <= ballStillTime){

            ballStillTimer += Time.deltaTime;
            if(rb.velocity.magnitude >= 0.1f || Physics.OverlapSphere(rb.position, 0.25f, ignoreStill).Length < 2 || collidedWithNoZone)
            {
                ballStill = false;
                yield break;
            }
            else{
                yield return null;
            }
        }
        //origin = transform.position;
        ballStill = true;
        onStill?.Invoke();

        if (!astronautFading && !deathDelay){
            StartCoroutine(AstronautFade());
        }
        yield break;
    }
    IEnumerator AstronautFade(){
        astronautFading = true;
        if(movedAstronaut) { astronautFading = false; astronautMatController.EndFade(); yield break; }

        if(Vector2.Distance(astronaut.transform.position, rb.position) < 0.05f) {

            bool notFullyFinished = false; //whether the astronaut has completely faded or not
            //checks cutoff value on every material
            foreach (Material mat in astronautFadeMats)
            {
                if (mat.GetFloat("_Cutoff") != 1) //this means it has not fully faded
                {
                    notFullyFinished = true;
                    break;
                }
            }
            if(!notFullyFinished) //this means it has fully faded. therefore, the astronaut is very close to the ball and it is solid so there is no need for this function.
            {
                astronautFading = false;
                yield break;
            }
        }

        float timeCounter = 0;
        foreach(Material mat in astronautFadeMats){
            mat.SetColor("_EdgeColor", GameManager.LevelManager.currentTheme.ballColor * 4.5f/6f);    
        }
            astronautMatController.StartFade();

        while(timeCounter <= astronautAnimLength){
            float progress = timeCounter/astronautAnimLength;
            foreach(Material mat in astronautFadeMats){
                mat.SetFloat("_Cutoff", 1 - progress);    
            }
            timeCounter += Time.unscaledDeltaTime;
            yield return null;
        }
        timeCounter = 0;
        foreach(Material mat in astronautFadeMats){
            mat.SetFloat("_Cutoff", 0);    
        }

        if(!movedAstronaut){
            astronaut.transform.position = new Vector3(rb.position.x, rb.position.y, 10);
            Collider[] currentPlanets = Physics.OverlapSphere(rb.position, 0.25f, ignoreStill);
            Vector3 planetPos = Vector3.zero;
            foreach(Collider collider in currentPlanets){
                    ForceField forcefieldScript;
                    collider.gameObject.TryGetComponent<ForceField>(out forcefieldScript);
                    if(forcefieldScript != null){
                        planetPos = forcefieldScript.gameObject.transform.position;
                    }
                }
            Vector2 dir = planetPos - rb.position;
            astronaut.transform.rotation = Quaternion.Euler(astronaut.transform.rotation.x, astronaut.transform.rotation.y, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            movedAstronaut = true;
        }

        while(timeCounter <= astronautAnimLength){
            float progress = timeCounter/astronautAnimLength;
            foreach(Material mat in astronautFadeMats){
                mat.SetFloat("_Cutoff", progress);    
            }
            timeCounter += Time.unscaledDeltaTime;
            yield return null;
        }
        foreach(Material mat in astronautFadeMats){
            mat.SetFloat("_Cutoff", 1);    
        }
        origin = transform.position; //Ball reset position
        astronautFading = false;
        astronautMatController.EndFade();
        yield break;

    }
    IEnumerator LineUpShot() //starts when the player clicks, ends when the unclick. Works in vector 2s
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;
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
            if(PauseManager.paused) //cant shoot while paused
            {
                if (simulatedObj != null)
                {
                    Destroy(simulatedObj);
                }
                slr.positionCount = 0;//deleting line
                lr.positionCount = 0; //deleting line
                liningUpShot = false; 
                if (colIcons) { DrawCollisionIcons(); }
                yield break; 
            }
            stopGravity?.Invoke();
            dir = startingMousePos - new Vector2(mousePos.x, mousePos.y);
            dist = dir.magnitude;
            
            dist = Mathf.Clamp(dist * moveScale, -maxPower, maxPower); //stops the player from doing insanely powerful shots, is also where it is scaled
            shootPower = Mathf.Pow(dist/maxPower, 2.5f) * 100;
            force = dist * dir.normalized;

            RaycastHit hit;
            bool rayHit = Physics.Raycast(transform.position, ArtificialGravity.GravityForceAtPoint(rb.position, planets.ToArray(), rb.mass).normalized, out hit, 1f, obstacles);
            Vector2 normal = (transform.position - hit.transform.position).normalized;
            Vector2 mouseNormal = (mousePos - (transform.position - new Vector3(normal.x, normal.y) * 0.22f)).normalized;

            shootAngle = -Vector2.SignedAngle(normal, dir);
            if (rayHit) //this happens if the player is about to shoot directly onto a planet
            {
                if(Vector3.Dot(mouseNormal, normal) > 0)
                {
                    float right = -Vector3.Dot(mouseNormal, new Vector3(-normal.y, normal.x)) / Mathf.Abs(Vector3.Dot(mouseNormal, new Vector3(-normal.y, normal.x)));
                    Vector3[] tangentVectors = CalculateTangentVector(hit.transform.GetComponent<SphereCollider>(), rb.position);
                    if(tangentVectors.Length == 0){
                        tangentVectors = new Vector3[2]{new Vector3(-normal.y, normal.x, 0), new Vector3(normal.y, -normal.x, 0)};
                    }
                    Vector3 chosenVector = right > 0 ? tangentVectors[0] : tangentVectors[1];

                    float minDot = Vector3.Dot(-chosenVector, normal);
                    if (Vector3.Dot(mouseNormal.normalized, normal) > minDot)
                    {
                        dir = chosenVector;

                        float dotPower = Mathf.Clamp01(1 - Vector3.Dot(mouseNormal.normalized, normal));
                        dist *= dotPower;

                    }
                }

            }
            force = dist * dir.normalized;

            if (simulated && Vector3.Distance(force, forceLastFrame)>0.01f && simulationScene != null)
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

        if(Mathf.CeilToInt(shootPower) <= 5f)
        {
            if (colIcons) { DrawCollisionIcons(); }
            liningUpShot = false;
            yield break;
        }
        StopCoroutine(BallStillTiming(ballStillTimeVal));
        onSwing?.Invoke(); //invoke shoot event

        yield return new WaitUntil(() => pullBack);
        yield return new WaitForSeconds(swingDelay);
        //unfreeze position
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        ballStill = false;
        movedAstronaut = false;

        ballAudioSrc.pitch = Mathf.Lerp(0.75f, 1.25f, 1-force.magnitude / maxPower) + UnityEngine.Random.Range(-0.15f, 0.15f);
        GameManager.SFXManager.PlaySFX(hitBall, ballAudioSrc, Mathf.Clamp01(force.magnitude / maxPower * 2));
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
        if(inPortal) { return; }
        GetComponent<Collider>().enabled = true;
        childObj.GetComponent<MeshRenderer>().enabled = true;

        Vector3 originalPos = transform.position;
        rb.position = origin;
        transform.position = origin;
        tr?.Clear();
        tr.emitting = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector2.zero; //wont keep spinning
        lr.positionCount = 0; //no line
        slr.positionCount =0; //no line
        ballStill = false;
        if(Vector3.Distance(originalPos, rb.position)>0.05f)
        {
            movedAstronaut = false;
        }
        //Stopping all coroutines + stopping coroutine booleans
        StopAllCoroutines();
        astronautFading = false;
        if(!movedAstronaut) { StartCoroutine(AstronautFade()); }
        astronautMatController.EndFade();

        deathDelay = false;
        collidedWithNoZone = false;
        liningUpShot = false;
        DrawCollisionIcons();
    }
    void DeleteSimulation()
    {
        SceneManager.UnloadSceneAsync(simulationScene);
    }
    #region Performant Simulation
    Vector3[] CalculateTangentVector(SphereCollider sphere, Vector3 point)
    {
        Vector3 circle1Centre = sphere.transform.position;
        Vector3 circle2Centre = (circle1Centre + point) / 2;

        float circle1Radius = sphere.radius * sphere.transform.localScale.x + 0.22f;
        float circle2Radius = (point - circle1Centre).magnitude / 2;

        float distance = Vector2.Distance(circle1Centre, circle2Centre);
        float radiusSum = circle1Radius + circle2Radius;
        float radiusDiff = Mathf.Abs(circle1Radius - circle2Radius);

        // No intersection if the circles are too far apart or contained within each other
        if (distance > radiusSum || distance < radiusDiff)
        {
            return new Vector3[0];
        }

        // Calculate intersection points
        float a = (circle1Radius * circle1Radius - circle2Radius * circle2Radius + distance * distance) / (2 * distance);
        float h = Mathf.Sqrt(circle1Radius * circle1Radius - a * a);
        Vector3 intersectionPoint1 = circle1Centre + a * (circle2Centre - circle1Centre) / distance + h * new Vector3(-(circle2Centre.y - circle1Centre.y), circle2Centre.x - circle1Centre.x) / distance;
        Vector3 intersectionPoint2 = circle1Centre + a * (circle2Centre - circle1Centre) / distance - h * new Vector3(-(circle2Centre.y - circle1Centre.y), circle2Centre.x - circle1Centre.x) / distance;

        //Debug.DrawLine(intersectionPoint1, intersectionPoint2);
        //Debug.DrawLine(intersectionPoint1, point);
        //Debug.DrawLine(intersectionPoint2, point);

        return new Vector3[] { (intersectionPoint1 - point).normalized, (intersectionPoint2 - point).normalized };
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
        slr.SetPosition(0, new Vector3(startPos.x, startPos.y, -8));

        float rad = GetComponent<SphereCollider>().radius * transform.localScale.x;
        for (int i = 0; i < maxstep; i++)
        {
            if (velocity.magnitude < 0.1f)
            {
                slr.positionCount = i + 2;
                slr.SetPosition(i + 1, new Vector3(trackPos.x, trackPos.y, -8));
                break;
            }
            
            //fixed update
            gravForce = ArtificialGravity.GravityForceAtPoint(trackPos, planets.ToArray(), rb.mass, true);
            velocity += gravForce * Time.fixedDeltaTime / rb.mass;


            //physics update
            //bool blackHole = false;
            trackPos += velocity * Time.fixedDeltaTime;
            foreach (CelestialMass r in planets)
            {
                if((excludeLayer & (1 << r.gameObject.layer)) != 0) { continue; }
                if (Vector3.Distance(r.transform.position, trackPos) <= rad + r.transform.localScale.x) //may cause issues with scale not matching collider scale
                {
                    if(r.gameObject.GetComponent<BlackHoleGenerator>() != null)
                    {
                        //blackHole = true;
                    }
                    if (r.gameObject.CompareTag("Respawn"))
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
                slr.SetPosition(i + 1, new Vector3(trackPos.x, trackPos.y, -8));
                break;
            }
            if (dist + (trackPos - prevPos).magnitude > maxLengthSim)
            {
                float distDifference = maxLengthSim - dist;
                Vector3 finalPos = prevPos + (trackPos-prevPos).normalized * distDifference;
                slr.positionCount = i + 2;
                slr.SetPosition(i+1,new Vector3(finalPos.x, finalPos.y, -8));
                break;
            }
            dist += (trackPos - prevPos).magnitude;

            prevPos = trackPos;
            slr.SetPosition(i + 1, new Vector3(trackPos.x, trackPos.y, -8));
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
            if(obj.GetComponent<Renderer>() != null)
            {
                obj.GetComponent<Renderer>().enabled = false; //hides it
            }
            //if(obj.layer != 12)
            //{
            RecursiveLayer(obj.transform, LayerMask.NameToLayer("Simulation"));
            //}
            SceneManager.MoveGameObjectToScene(obj, simulationScene); //moves the object to simulation scene
        }
    }
    void RecursiveLayer(Transform searchChildren, int layer)
    {
        searchChildren.gameObject.layer = layer;
        foreach (Transform t in searchChildren)
        {
            RecursiveLayer(t, layer);
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
            Vector3 closestPoint = new Vector3(1000, 1000);
            int index = 0;
            for(int j = 0; j < slr.positionCount; j++){
                if((slr.GetPosition(j) - colPos[s]).sqrMagnitude < (closestPoint - colPos[s]).sqrMagnitude){
                    closestPoint = slr.GetPosition(j);
                    index = j;
                }
            }
            cols[s].transform.GetChild(1).GetComponent<SpriteRenderer>().color = slr.colorGradient.Evaluate((float)index/(float)slr.positionCount);

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
         if(other.gameObject.CompareTag("Hole"))
        {
            GameManager.SFXManager.PlaySFX(enterPortal);
            other.enabled = false;

            StartCoroutine(FinishedLevel(other.transform, transform.position));
        }
        if (other.gameObject.CompareTag("Respawn"))
        {
            StartCoroutine(ResetDelay(0.3f, other.gameObject));

        }
        print("o");
        if(other.gameObject.CompareTag("NoShotZone")){
            collidedWithNoZone = true;
        }
    }
    private void OnTriggerExit(Collider other) {
        if(other.gameObject.CompareTag("NoShotZone")){
            collidedWithNoZone = false;
        }
    }
    IEnumerator FinishedLevel(Transform hole, Vector3 hitPoint)
    {
        inPortal = true;
        GetComponent<Collider>().enabled = false;
        StopCoroutine(nameof(BallStillTiming));
        Vector3 velocity = rb.velocity;
        bool clockwise = DetermineOrbitType(velocity, hitPoint - hole.position);
        StartCoroutine(StartAnimation(hole, hitPoint, clockwise));
        yield return new WaitForSeconds(animLength);
        onHole?.Invoke();
        inPortal = false;
        triggerReset?.Invoke();
    }
    IEnumerator StartAnimation(Transform hole, Vector3 hitPoint, bool clockwise){
        holeAnim?.Invoke();
        rb.constraints = RigidbodyConstraints.FreezePosition;

        GetComponent<TrailRenderer>().enabled = false;

        float timeCounter = 0;
        Material mat = defaultBallMat;
        Vector3 velocity = rb.velocity;

        mat.SetVector("_Offset", new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value));
        if(GameManager.LevelManager.currentTheme != null)
        {
            mat.SetColor("_EdgeColor", GameManager.LevelManager.currentTheme.ballColor);
        }
        while (timeCounter < animLength){
            timeCounter += Time.deltaTime;
            float progress = timeCounter/animLength;
            progress = Mathf.Clamp01(progress);
            //Failsafe here
            if((hole.position - transform.position).magnitude < 1.5f){
                transform.position = hole.GetComponent<Collider>().ClosestPoint(transform.position);
            }
            Vector3 holeDir = hitPoint - hole.position;
            if(clockwise){
                transform.position = Vector3.SmoothDamp(transform.position, hole.position + Quaternion.Euler(new Vector3 (0, 0, (1-progress) * 360)) * holeDir * (1-progress) * animScale, ref velocity, animSmoothing);
            }
            else{
                transform.position = Vector3.SmoothDamp(transform.position, hole.position + Quaternion.Euler(new Vector3 (0, 0, (progress-1) * 360)) * holeDir * (1-progress) * animScale, ref velocity, animSmoothing);
            }
            
            mat.SetFloat("_Cutoff", 1 - progress);
            yield return new WaitForFixedUpdate();
        }
    } 
    private bool DetermineOrbitType(Vector3 velocity, Vector3 hitPosition)
    {
        float product = hitPosition.x * velocity.y - hitPosition.y * velocity.x;
        if (product > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    private bool CheckSpiralDir(Vector3 velocity, Vector3 hitPoint, Transform hole){
        Vector3 holeDir = hitPoint - hole.position;
        velocity *= -1;
        float angle = ((Mathf.Atan2(velocity.y, velocity.x)) * Mathf.Rad2Deg);
        if(angle < 0){
            angle += 360;
        }
        else if(angle > 360){
            angle = angle % 360;
        }
            if(((angle > 45)  && (angle < 90)) || ((angle < 315) && (angle > 270)) || ((angle < 135) && (angle < 90)) || ((angle > 225) && (angle < 270))){
                 return true;
            }
            else{
                return false;
            }
    }

    private bool CheckSpiralDirOLD(Vector3 velocity, Vector3 hitPoint, Transform hole){
        Vector3 holeDir = hitPoint - hole.position;
        velocity *= -1;
        float angle = ((Mathf.Atan2(velocity.y, velocity.x)) * Mathf.Rad2Deg);
        print(angle);

        if(holeDir.x > 0 && holeDir.y > 0){
            if((angle > 45)){
                 return true;
            }
            else{
                return false;
            }
        }
        else if(holeDir.x > 0 && holeDir.y < 0){
            if((angle > -45)){
                return true;
            }
            else{
                return false;
            }
        }
        else if(holeDir.x < 0 && holeDir.y > 0){
            if((angle > 135)){
                return true;
            }
            else{
                return false;
            }
        }
        else if(holeDir.x < 0 && holeDir.y < 0){
            if((angle > -135)){
                return true;
            }
            else {
                return false;
            }
        }
        else {
            return true;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Respawn"))
        {
            StartCoroutine(ResetDelay(0.3f, collision.gameObject));

        }
    }
    private void OnCollisionExit(Collision collision){
    }
    IEnumerator ResetDelay(float delay, GameObject objCol)
    {
        GetComponent<Collider>().enabled = false;
        StopCoroutine(nameof(BallStillTiming));
        deathDelay = true;
        if(objCol.GetComponent<QuasarGenerator>()!=null)
        {
            GameManager.SFXManager.PlaySFX(quasarDeath);
            death?.Invoke(2);
            if (GameManager.LevelManager.currentTheme != null)
            {
                defaultBallMat.SetColor("_EdgeColor", GameManager.LevelManager.currentTheme.ballColor);
            }
            tr.emitting = false;
            Vector3 vel = rb.velocity.normalized;
            rb.constraints = RigidbodyConstraints.FreezeAll;//will stay still, no gravity

            float time = 0;
            while(time < delay)
            {
                transform.position += vel * Time.deltaTime * 3;
                defaultBallMat.SetFloat("_Cutoff", 1-(time/delay));
                time += Time.deltaTime;
                yield return null;
            }
            childObj.GetComponent<MeshRenderer>().enabled = false;
            defaultBallMat.SetFloat("_Cutoff", 1);
            rb.constraints = RigidbodyConstraints.None;//will stay still, no gravity

            triggerReset?.Invoke();
            deathDelay = false;
            yield break;
        }
        else if(objCol.GetComponent<SunGenerator>() != null){
            death?.Invoke(1);
        }
        else{
            death?.Invoke(0);
        }
        rb.constraints = RigidbodyConstraints.FreezeAll;//will stay still, no gravity
        childObj.GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(delay);
        rb.constraints = RigidbodyConstraints.None;//will move
        deathDelay = false;
        triggerReset?.Invoke();

    }
    private void OnDrawGizmosSelected() {
        Gizmos.DrawCube(mousePos, Vector3.one); //mousepos cube
    }
    private void OnApplicationQuit()
    {
    }
    #endregion
    void OnHackMode(bool enable, Material whiteMat)
    {
        ballRenderer.material = enable ? ballHackModeMat : defaultBallMat; 
    }
    private void OnEnable()
    {
        onHole += DeleteSimulation;
        triggerReset += BallReset;
        HackMode.onHackMode += OnHackMode;
    }
    private void OnDisable()
    {
        onHole -= DeleteSimulation;
        triggerReset -= BallReset;
        HackMode.onHackMode -= OnHackMode;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutomaticBallShooter : MonoBehaviour
{
    [Range(0,360f)]
    [SerializeField] float bearing;
    [Range(0f, 1f)]
    [SerializeField] float power;
    [SerializeField] KeyCode shootKey;
    [SerializeField] AnimationCurve pullbackCurve;
    [SerializeField] float waitTime;
    [SerializeField] float pullbackTime;
    [SerializeField] bool dynamicPullbackTime;
    [Header("Simulation")]
    [SerializeField] int simSteps;
    [SerializeField] LayerMask excludeLayer;
    ShootBall shootBall;
    Rigidbody rb;
    CelestialMass[] planetMasses;
    Transform environment;
    Vector2[] simPoints;
    bool started;
    float timePressed;
    // Start is called before the first frame update
    void Start()
    {
        shootBall = GetComponent<ShootBall>();
        rb = GetComponent<Rigidbody>();
        environment = GameObject.FindGameObjectWithTag("Environment").transform;
        planetMasses = environment.GetComponentsInChildren<CelestialMass>();
        simPoints = new Vector2[1]
        {
            transform.position
        };
        started = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftAlt)) { Vector2 shot = OptimalShot();
            if(shot != Vector2.zero)
            {
                bearing = shot.x;
                power = shot.y;
            }

        }
        if (Input.GetKeyDown(shootKey))
        {
            timePressed = 0;
        }
        if(Input.GetKey(shootKey))
        {
            timePressed += Time.deltaTime;
        }
        if(Input.GetKeyUp(shootKey))
        {
            if(!dynamicPullbackTime) { timePressed = pullbackTime; }
            StartCoroutine(shootBall.ArtificialShot(bearing, power, timePressed, waitTime, pullbackCurve));
        }
    }
    Vector2 OptimalShot()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = mousePos - (Vector2)transform.position;
        int startingAngle = (int)(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        for (int ang = startingAngle; ang < startingAngle + 360; ang++)
        {
            for (float pow = 0.1f; pow < 1; pow+=0.1f)
            {
                bearing = ang;
                power = pow;

                MockSimulate();

                for (int i = 1; i < simPoints.Length; i+=3)
                {
                    RaycastHit rayHit;
                    if (Physics.Raycast(simPoints[i - 1], (simPoints[i] - simPoints[i - 1]).normalized, out rayHit, (simPoints[i] - simPoints[i - 1]).magnitude))
                    {
                        if(rayHit.transform.CompareTag("Hole"))
                        {
                            return new Vector2(ang, pow);
                        }
                        break;
                    }
                }
            }
        }
        return Vector2.zero;
    }

    private void OnDrawGizmos()
    {
        if(!Application.isPlaying || !started) { return; }
        
        if (rb.velocity.magnitude < 0.1f)
        {
            MockSimulate();
        }
        float ang = bearing * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));

        Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * power);

        if(simPoints == null) { return; }
        Gizmos.color = Color.yellow;
        for (int i = 1; i < simPoints.Length && simPoints.Length > 1; i++)
        {
            Gizmos.DrawLine(simPoints[i - 1], simPoints[i]);
        }
    }
    void MockSimulate()
    {
        float ang = bearing * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
        float dist = power * shootBall.maxPower; //stops the player from doing insanely powerful shots, is also where it is scaled
        Vector3 force = dist * dir.normalized;

        UpdatePoints(rb.position, force, rb.velocity);
    }
    void UpdatePoints(Vector3 startPos, Vector3 forceVar, Vector3 currentVelocity)
    {
        if (planetMasses.Length != environment.childCount)
        {
            planetMasses = environment.GetComponentsInChildren<CelestialMass>();
        }

        Vector3 trackPos = startPos;
        Vector3 velocity = forceVar / rb.mass + currentVelocity;
        Vector3 gravForce = Vector3.zero;

        List<Vector2> simulationPoints = new List<Vector2>(simSteps + 1)
        {
            startPos
        };

        float rad = GetComponent<SphereCollider>().radius * transform.localScale.x;
        for (int i = 0; i < simSteps; i++)
        {
            if (velocity.magnitude < 0.1f)
            {
                break;
            }

            //fixed update
            gravForce = ArtificialGravity.GravityForceAtPoint(trackPos, planetMasses, rb.mass, true);
            velocity += gravForce * Time.fixedDeltaTime / rb.mass;

            //physics update
            //bool blackHole = false;
            trackPos += velocity * Time.fixedDeltaTime;
            foreach (CelestialMass cm in planetMasses)
            {
                if ((excludeLayer & (1 << cm.gameObject.layer)) != 0) { continue; }
                if (Vector3.Distance(cm.transform.position, trackPos) <= rad + cm.transform.localScale.x) //may cause issues with scale not matching collider scale
                {
                    break;
                }
            }
            //end physics update

            simulationPoints.Add(trackPos);
            //Debug.Log(gravForce);
        }

        simPoints = simulationPoints.ToArray();
    }
    private void OnEnable()
    {
    }
    private void OnDisable()
    {
    }
}

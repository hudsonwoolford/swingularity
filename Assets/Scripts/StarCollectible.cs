using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class StarCollectible : MonoBehaviour
{
    [SerializeField] float randomRadius;
    [SerializeField] float travelRadius;
    [SerializeField] float breakRadius;
    [SerializeField] float travelStep;
    [SerializeField] int maxDirections;
    [SerializeField] float fadeDuration;
    [SerializeField] AnimationCurve fadeCurve;
    [SerializeField] Vector3 minVel, maxVel, minAngVel, maxAngVel;
    SphereCollider sCol;
    Rigidbody rb;
    Vector3 chosenPos;
    Coroutine collectionAnimRoutine;
    SpriteRenderer spr;
    CelestialMass cMass;
    public delegate void StarCollected();
    public static StarCollected onStarCollected;
    public void Init(Vector2 midPoint, int seed)
    {
        GetAllComponents();
        spr.enabled = false;
        sCol.enabled = false;
        Physics.SyncTransforms();
        StartCoroutine(RandomPointAlgorithm(midPoint, seed));
    }
    IEnumerator RandomPointAlgorithm(Vector2 midPoint, int seed)
    {
        UnityEngine.Random.InitState(seed);

        Vector2 randPos = UnityEngine.Random.insideUnitCircle * randomRadius + midPoint;
        Vector2 randDir = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
        float distanceTraveled = 0;
        int iterations = 0;
        int directions = 0;
        transform.position = randPos;
        while (Physics.CheckSphere((Vector3)randPos, breakRadius))
        {
            randPos += randDir * travelStep;
            distanceTraveled += travelStep;
            if (distanceTraveled > travelRadius)
            {
                if (directions >= maxDirections)
                {
                    Debug.LogFormat("No suitable position found for collectible. No collectible will be generated and collectible will automatically be collected.");
                    onStarCollected?.Invoke();
                    Destroy(gameObject);
                    break;
                }
                UnityEngine.Random.InitState(seed + iterations);
                randPos = UnityEngine.Random.insideUnitCircle * randomRadius + midPoint;
                randDir = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
                distanceTraveled = 0;
                directions++;
            }
            iterations++;
            transform.position = randPos;
            yield return new WaitForFixedUpdate();
        }
        
        transform.position = randPos;
        chosenPos = randPos;
        sCol.enabled = true;
        Debug.Log(iterations);
    }
    IEnumerator CollectionAnimation()
    {
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        GetAllComponents();
        spr.enabled = true;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        rb.AddForce(Vector3.Lerp(minVel, maxVel, UnityEngine.Random.value), ForceMode.VelocityChange);
        rb.AddTorque(Vector3.Lerp(minAngVel, maxAngVel, UnityEngine.Random.value), ForceMode.VelocityChange);
        float timer = 0;
        while (timer < fadeDuration)
        {
            spr.color = new Color(spr.color.r, spr.color.g, spr.color.b, Mathf.Lerp(1, 0, fadeCurve.Evaluate(timer/fadeDuration)));
            timer += Time.deltaTime;
            yield return null;
        }
        spr.color = Color.clear;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    private void OnTriggerEnter(Collider collision)
    {
        if(collision.tag=="Player")
        {
            onStarCollected?.Invoke();
        }
    }
    void CollectibleReset()
    {
        if(LevelManager.levelComplete) { return; }
        if(collectionAnimRoutine != null) { StopCoroutine(collectionAnimRoutine); }
        spr.color = Color.white;
        transform.position = chosenPos;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        transform.rotation = Quaternion.identity;
    }
    void GetAllComponents()
    {
        sCol = GetComponent<SphereCollider>();
        rb = GetComponent<Rigidbody>();
        spr = GetComponent<SpriteRenderer>();
        cMass = GetComponent<CelestialMass>();
    }
    void CollectStar()
    {
        collectionAnimRoutine = StartCoroutine(CollectionAnimation());
    }
    private void OnEnable()
    {
        onStarCollected += CollectStar;
    }
    private void OnDisable()
    {
        onStarCollected -= CollectStar;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, randomRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, travelRadius + randomRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, breakRadius);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * travelStep);
    }
}

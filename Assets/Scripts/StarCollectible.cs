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
    [SerializeField] float waitDuration;
    [SerializeField] AnimationCurve fadeCurve;
    [SerializeField] Vector3 minVel, maxVel, minAngVel, maxAngVel;
    [SerializeField] MeshRenderer[] glowRenderers;
    [SerializeField] MeshRenderer[] metalRenderers;
    [SerializeField] Material emissionMat;
    [SerializeField] float glowMult;
    [SerializeField] AudioObj collectionAudio;
    MaterialPropertyBlock glowPBlock;
    MaterialPropertyBlock metalPBlock;
    SphereCollider sCol;
    Rigidbody rb;
    Vector3 chosenPos;
    Coroutine collectionAnimRoutine;
    GameObject starObj;
    CelestialMass cMass;
    public delegate void StarCollected();
    public static StarCollected onStarCollected;
    private void Awake()
    {
        NullChecks();
        starObj.SetActive(false);
        sCol.enabled = true;
        chosenPos = transform.position;
    }
    public void Init(Vector2 midPoint, int seed)
    {
        NullChecks();
        CreatePropertyBlocks();
        starObj.SetActive(false);
        sCol.enabled = false;
        Physics.SyncTransforms();
        RandomPointAlgorithm(midPoint, seed);
    }
    /// <summary>
    /// Chooses a random point in a circle, travels in a random direction in a max radius until it finds an open spot
    /// </summary>
    /// <param name="midPoint"></param>
    /// <param name="seed"></param>
    /// <returns></returns>
    void RandomPointAlgorithm(Vector2 midPoint, int seed)
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
        }
        
        transform.position = randPos;
        chosenPos = randPos;
        sCol.enabled = true;
    }
    IEnumerator CollectionAnimation()
    {
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        NullChecks();
        starObj.SetActive(true);
        //rigid body movement
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
        rb.AddForce(Vector3.Lerp(minVel, maxVel, UnityEngine.Random.value), ForceMode.VelocityChange);
        rb.AddTorque(Vector3.Lerp(minAngVel, maxAngVel, UnityEngine.Random.value), ForceMode.VelocityChange);
        //wait until fade
        yield return new WaitForSeconds(waitDuration);
        float timer = 0;
        //loop fades material
        while (timer < fadeDuration)
        {
            SetMatCutoff(1-fadeCurve.Evaluate(timer / fadeDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        //disabling
        starObj.SetActive(false);
        cMass.enabled = false;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = Vector3.one * 1000;
    }
    void CollectibleReset()
    {
        if(LevelManager.levelComplete) { return; }
        if(collectionAnimRoutine != null) { StopCoroutine(collectionAnimRoutine); }
        starObj.SetActive(false);
        transform.position = chosenPos;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        transform.rotation = Quaternion.identity;
    }
    void NullChecks()
    {
        sCol = GetComponent<SphereCollider>();
        rb = GetComponent<Rigidbody>();
        starObj = transform.GetChild(0).gameObject;
        cMass = GetComponent<CelestialMass>();
    }
    #region Materials
    void CreatePropertyBlocks()
    {
        metalPBlock = new MaterialPropertyBlock(); metalPBlock.SetColor("_EdgeColor", emissionMat.GetColor("_EmissionColor"));
        for (int i = 0; i < metalRenderers.Length; i++)
        {
            metalRenderers[i].SetPropertyBlock(metalPBlock);
        }
        glowPBlock = new MaterialPropertyBlock();
        glowPBlock.SetColor("_EdgeColor", emissionMat.GetColor("_EmissionColor"));
        glowPBlock.SetColor("_Color", emissionMat.GetColor("_EmissionColor") * glowMult);
        for (int i = 0; i < glowRenderers.Length; i++)
        {
            glowRenderers[i].SetPropertyBlock(glowPBlock);
        }
    }
    void SetMatCutoff(float cutoff)
    {
        if(glowPBlock == null || metalPBlock == null) { CreatePropertyBlocks(); }
        glowPBlock.SetFloat("_Cutoff", cutoff);
        metalPBlock.SetFloat("_Cutoff", cutoff);
        for (int i = 0; i < metalRenderers.Length; i++)
        {
            metalRenderers[i].SetPropertyBlock(metalPBlock);
        }
        for (int i = 0; i < glowRenderers.Length; i++)
        {
            glowRenderers[i].SetPropertyBlock(glowPBlock);
        }
    }
    #endregion
    void CollectStar()
    {
        Debug.Log("Collected");
        GameManager.SFXManager.PlaySFX(collectionAudio);
        collectionAnimRoutine = StartCoroutine(CollectionAnimation());
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Player")
        {
            onStarCollected?.Invoke();
        }
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

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class ForceField : MonoBehaviour
{
    static float intensityMult = 1.5f;
    [Header("Script Variables")]
    [SerializeField] Transform planet;
    [SerializeField] List<Transform> interactableObject = new List<Transform>(3);
    [Range(-1f, 1f)]
    [SerializeField] float rangeOffset = 0;
    [SerializeField] Material forceMat;
    [SerializeField] AudioSource src;
    [SerializeField] AudioClip bump;
    [SerializeField] bool autoMass = false;

    [Header("Property Variables (Set by script)")]
    [ColorUsage(true, true)]
    [SerializeField] Color frontColor;
    [ColorUsage(true, true)]
    [SerializeField] Color fresnelColor;


    [HideInInspector]
    [SerializeField] Vector4[] hitPos = new Vector4[3];
    [HideInInspector]
    [SerializeField] Vector4[] hitTime = new Vector4[3];
    [HideInInspector]
    [SerializeField] int hitIterator;

    private MaterialPropertyBlock pBlock;
    private MeshRenderer meshRenderer;
    private Light pOuterAtmosphere;
    private PlanetDataHolder pData;
    private Rigidbody rb;
    private void Awake()
    {
        if(interactableObject.Count == 0)
        {
            interactableObject.Add(GameObject.FindGameObjectWithTag("Player").transform);
        }
        meshRenderer = GetComponent<MeshRenderer>();
        forceMat = meshRenderer.sharedMaterial;
        pBlock = new MaterialPropertyBlock();
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        hitIterator = 0;
        hitPos = new Vector4[10];
        hitTime = new Vector4[10];
        forceMat.SetVectorArray("_HitPos", hitPos);
        forceMat.SetVectorArray("_HitInfo", hitTime);
    }
    void Update()
    {
        if (autoMass) { rb.mass = Mathf.Pow(transform.localScale.x, 2f) * 100; }
        SetPBlockProp();
        PlanetCorrespond();
    }
    void PlanetCorrespond()
    {
        if (planet == null) {Debug.LogWarning("Set planet object in inspector"); return; }
        if (pOuterAtmosphere == null) { pOuterAtmosphere = planet.GetComponentInChildren<Light>(true); if (pOuterAtmosphere == null) { return; } }
        if (pData == null) { pData = planet.gameObject.GetComponent<PlanetDataHolder>(); }
        transform.position = planet.position;
        transform.localScale = Vector3.one * (pOuterAtmosphere.range+rangeOffset) * 1.075f / 2;
        if (pData == null) { return; }
        Color c = pData.forceFieldColor;
        frontColor = new Color(c.r, c.g, c.b, 0);
        fresnelColor = c * intensityMult;
    }
    void SetPBlockProp()
    {
        //null checks
        if (meshRenderer == null) { meshRenderer = GetComponent<MeshRenderer>(); }
        if (forceMat == null) { forceMat = meshRenderer.sharedMaterial; }
        if (pBlock == null) { pBlock = new MaterialPropertyBlock(); }

        //p Block
        pBlock.SetColor("_FrontColor", frontColor);
        pBlock.SetColor("_FresnelColor", fresnelColor);
        interactableObject.RemoveAll(Transform => Transform == null);
        if (interactableObject.Count > 0)
        {
            Vector4[] ioVectors = Array.ConvertAll<Transform, Vector4>(interactableObject.ToArray(), t => t.position);
            pBlock.SetVectorArray("_ObjPos", ioVectors);
        }
        pBlock.SetFloat("_Scale", transform.localScale.x);
        pBlock.SetVector("_TransformWorldPos", transform.position);

        meshRenderer.SetPropertyBlock(pBlock);
    }
    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.layer == 7) { return; } // simulation layer

        hitPos[hitIterator] = col.transform.position; //colision pos
        hitTime[hitIterator].x = Time.time; //time of hit
        
        //calculate hit power
        float hitPower = 0;
        foreach (ContactPoint contact in col.contacts){
            hitPower += contact.impulse.magnitude;
        }
        hitTime[hitIterator].y = hitPower; //hit power array var

        src.PlayOneShot(bump, Mathf.Clamp01(hitPower / 30)); //noise

        //property setting
        pBlock.SetVectorArray("_HitPos", hitPos);
        pBlock.SetVectorArray("_HitInfo", hitTime);

        //hit iterator cycle
        hitIterator++;
        if (hitIterator >= 3){
            hitIterator = 0;
        }
    }
}
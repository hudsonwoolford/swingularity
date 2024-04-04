using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ForceField : MonoBehaviour
{
    [Header("Script Variables")]
    [SerializeField] Transform planet;
    [SerializeField] Transform interactableObject;
    [Range(-1f, 1f)]
    [SerializeField] float rangeOffset = 0;
    [SerializeField] Material forceMat;
    [SerializeField] AudioSource src;
    [SerializeField] AudioClip bump;

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
    private void Awake()
    {
        if (interactableObject == null)
        {
            interactableObject = GameObject.FindGameObjectWithTag("Player").transform;
        }
        meshRenderer = GetComponent<MeshRenderer>();
        forceMat = meshRenderer.sharedMaterial;
        pBlock = new MaterialPropertyBlock();
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
        SetPBlockProp();
        PlanetCorrespond();
    }
    void PlanetCorrespond()
    {
        if (planet == null) {Debug.LogWarning("Set planet object in inspector"); return; }
        if (pOuterAtmosphere == null) { pOuterAtmosphere = planet.GetComponentInChildren<Light>(); }
        transform.position = planet.position;
        transform.localScale = Vector3.one * (pOuterAtmosphere.range+rangeOffset) / 2;
        Color fc = pOuterAtmosphere.color * 1.5f;
        frontColor = new Color(fc.r, fc.g, fc.b, 0) ;
        fresnelColor = pOuterAtmosphere.color * 1.75f;
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
        //Obj info
        pBlock.SetFloat("_Scale", transform.localScale.x);
        pBlock.SetVector("_TransformWorldPos", transform.position);
        forceMat.SetVector("_ObjPos", interactableObject.position);
        meshRenderer.SetPropertyBlock(pBlock);
    }
    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.layer == 7) { return; } // simulation layer

        hitPos[hitIterator] = interactableObject.position; //colision pos
        hitTime[hitIterator].x = Time.time; //time of hit
        
        //calculate hit power
        float hitPower = 0;
        foreach (ContactPoint contact in col.contacts){
            hitPower += contact.impulse.magnitude;
        }
        hitTime[hitIterator].y = hitPower; //hit power array var

        src.PlayOneShot(bump, Mathf.Clamp01(hitPower / 20)); //noise

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
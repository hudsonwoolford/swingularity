using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ForceField : MonoBehaviour
{
    [Header("Property Variables")]
    [ColorUsage(true, true)]
    [SerializeField] Color frontColor;
    [ColorUsage(true, true)]
    [SerializeField] Color fresnelColor;
    [Header("Script Variables")]
    [SerializeField] Material forceMat;
    [SerializeField] Transform interactableObject;
    [SerializeField] AudioSource src;
    [SerializeField] AudioClip bump;
    [HideInInspector]
    [SerializeField] Vector4[] hitPos = new Vector4[3];
    [HideInInspector]
    [SerializeField] Vector4[] hitTime = new Vector4[3];
    [HideInInspector]
    [SerializeField] int hitIterator;

    private MaterialPropertyBlock pBlock;
    private MeshRenderer meshRenderer;
    private void Awake()
    {
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
        if (meshRenderer == null) { meshRenderer = GetComponent<MeshRenderer>(); }
        if (forceMat == null){ forceMat = meshRenderer.sharedMaterial;}
        if(pBlock == null) { pBlock = new MaterialPropertyBlock(); }

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
        if (col.gameObject.layer == 7)
        {
            return;
        }

        hitPos[hitIterator] = interactableObject.position;
        hitTime[hitIterator].x = Time.time;
        float hitPower = 0;
        foreach (ContactPoint contact in col.contacts){
            hitPower += contact.impulse.magnitude;
        }
        src.PlayOneShot(bump, hitPower / 20);
        hitTime[hitIterator].y = hitPower;

        pBlock.SetVectorArray("_HitPos", hitPos);
        pBlock.SetVectorArray("_HitInfo", hitTime);

        hitIterator++;
        if (hitIterator >= 3){
            hitIterator = 0;
        }
    }
}
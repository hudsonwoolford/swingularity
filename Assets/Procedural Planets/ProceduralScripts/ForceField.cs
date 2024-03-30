using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour
{
    [SerializeField] Material forceMat;
    [SerializeField] Transform interactableObject;
    [SerializeField] AudioSource src;
    [SerializeField] AudioClip bump;
    [SerializeField] Vector4[] hitPos = new Vector4[10];
    [SerializeField] Vector4[] hitTime = new Vector4[10]; 
    [SerializeField] int hitIterator;

    private void Awake()
    {
        forceMat = GetComponent<MeshRenderer>().sharedMaterial;
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
        if (forceMat != null)
        {
            forceMat = GetComponent<MeshRenderer>().sharedMaterial;
        }

        forceMat.SetVector("_ObjPos", interactableObject.position);
        forceMat.SetFloat("_Scale", transform.localScale.x);
        forceMat.SetVector("_TransformWorldPos", transform.position);
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.layer == 7)
        {
            return;
        }
        src.PlayOneShot(bump);

        hitPos[hitIterator] = interactableObject.position;
        hitTime[hitIterator].x = Time.time;
        float hitPower = 0;
        foreach (ContactPoint contact in col.contacts){
            hitPower += contact.impulse.magnitude;
        }
        hitTime[hitIterator].y = hitPower;

        forceMat.SetVectorArray("_HitPos", hitPos);
        forceMat.SetVectorArray("_HitInfo", hitTime);

        hitIterator++;
        if (hitIterator >= 10){
            hitIterator = 0;
        }
    }
}
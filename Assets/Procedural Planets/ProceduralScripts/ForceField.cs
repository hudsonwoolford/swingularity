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
        float hitPower = Mathf.Clamp(Vector3.Dot(col.contacts[0].normal,col.relativeVelocity)/10-1, 0, Mathf.Infinity)*3;
        print(hitPower);
        hitTime[hitIterator].y = hitPower;

        forceMat.SetVectorArray("_HitPos", hitPos);
        forceMat.SetVectorArray("_HitInfo", hitTime);

        hitIterator++;
        if (hitIterator >= 10){
            hitIterator = 0;
        }
    }
}
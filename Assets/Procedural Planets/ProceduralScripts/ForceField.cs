using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceField : MonoBehaviour
{
    [SerializeField] Material forceMat;
    [SerializeField] Transform interactableObject;
    [SerializeField] AudioSource src;
    [SerializeField] AudioClip bump;
    [SerializeField] float timeOffset = 0, timeSinceHit;
    [SerializeField] Vector4[] hitPos = new Vector4[10];
    [SerializeField] float[] hitTime = new float[10]; 
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

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        forceMat.SetVector("_ObjPos", interactableObject.position);
        forceMat.SetFloat("_Scale", transform.localScale.x);
        forceMat.SetVector("_TransformWorldPos", transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 7)
        {
            return;
        }
        src.PlayOneShot(bump);

        hitPos[hitIterator] = interactableObject.position;
        hitTime[hitIterator] = Time.time;
        forceMat.SetVectorArray("_HitPos", hitPos);
        forceMat.SetFloatArray("_HitTime", hitTime);

        hitIterator++;
        if (hitIterator >= 10){
            hitIterator = 0;
        }
    }

    private void FixedUpdate()
    {
        if (forceMat != null)
        {
            timeSinceHit = Time.time - timeOffset;

            
        }
    }
}
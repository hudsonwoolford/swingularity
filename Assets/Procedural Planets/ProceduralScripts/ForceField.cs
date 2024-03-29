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
        forceMat.SetVector("_HitPos", interactableObject.position);
        timeOffset = Time.time;
        forceMat.SetFloat("_HitTime", Time.time);
    }

    private void FixedUpdate()
    {
        if (forceMat != null)
        {
            timeSinceHit = Time.time - timeOffset;

            
        }
    }
}
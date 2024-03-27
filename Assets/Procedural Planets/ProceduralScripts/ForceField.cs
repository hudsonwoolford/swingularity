using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ForceField : MonoBehaviour
{
    [SerializeField]Material forceMat;
    [SerializeField] Transform interactableObject;
    [SerializeField] AudioSource src;
    [SerializeField] AudioClip bump;
    private void Awake()
    {
        forceMat = GetComponent<MeshRenderer>().sharedMaterial;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(forceMat != null) { forceMat = GetComponent<MeshRenderer>().sharedMaterial;  }
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        forceMat.SetVector("_ObjPos", interactableObject.position);
        forceMat.SetFloat("_Scale", transform.localScale.x);
        forceMat.SetVector("_TransformWorldPos", transform.position);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.layer == 7)
        {
            return;
        }
        src.PlayOneShot(bump);
        forceMat.SetVector("_HitPos", interactableObject.position);
    }
}

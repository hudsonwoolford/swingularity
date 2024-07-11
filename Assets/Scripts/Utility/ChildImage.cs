using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ChildImage : MonoBehaviour
{
    Image parentImage; 
    Image myImage; 
    // Start is called before the first frame update
    void Start()
    {
        parentImage = transform.parent.GetComponent<Image>();
        myImage = GetComponent<Image>();
        if(parentImage == null || myImage == null) {Destroy(gameObject);}
        myImage.enabled = parentImage.enabled;
        myImage.color = parentImage.color;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        myImage.color = parentImage.color;
        myImage.enabled = parentImage.enabled;
    }
}

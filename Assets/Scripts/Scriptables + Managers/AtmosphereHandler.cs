using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class AtmosphereHandler : MonoBehaviour
{
    float scaleRatio;
    Light atmosphere;
    public void Awake()
    {
        atmosphere = GetComponent<Light>();
        scaleRatio = transform.parent.localScale.x / atmosphere.range;
        atmosphere.range = transform.parent.localScale.x / scaleRatio;
    }
    // Update is called once per frame
    void Update()
    {
        ScaleAtmosphere();
    }
    public void ScaleAtmosphere()
    {
        if (atmosphere == null) { atmosphere = GetComponent<Light>(); }
        atmosphere.range = transform.parent.lossyScale.x / scaleRatio;
    }
}

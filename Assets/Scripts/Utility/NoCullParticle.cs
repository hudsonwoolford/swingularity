using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoCullParticle : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var param = new ParticleSystem.EmitParams();
        param.position = new Vector3(10000, 10000, 10000);
        param.velocity = Vector3.zero;
        param.startLifetime = 1000000000;


        GetComponent<ParticleSystem>().Emit(param, 1);
        param.position = -new Vector3(10000, 10000, 10000);

        GetComponent<ParticleSystem>().Emit(param, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

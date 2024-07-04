using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    [SerializeField] Material skybox;
    [SerializeField] float speed;
    float rotation;

    void Update()
    {
        rotation += Time.deltaTime * speed;
        if(rotation > 360){
            rotation = 0;
        }
        skybox.SetFloat("_Rotation", rotation);
    }
}

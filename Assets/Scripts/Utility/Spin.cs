using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    public Vector3 rotateDir;

    void Update()
    {
            transform.localEulerAngles += rotateDir * Time.deltaTime;
    }
}

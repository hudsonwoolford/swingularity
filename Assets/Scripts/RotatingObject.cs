using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    [SerializeField] private Vector3 rotateVel;
    void Update()
    {
        transform.Rotate(rotateVel * Time.deltaTime);
    }
}

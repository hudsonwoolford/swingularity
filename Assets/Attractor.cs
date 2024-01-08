using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Attractor : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    const float G = 0.874f;
    GameObject ball;
    private void Awake()
    {
        ball = GameObject.FindGameObjectWithTag("Player"); //stores ball, can be optimised later
    }
    private void FixedUpdate()
    {
        Attract(ball); //always attracting ball
    }
    void Attract(GameObject objToAttract) //brackeys gravity tutorial
    {
        Rigidbody rbToAttract = objToAttract.GetComponent<Rigidbody>(); //stores rigidbody of ball

        Vector3 direction = rb.position - rbToAttract.position; //direction vector
        float distance  = direction.magnitude; //distance

        float forceMagnitude = G * (rb.mass * rbToAttract.mass) / Mathf.Pow(distance, 2); //newton equation, uses mass from rigid bodies
        Vector3 force = direction.normalized * forceMagnitude; //storing force

        rbToAttract.AddForce(force); //applying force
    }
}

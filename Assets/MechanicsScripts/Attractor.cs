using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Attractor : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    bool simulation;
    const float G = 1;
    ShootBall ball;
    Rigidbody ballrb;
    private void Awake()
    {
        GameObject ballObj = GameObject.FindGameObjectWithTag("Player");
        ball = ballObj.GetComponent<ShootBall>(); //stores ball, can be optimised later
        ballrb = ballObj.GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        if(!simulation) { ball.Attract(ballrb, rb, false); } //always attracting ball
    }
    //void Attract(GameObject objToAttract) //brackeys gravity tutorial
    //{
    //    Rigidbody rbToAttract = objToAttract.GetComponent<Rigidbody>(); //stores rigidbody of ball

    //    Vector3 direction = rb.position - rbToAttract.position; //direction vector
    //    float distance  = direction.magnitude; //distance

    //    float forceMagnitude = G * (rb.mass * rbToAttract.mass) / Mathf.Pow(distance, 2); //newton equation, uses mass from rigid bodies
    //    Vector3 force = direction.normalized * forceMagnitude; //storing force

    //    rbToAttract.AddForce(force); //applying force
    //}
}

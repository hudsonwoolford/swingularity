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
        if(!simulation) { ballrb.AddForce(ArtificialGravity.AttractForce(ballrb, rb)); } //always attracting ball
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationBall : MonoBehaviour
{
    ShootBall sB;
    Rigidbody rb;
    public void Init(ShootBall shootBall)
    {
        rb = GetComponent<Rigidbody>();
        sB = shootBall;
    }
    private void OnCollisionEnter(Collision collision)
    {
        sB.BallCollides(collision.GetContact(0).point);
    }
    private void OnTriggerEnter(Collider other)
    {
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationBall : MonoBehaviour
{
    ShootBall sB;
    public void Init(ShootBall shootBall)
    {
        sB = shootBall;
    }
    private void OnCollisionEnter(Collision collision)
    {
        sB.BallCollides(collision.GetContact(0).point);
    }
}

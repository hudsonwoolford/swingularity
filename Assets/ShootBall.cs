using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootBall : MonoBehaviour
{
    [SerializeField] float maxPower; //max power the player can input
    [SerializeField] float moveScale; //how much moving your mouse can affect the power
    Vector3 mousePos;
    Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0, 0, 4)); //for some reason i need to add the vector3 to it, i have no idea why
        print(mousePos);
        if(Input.GetMouseButtonDown(0))
        {
            StartCoroutine(LineUpShot());
        }
    }
    IEnumerator LineUpShot() //starts when the player clicks, ends when the unclick. Works in vector 2s
    {
        Vector2 startingMousePos = mousePos; //stores the initial mousepos
        while(Input.GetMouseButton(0))
        {
            //this will be used later for drawing a line
            yield return null;
        }

        Vector2 dir = startingMousePos - new Vector2(mousePos.x, mousePos.y); //gives the direction to the initial mouse pos, also the direction of the ball
        float dist = dir.magnitude; //distance from initial mouse pos
        print(dist);
        dist = Mathf.Clamp(dist * moveScale, -maxPower, maxPower); //stops the player from doing insanely powerful shots, is also where it is scaled

        Vector2 force = dir.normalized * dist; //the force
        
        rb.AddForce(force, ForceMode.Impulse); //adding force
    }
}

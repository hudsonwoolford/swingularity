using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MappingHandling : MonoBehaviour
{
    [SerializeField] BoxCollider camArea, playArea;
    [SerializeField] Transform ball;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!camArea.bounds.Contains(ball.position))
        {
            //out of view
        }
    }
}

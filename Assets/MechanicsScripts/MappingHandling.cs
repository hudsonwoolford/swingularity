using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MappingHandling : MonoBehaviour
{
    [SerializeField] BoxCollider playArea, camArea;
    [SerializeField] Transform ball;
    Camera mainCam;
    Vector3 point;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if(!camArea.bounds.Contains(ball.position))
        {
            //out of view
            print("oov");
            Vector3 dirtoCamera = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, camArea.transform.position.z) - transform.position;
            Ray backToCameraRay = new Ray(transform.position, transform.forward);
            RaycastHit hitData;
            if (Physics.Raycast(backToCameraRay, out hitData, 1000))
            {
                if (hitData.collider == camArea)
                {
                    print(hitData.point);
                    point = hitData.point;
                }
            }
        }
        if(!playArea.bounds.Contains(ball.position))
        {
            //out of bounds
            print("oob");

        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(point, Vector3.one);
    }
}

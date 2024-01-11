using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MappingHandling : MonoBehaviour
{
    [SerializeField] BoxCollider playArea, camArea, camAreaSmaller;
    [SerializeField] TextMeshProUGUI distText;
    [SerializeField] Transform ball;
    [SerializeField] Transform arrow;
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
        arrow.position = Vector3.one * 100;
        if(!camArea.bounds.Contains(ball.position))
        {
            //out of view
            print("oov");
            Vector3 dirtoCamera = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, camArea.transform.position.z) - ball.position;
            Ray backToCameraRay = new Ray(ball.position, dirtoCamera);
            Debug.DrawRay(ball.position, dirtoCamera, Color.red, 0f);
            RaycastHit[] hits = Physics.RaycastAll(backToCameraRay, 1000);
            if (hits!=null)
            {
                print("Hit");
                foreach (RaycastHit r in hits)
                {
                    if (r.collider == camAreaSmaller)
                    {
                        point = r.point;
                        float zRot = Mathf.Atan2(dirtoCamera.normalized.y, dirtoCamera.normalized.x) * Mathf.Rad2Deg;
                        arrow.rotation = Quaternion.Euler(0, 0, zRot - 180);
                        arrow.position = point + new Vector3(0,0,-5);
                        distText.text = Mathf.RoundToInt(Vector3.Distance(point, ball.position) * 10).ToString();
                    }
                    
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

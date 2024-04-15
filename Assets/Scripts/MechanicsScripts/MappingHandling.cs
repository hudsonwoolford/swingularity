using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class MappingHandling : MonoBehaviour
{
    [SerializeField] BoxCollider camArea;
    [Range(0, 2)]
    [SerializeField] float smallerBounds = 0.9f;
    [SerializeField] TextMeshProUGUI distText;
    [SerializeField] Transform ball;
    [SerializeField] Transform arrow;
    [SerializeField] Animator canvasAnimator;
    BoxCollider camAreaSmaller;
    Camera mainCam;
    Vector3 point;
    private void Awake()
    {
        if(ball == null) { ball = GameObject.FindGameObjectWithTag("Player").transform; }
    }
    // Start is called before the first frame update
    void Start()
    {
        Transform cas = new GameObject("Cam Area Smaller").transform;
        cas.SetParent(transform);
        cas.localScale = camArea.transform.localScale - Vector3.one * smallerBounds;
        camAreaSmaller = cas.AddComponent<BoxCollider>();
        camAreaSmaller.isTrigger = true;
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        arrow.position = Vector3.one * 100;
        camArea.transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, 0);
        camAreaSmaller.transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, 0);
        if (!camArea.bounds.Contains(ball.position))
        {
            //out of view
            Vector3 dirtoCamera = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, camArea.transform.position.z) - ball.position;
            Ray backToCameraRay = new Ray(ball.position, dirtoCamera);
            Debug.DrawRay(ball.position, dirtoCamera, Color.red, 0f);
            RaycastHit[] hits = Physics.RaycastAll(backToCameraRay, 1000);
            if (hits!=null)
            {
                int dist = 0;
                foreach (RaycastHit r in hits)
                {
                    
                    if (r.collider == camArea)
                    {
                        point = r.point;
                        dist = Mathf.RoundToInt(Vector3.Distance(point, ball.position) * 10);
                        if(dist > 100)
                        {
                            print(dist);
                           
                            ShootBall.boundsReset?.Invoke();
                        }

                    }
                    if (r.collider == camAreaSmaller)
                    {
                        Vector3 arrowPos = r.point;
                        float zRot = Mathf.Atan2(dirtoCamera.normalized.y, dirtoCamera.normalized.x) * Mathf.Rad2Deg;
                        arrow.rotation = Quaternion.Euler(0, 0, zRot - 180);
                        Transform dt = distText.gameObject.transform;
                        if ((-arrow.transform.up).y > 0)
                        {

                            dt.localRotation = Quaternion.Euler(180, 180, 0);
                            distText.alignment = TextAlignmentOptions.Left;
                        }
                        else
                        {
                            dt.localRotation = Quaternion.Euler(0, 0, 0);
                            distText.alignment = TextAlignmentOptions.Right;
                        }
                        arrow.position = arrowPos + new Vector3(0,0,-5);
                    }
                                
                }
                distText.text = dist.ToString();
            }
        }
    } 
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(point, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, camArea.transform.localScale - Vector3.one * smallerBounds);
        Gizmos.DrawWireCube(Vector3.zero, camArea.transform.localScale - Vector3.one * 100);
    }
}

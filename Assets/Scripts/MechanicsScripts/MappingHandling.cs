using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class MappingHandling : MonoBehaviour
{
    [SerializeField] BoxCollider2D camArea;
    [Range(0, 2)]
    [SerializeField] float smallerBounds = 0.9f;
    [Min(0)]
    [SerializeField] TextMeshProUGUI distText;
    [SerializeField] Transform ball;
    [SerializeField] Transform arrow;
    [SerializeField] Animator canvasAnimator;
    BoxCollider2D camAreaSmaller;
    PolygonCollider2D boundaries;
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
        cas.localScale = Vector3.one;
        camAreaSmaller = cas.AddComponent<BoxCollider2D>();
        camAreaSmaller.isTrigger = true;
        mainCam = Camera.main;
        boundaries = CameraFollow.innerShape;
    }

    // Update is called once per frame
    void Update()
    {
        float height = mainCam.orthographicSize * 2; //camera height in units
        float width = height * mainCam.aspect;       //camera width in units
        Vector3 camSize = new Vector3(width, height, 0);
        camArea.transform.localScale = camSize;
        camAreaSmaller.transform.localScale = camArea.transform.localScale - new Vector3(1,1,0) * smallerBounds;


        arrow.position = -Vector3.one * 100;
        camArea.transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, 0);
        camAreaSmaller.transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, 0);

        if(!boundaries.OverlapPoint(ball.position))
        {
            ShootBall.boundsReset?.Invoke();
        }
        if (CameraFollow.panning) { return; }
        if (!camArea.bounds.Contains(ball.position))
        {
            //out of view
            Vector3 dirtoCamera = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, camArea.transform.position.z) - ball.position;
            Ray2D backToCameraRay = new Ray2D(ball.position, dirtoCamera);
            Debug.DrawRay(ball.position, dirtoCamera, Color.red, 0f);
            RaycastHit2D[] hits = Physics2D.RaycastAll(ball.position, dirtoCamera, 1000);
            if (hits!=null)
            {
                float dist = 0;
                foreach (RaycastHit2D r in hits)
                {
                    if (r.collider == camArea)
                    {
                        point = r.point;
                        dist = Mathf.RoundToInt(Vector3.Distance(point, ball.position) * 10);
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
                        arrow.position = Camera.main.WorldToScreenPoint(arrowPos + new Vector3(0,0,-5));
                    }
                                
                }
                distText.text = ((int)dist).ToString();
            }
            
        }
    } 
    void ShowOOB()
    {
        canvasAnimator.Play("ShowOOB");
    }
    private void OnEnable()
    {
        ShootBall.boundsReset += ShowOOB;
    }
    private void OnDisable()
    {
        ShootBall.boundsReset -= ShowOOB;
    }
    private void OnDrawGizmosSelected()
    {
        float height = Camera.main.orthographicSize * 2; //camera height in units
        float width = height * Camera.main.aspect;       //camera width in units
        Vector3 camSize = new Vector3(width, height);
        Gizmos.DrawCube(point, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, camSize - Vector3.one * smallerBounds);
    }
}

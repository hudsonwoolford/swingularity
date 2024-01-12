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
    [SerializeField] Animator canvasAnimator;
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
                int dist = 0;
                foreach (RaycastHit r in hits)
                {
                    
                    if (r.collider == camArea)
                    {
                        dist = Mathf.RoundToInt(Vector3.Distance(point, ball.position) * 10);
                    }
                    if (r.collider == camAreaSmaller)
                    {
                        point = r.point;
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
                        arrow.position = point + new Vector3(0,0,-5);
                    }
                                
                }
                distText.text = dist.ToString();
            }
        }
        if(!playArea.bounds.Contains(ball.position))
        {
            //out of bounds
            ShootBall.triggerReset?.Invoke();
            canvasAnimator.Play("ShowOOB");
        }
    } 
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(point, Vector3.one);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class MappingHandling : MonoBehaviour
{
    [SerializeField] BoxCollider2D camArea;
    [SerializeField] TextMeshProUGUI ballDistText, flagDistText;
    [SerializeField] Transform ball, flag;
    [SerializeField] Transform ballArrow, flagArrow;
    [SerializeField] Animator canvasAnimator;
    [SerializeField] Canvas mappingCanvas;
    [SerializeField] PolygonCollider2D boundaries;
    [SerializeField] PolygonCollider2D exteriorCollider;
    [SerializeField] RectTransform[] boundsPoints;
    [SerializeField] AudioObj outOfBoundsAudio;
    [SerializeField] AudioObj wrapAudio;
    Camera mainCam;
    Rigidbody ballRB;

    int deathMessageIndex;

    [SerializeField] TextMeshProUGUI deathMessageTxt;

    readonly string[] deathMessages = new string[4] { "Lost in space!", "Burnt to a crisp!", "Fried by X-rays!", "Galaxy Failed!"};
    private void Awake()
    {
        if(ball == null) { ball = GameObject.FindGameObjectWithTag("Player").transform; }
        if(ball != null) { ballRB =  ball.GetComponent<Rigidbody>(); }
        if (flag == null) { flag = GameObject.FindGameObjectWithTag("Hole").transform; }
    }
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        ballArrow.position = -Vector3.one * 9000;
        flagArrow.position = -Vector3.one * 9000;
        MatchCameraColliders();
        boundaries = CameraFollow.innerShape;

        ballDistText.text = "";
        flagDistText.text = "";
    }
    void LateUpdate()
    {
        mappingCanvas.enabled = !LevelManager.levelComplete;
        MatchCameraColliders();

        if(!boundaries.OverlapPoint(ball.position))
        {
            if(GameManager.LevelManager.currentUniverse.universeType != LevelManager.UniverseParams.UniverseType.recursive)
            {
                ShootBall.death?.Invoke(0);
                GameObject.FindGameObjectWithTag("Trail").GetComponent<TrailRenderer> ().enabled = false;
                GameObject.FindGameObjectWithTag("Trail").GetComponent<TrailRenderer> ().emitting = false;
                GameObject.FindGameObjectWithTag("Trail").GetComponent<TrailRenderer> ().Clear();
                ShootBall.triggerReset?.Invoke();
                GameManager.SFXManager.PlaySFX(outOfBoundsAudio);
            }
        }
        
        ArrowToObject(flag, flagArrow, flagDistText);
        ArrowToObject(ball, ballArrow, ballDistText);

    }
    // void ArrowPos(Transform obj, Transform arrow, TextMeshProUGUI distText){
    //     if (!camArea.bounds.Contains(obj.position)){
    //         Vector3 dirtoCamera = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, camArea.transform.position.z) - obj.position;
    //         float zRot = Mathf.Atan2(dirtoCamera.normalized.y, dirtoCamera.normalized.x) * Mathf.Rad2Deg;
    //         arrow.rotation = Quaternion.Euler(0, 0, zRot - 180);
    //         Transform dt = distText.gameObject.transform;
    //         if ((-arrow.transform.up).y > 0)
    //         {
    //             dt.localRotation = Quaternion.Euler(180, 180, 0);
    //             distText.alignment = TextAlignmentOptions.Left;
    //         }
    //         else
    //         {
    //             dt.localRotation = Quaternion.Euler(0, 0, 0);
    //             distText.alignment = TextAlignmentOptions.Right;
    //         }
    //         Vector3 arrowPos = exteriorCollider.ClosestPoint(ball.transform.position);
    //         arrow.position = Camera.main.WorldToScreenPoint(arrowPos + new Vector3(0, 0, -5));
    //     }
    // }
    void ArrowToObject(Transform obj, Transform arrow, TextMeshProUGUI distText)
    {
        if (!camArea.bounds.Contains(obj.position))
        {
            //out of view
            Vector3 dirtoCamera = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, camArea.transform.position.z) - obj.position;
            Ray2D backToCameraRay = new Ray2D(obj.position, dirtoCamera);
            Debug.DrawRay(obj.position, dirtoCamera, Color.red, 0f);
            RaycastHit2D[] hits = Physics2D.RaycastAll(obj.position, dirtoCamera, 1000);
            if (hits != null)
            {
                float dist = 0;
                foreach (RaycastHit2D r in hits)
                {
                    if (r.collider == camArea)
                    {
                        Vector3 point = r.point;
                        dist = Mathf.RoundToInt(Vector3.Distance(point, obj.position) * 10);
                    }
                    if (r.collider == exteriorCollider)
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
                        arrow.position = Camera.main.WorldToScreenPoint(arrowPos + new Vector3(0, 0, -5));
                    }

                }
                distText.text = ((int)dist).ToString();
            }

        }
    }
    void MatchCameraColliders()
    {
        float height = mainCam.orthographicSize * 2; //camera height in units
        float width = height * mainCam.aspect;       //camera width in units
        Vector3 camSize = new Vector3(width, height, 0);

        camArea.transform.localScale = camSize;
        camArea.transform.position = new Vector3(mainCam.transform.position.x, mainCam.transform.position.y, 0);
        ballArrow.position = -Vector3.one * 9000;
        flagArrow.position = -Vector3.one * 9000;
        Vector2[] points = new Vector2[boundsPoints.Length];
        for(int i = 0; i < boundsPoints.Length; i++){
            points[i] = mainCam.ScreenToWorldPoint(boundsPoints[i].position);
        }
        exteriorCollider.points = points; 
    }
    void ShowDeathMessage()
    {
        flagArrow.gameObject.SetActive(false);
        ballArrow.gameObject.SetActive(false);

        deathMessageTxt.text = deathMessages[deathMessageIndex].ToUpper();
        canvasAnimator.Play("ShowOOB");
        StartCoroutine(ShowArrowsAgain());
    }
    void SetDeathMessage(int index){
        deathMessageIndex = index;
        ShowDeathMessage();
    }
    void Pause(){
        flagArrow.gameObject.SetActive(false);
        ballArrow.gameObject.SetActive(false);
    }
    void UnPause(){
        flagArrow.gameObject.SetActive(true);
        ballArrow.gameObject.SetActive(true);
    }
    void Reset(){

    }
    IEnumerator ShowArrowsAgain(){
        yield return new WaitForSeconds(canvasAnimator.GetCurrentAnimatorStateInfo(0).length + 0.25f);
        flagArrow.gameObject.SetActive(true);
        ballArrow.gameObject.SetActive(true);
    }
    private void OnEnable()
    {
        ShootBall.death += SetDeathMessage;
        PauseManager.pauseGame += Pause;
        PauseManager.unpauseGame += UnPause;
        ShootBall.triggerReset += Reset;
    }
    private void OnDisable()
    {
        ShootBall.death -= SetDeathMessage;
        PauseManager.pauseGame -= Pause;
        PauseManager.unpauseGame -= UnPause;
        ShootBall.triggerReset -= Reset;


    }
    private void OnDrawGizmosSelected()
    {
        float height = Camera.main.orthographicSize * 2; //camera height in units
        float width = height * Camera.main.aspect;       //camera width in units
        Vector3 camSize = new Vector3(width, height);
        Gizmos.DrawWireCube(Vector3.zero, camSize - Vector3.one);

        
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < boundsPoints.Length; i++)
        {
            Gizmos.DrawLine(boundsPoints[i].transform.position, boundsPoints[(i + 1) % boundsPoints.Length].transform.position);
            Gizmos.DrawWireCube(boundsPoints[i].transform.position, Vector3.one * 50);
        }
    }
}

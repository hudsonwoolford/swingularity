using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] Animator tutorialAnimator;
    Transform playerBall;
    [Header("Scroll Pan")]
    [SerializeField] GameObject scrollPanParent;
    [Header("Ball Input")]
    [SerializeField] GameObject ballInputParent;
    [SerializeField] LineRenderer ballToCursorLR;
    [SerializeField] Transform fakeCursor;
    bool isPullingBack;
    [Header("Hole Tutorial")]
    [SerializeField] GameObject holeTutorialParent;
    CameraFollow cameraFollow;
    enum TutorialState
    {
        TeachScrollPan,
        TeachBallInput,
        TeachHole,
        TeachNothing,
        TeachHackMode,
        TeachStar,
        TeachComplete
    }
    TutorialState currentState;
    private void Start()
    {
        if(!SettingsManager.userSettings.hints) { GameManager.LevelManager.EnableBallInput(true); Destroy(gameObject); Destroy(this); return; }
        DisableAll();
        tutorialAnimator.Play("TeachScroll");
        ChangeTutorialState(TutorialState.TeachScrollPan);
        DontDestroyOnLoad(gameObject);
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }
    private void Update()
    {
        switch (currentState)
        {
            case TutorialState.TeachScrollPan:
                break;
            case TutorialState.TeachBallInput:
                UpdateBallInput();
                break;
            case TutorialState.TeachHole:
                break;
            case TutorialState.TeachNothing:
                break;
            case TutorialState.TeachHackMode:
                break;
            case TutorialState.TeachStar:
                break;
            case TutorialState.TeachComplete:
                break;
            default:
                break;
        }
    }
    void ChangeTutorialState(TutorialState newState)
    {
        switch (newState)
        {
            case TutorialState.TeachScrollPan:
                StartCoroutine(EnterScrollPan());
                break;
            case TutorialState.TeachBallInput:
                EnterBallInput();
                break;
            case TutorialState.TeachHole:
                ExitBallInput();
                EnterHoleTutorial();
                break;
            case TutorialState.TeachNothing:
                break;
            case TutorialState.TeachHackMode:
                break;
            case TutorialState.TeachStar:
                break;
            case TutorialState.TeachComplete:
                break;
            default:
                break;
        }
        currentState = newState;
    }
    #region State Functions
    #region Scroll Pan
    IEnumerator EnterScrollPan()
    {
        scrollPanParent.SetActive(true);
        GameManager.LevelManager.EnableBallInput(false);
        tutorialAnimator.Play("TeachScroll");
        float timeoffset = Time.time;
        while(Input.mouseScrollDelta.y == 0 && Time.time - timeoffset < 30)
        {
            yield return null;
        }
        timeoffset = Time.time;
        tutorialAnimator.Play("TeachPan");
        while (!Input.GetMouseButton(1) && Time.time - timeoffset < 30)
        {
            yield return null;
        }
        timeoffset = Time.time;
        tutorialAnimator.Play("TeachPanReset");
        while (!Input.GetMouseButton(2) && Time.time - timeoffset < 30)
        {
            yield return null;
        }
        tutorialAnimator.Play("HideScrollPan");
        yield return new WaitForSecondsRealtime(0.3f);
        scrollPanParent.SetActive(false);
        GameManager.LevelManager.EnableBallInput(true);
        ChangeTutorialState(TutorialState.TeachBallInput);
    }
    #endregion
    #region Ball Input
    void EnterBallInput()
    {
        tutorialAnimator.Play("TeachBallInput");
    }
    void ExitBallInput()
    {
    }
    void UpdateBallInput()
    {
        if(ShootBall.liningUpShot) {
            if (ballInputParent.activeInHierarchy)
            {
                ballInputParent.SetActive(false);
            }
            return;
        }
        if(!ballInputParent.activeInHierarchy)
        {
            ballInputParent.SetActive(true);
        }
        if(isPullingBack)
        {
            ballToCursorLR.positionCount = 2;
            ballToCursorLR.SetPosition(0, playerBall.position);
            ballToCursorLR.SetPosition(1, fakeCursor.position);
        }
        else
        {
            ballToCursorLR.positionCount = 0;
        }
    }
    void StartPullBack() //animation event
    {
        isPullingBack = true;
    }
    void EndPullBack() //animation event
    {
        isPullingBack = false;
    }
    #endregion
    #region Hole
    void EnterHoleTutorial()
    {
        StartCoroutine(PlayHoleTutorial());
    }
    IEnumerator PlayHoleTutorial()
    {
        GameManager.LevelManager.EnableBallInput(false);
        cameraFollow.SetTargetZoom(12);
        Time.timeScale = 0.2f;
        holeTutorialParent.SetActive(true);
        tutorialAnimator.Play("TeachHoleTutorial");
        yield return new WaitForSecondsRealtime(5f);
        tutorialAnimator.Play("HideHoleTutorial");
        yield return new WaitForSecondsRealtime(0.3f);
        Time.timeScale = 1;
        DisableAll();
        GameManager.LevelManager.EnableBallInput(true);
    }
    #endregion
    #region Hack Mode

    #endregion
    #endregion
    void DisableAll()
    {
        scrollPanParent.SetActive(false);
        ballInputParent.SetActive(false);
        holeTutorialParent.SetActive(false);
    }
    #region Event Functions
    void OnHackMode(bool enable, Material whiteMat)
    {
        if(enable && currentState == TutorialState.TeachHackMode)
        {
            ChangeTutorialState(TutorialState.TeachStar);
        }
    }
    void OnBallShoot()
    {
        if(currentState == TutorialState.TeachBallInput)
        {
            ChangeTutorialState(TutorialState.TeachHole);
            ShootBall.onShoot -= OnBallShoot;
        }
    }
    void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == "Earth_0")
        {
            //do nothing
        }
        else if (scene.name == "Earth_1")
        {
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);

            ChangeTutorialState(TutorialState.TeachComplete);
        }
        if(playerBall == null)
        {
            playerBall = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
        ShootBall.onShoot += OnBallShoot;
        HackMode.onHackMode += OnHackMode;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        ShootBall.onShoot -= OnBallShoot;
        HackMode.onHackMode -= OnHackMode;
        GameManager.LevelManager.EnableBallInput(true);
    }
    #endregion
}



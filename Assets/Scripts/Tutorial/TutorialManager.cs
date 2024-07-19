using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] Animator tutorialAnimator;
    Transform playerBall;
    public ScrollPanTutorial scrollPanTutorial = new ScrollPanTutorial();
    public BallInputTutorial ballInputTutorial = new BallInputTutorial();
    public TeachHoleTutorial teachHoleTutorial = new TeachHoleTutorial();
    public TeachHackMode teachHackMode = new TeachHackMode();
    public TeachNothing teachNothing = new TeachNothing();
    public bool isPullingBack { get; private set; }
    bool dialogueComplete;
    TutorialState currentState;
    private IEnumerator Start()
    {
        if(!SettingsManager.userSettings.hints) { GameManager.LevelManager.EnableBallInput(true); Destroy(gameObject); Destroy(this); yield break; }
        DontDestroyOnLoad(gameObject);
        DisableAll();
        currentState = teachNothing;
        StartCoroutine(WaitUntilDialogueChangeState(scrollPanTutorial));
    }
    IEnumerator WaitUntilDialogueChangeState(TutorialState state)
    {
        yield return new WaitForSecondsRealtime(0.5f);
        while (!dialogueComplete)
        {
            yield return null;
        }
        ChangeTutorialState(state);
    }
    private void Update()
    {
        if(!dialogueComplete) { return; }
        currentState.OnUpdate();
    }
    #region State Functions
    public void ChangeTutorialState(TutorialState newState)
    {
        if(currentState != null)
        {
            currentState.OnExit();
        }
        currentState = newState;
        currentState.OnEnter(tutorialAnimator, this);
    }
    public void CompleteTutorial()
    {
        DisableAll();
        Destroy(gameObject);
    }
    public Coroutine CallCoroutine(IEnumerator coroutine)
    {
        return StartCoroutine(coroutine);
    }
    public void StopCallCoroutine(Coroutine coroutine)
    {
        StopCoroutine(coroutine);
    }
    void StartPullBack() //animation event
    {
        isPullingBack = true;
    }
    void EndPullBack() //animation event
    {
        isPullingBack = false;
    }
    void DisableAll()
    {
        scrollPanTutorial.DisableState();
        ballInputTutorial.DisableState();
        teachHoleTutorial.DisableState();
        teachHackMode.DisableState();
    }
    #endregion
    #region Event Functions
    void OnEnableDialogue(bool enable)
    {
        if(!enable)
        {
            dialogueComplete = true;
        }
    }
    void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name == "Earth_0" || scene.name == "Earth_1")
        {
            if(scene.name == "Earth_1")
            {
                DisableAll();
                StartCoroutine(WaitUntilDialogueChangeState(teachHackMode));
            }
        }
        else
        {
            DisableAll();
            Destroy(gameObject);
            return;
        }
        if (playerBall == null)
        {
            playerBall = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
        TIIGER.onEnableDialogue += OnEnableDialogue;
    }
    private void OnDisable()
    {
        TIIGER.onEnableDialogue -= OnEnableDialogue;
        SceneManager.sceneLoaded -= OnSceneLoad;
        GameManager.LevelManager.EnableBallInput(true);
    }
    #endregion
}
[System.Serializable]
public abstract class TutorialState
{
    protected TutorialManager tutorialManager;
    protected Animator tutorialAnimator;
    public virtual void OnEnter(Animator ta, TutorialManager tm)
    {
        tutorialAnimator = ta;
        tutorialManager = tm;
    }
    public virtual void OnUpdate()
    {

    }
    public virtual void OnExit()
    {

    }
    public virtual void DisableState()
    {
        //disable smth
    }
}
[System.Serializable]
public class ScrollPanTutorial : TutorialState
{
    [SerializeField] protected GameObject scrollPanParent;
    public override void OnEnter(Animator ta, TutorialManager tm)
    {
        base.OnEnter(ta, tm);
        tutorialManager.CallCoroutine(EnterScrollPan());
    }
    private IEnumerator EnterScrollPan()
    {
        scrollPanParent.SetActive(true);
        GameManager.LevelManager.EnableBallInput(false);
        tutorialAnimator.Play("TeachScroll");
        float timeoffset = Time.time;
        while (Input.mouseScrollDelta.y == 0 && Time.time - timeoffset < 30)
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
        tutorialManager.ChangeTutorialState(tutorialManager.ballInputTutorial);
    }
    public override void DisableState()
    {
        base.DisableState();
        scrollPanParent.SetActive(false);
    }
}
[System.Serializable]
public class BallInputTutorial : TutorialState
{
    [SerializeField]protected GameObject ballInputParent;
    [SerializeField]protected Transform fakeCursor;
    [SerializeField]protected LineRenderer ballToCursorLR;
    protected Transform playerBall;
    public override void OnEnter(Animator ta, TutorialManager tm)
    {
        base.OnEnter(ta, tm);
        ShootBall.onShoot += OnBallShoot;
        playerBall = GameObject.FindGameObjectWithTag("Player").transform;
        tutorialAnimator.Play("TeachBallInput");
    }
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (ShootBall.liningUpShot)
        {
            if (ballInputParent.activeInHierarchy)
            {
                ballInputParent.SetActive(false);
            }
            return;
        }
        if (!ballInputParent.activeInHierarchy)
        {
            ballInputParent.SetActive(true);
        }
        if (tutorialManager.isPullingBack)
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
    void OnBallShoot()
    {
        tutorialManager.ChangeTutorialState(tutorialManager.teachHoleTutorial);
        ShootBall.onShoot -= OnBallShoot;
    }
    public override void DisableState()
    {
        base.DisableState();
        ShootBall.onShoot -= OnBallShoot;//safety
        ballInputParent.SetActive(false);
    }
}
[System.Serializable]
public class TeachHoleTutorial : TutorialState
{
    [SerializeField] protected GameObject teachHoleParent;
    protected CameraFollow cameraFollow;
    Coroutine holeCoroutine;
    public override void OnEnter(Animator ta, TutorialManager tm)
    {
        base.OnEnter(ta, tm);
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        holeCoroutine = tutorialManager.CallCoroutine(PlayHoleTutorial());
    }
    IEnumerator PlayHoleTutorial()
    {
        GameManager.LevelManager.EnableBallInput(false);
        cameraFollow.SetTargetZoom(12);
        Time.timeScale = 0.2f;
        teachHoleParent.SetActive(true);
        tutorialAnimator.Play("TeachHoleTutorial");
        yield return new WaitForSecondsRealtime(3f);
        tutorialAnimator.Play("HideHoleTutorial");
        yield return new WaitForSecondsRealtime(0.3f);
        Time.timeScale = 1;
        GameManager.LevelManager.EnableBallInput(true);
        tutorialManager.ChangeTutorialState(tutorialManager.teachNothing);
    }
    public override void DisableState()
    {
        base.DisableState();
        if(holeCoroutine != null) { tutorialManager.StopCallCoroutine(holeCoroutine); Time.timeScale = 1; }
        teachHoleParent.SetActive(false);
    }
}
[System.Serializable]
public class TeachNothing : TutorialState
{

}
[System.Serializable]
public class TeachHackMode : TutorialState
{
    [SerializeField] protected GameObject hackModeParent;
    public override void OnEnter(Animator ta, TutorialManager tm)
    {
        base.OnEnter(ta, tm);
        HackMode.onHackMode += OnHackMode;
        hackModeParent.SetActive(true);
        GameManager.LevelManager.EnableBallInput(false);
        tutorialAnimator.Play("TeachHackMode");
    }
    public override void OnUpdate()
    {
        base.OnUpdate();
    }
    public override void OnExit()
    {
        base.OnExit();
    }
    void OnHackMode(bool enable, Material whiteMat)
    {
        GameManager.LevelManager.EnableBallInput(true);
        tutorialAnimator.Play("HideHackMode");
        tutorialManager.ChangeTutorialState(tutorialManager.teachNothing);
        HackMode.onHackMode -= OnHackMode;
        tutorialManager.CompleteTutorial();
    }
    public override void DisableState()
    {
        hackModeParent.SetActive(false);
        HackMode.onHackMode -= OnHackMode;//safety
        base.DisableState();
    }
}




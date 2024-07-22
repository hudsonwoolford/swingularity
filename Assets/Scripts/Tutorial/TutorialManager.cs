using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;
    [SerializeField] Animator tutorialAnimator;
    Transform playerBall;
    public ScrollPanTutorial scrollPanTutorial = new ScrollPanTutorial();
    public BallInputTutorial ballInputTutorial = new BallInputTutorial();
    public TeachHoleTutorial teachHoleTutorial = new TeachHoleTutorial();
    public TeachHackMode teachHackMode = new TeachHackMode();
    public TeachResetTutorial resetTutorial = new TeachResetTutorial();
    public TeachPause teachPause = new TeachPause();
    public TeachNothing teachNothing = new TeachNothing();
    public bool isPullingBack { get; private set; }
    TutorialState currentState;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }    
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private IEnumerator Start()
    {
        if(!SettingsManager.userSettings.hints) { GameManager.LevelManager.EnableBallInput(true); Destroy(gameObject); Destroy(this); yield break; }
        DontDestroyOnLoad(gameObject);
        DisableAll();
        currentState = teachNothing;
        while(SceneHandling.loadingScreen)
        {
            yield return null;
        }
        StartCoroutine(WaitUntilDialogueChangeState(scrollPanTutorial));
    }
    IEnumerator WaitUntilDialogueChangeState(TutorialState state)
    {
        GameManager.LevelManager.EnableBallInput(false);
        do
        {
            yield return null;
        } while (SceneHandling.loadingScreen);
        do
        {
            yield return null;
        } while (TIIGER.Instance.dialogueShown);
        ChangeTutorialState(state);
    }
    private void Update()
    {
        if (TIIGER.Instance.dialogueShown || SceneHandling.loadingScreen) { return; }
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
        resetTutorial.DisableState();
        teachPause.DisableState();
    }
    #endregion
    #region Event Functions
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
    void OnGamePause()
    {
        if(currentState != null)
        {
            currentState.DisableState();
        }
    }
    void OnGameUnpause()
    {
        if (currentState != null)
        {
            currentState.EnableState();
        }
    }
    private void OnEnable()
    {
        PauseManager.pauseGame += OnGamePause;
        PauseManager.unpauseGame += OnGameUnpause;
        SceneManager.sceneLoaded += OnSceneLoad;
    }
    private void OnDisable()
    {
        PauseManager.unpauseGame -= OnGameUnpause;
        PauseManager.pauseGame -= OnGamePause;
        SceneManager.sceneLoaded -= OnSceneLoad;
        GameManager.LevelManager.EnableBallInput(true);
    }
    #endregion
}
[System.Serializable]
public abstract class TutorialState
{
    [SerializeField] protected GameObject tutorialParent;
    protected TutorialManager tutorialManager;
    protected Animator tutorialAnimator;
    public virtual void OnEnter(Animator ta, TutorialManager tm)
    {
        tutorialAnimator = ta;
        tutorialManager = tm;
    }
    public virtual void OnUpdate()
    {
        //called on update
    }
    public virtual void OnExit()
    {
        //called when tutorial state progressed
    }
    public virtual void EnableState()
    {
        tutorialParent.SetActive(true);
        //enable state
    }
    public virtual void DisableState()
    {
        tutorialParent.SetActive(false);
        //disable state
    }
}
[System.Serializable]
public class ScrollPanTutorial : TutorialState
{
    public override void OnEnter(Animator ta, TutorialManager tm)
    {
        base.OnEnter(ta, tm);
        tutorialManager.CallCoroutine(EnterScrollPan());
    }
    private IEnumerator EnterScrollPan()
    {
        tutorialParent.SetActive(true);
        GameManager.LevelManager.EnableBallInput(false);
        tutorialAnimator.Play("TeachScroll");
        tutorialAnimator.Play("ScrollAnim");
        float timeoffset = Time.time;
        while (Input.mouseScrollDelta.y == 0 && Time.time - timeoffset < 30)
        {
            yield return null;
        }
        timeoffset = Time.time;
        tutorialAnimator.Play("TeachPan");
        tutorialAnimator.Play("PanAnim");
        while (!Input.GetMouseButton(1) && Time.time - timeoffset < 30)
        {
            yield return null;
        }
        timeoffset = Time.time;
        tutorialAnimator.Play("TeachPanReset");
        tutorialAnimator.Play("ResetPanAnim");
        while (!Input.GetMouseButton(2) && Time.time - timeoffset < 30)
        {
            yield return null;
        }
        tutorialAnimator.Play("HideScrollPan");
        yield return new WaitForSecondsRealtime(0.3f);
        tutorialParent.SetActive(false);
        GameManager.LevelManager.EnableBallInput(true);
        tutorialManager.ChangeTutorialState(tutorialManager.ballInputTutorial);
    }
    public override void EnableState()
    {
        base.EnableState();
    }
    public override void DisableState()
    {
        base.DisableState();
    }
}
[System.Serializable]
public class BallInputTutorial : TutorialState
{
    [SerializeField]protected Transform fakeCursor;
    [SerializeField]protected LineRenderer ballToCursorLR;
    protected Transform playerBall;
    ShootBall shootBall;
    public override void OnEnter(Animator ta, TutorialManager tm)
    {
        base.OnEnter(ta, tm);
        shootBall = GameObject.FindGameObjectWithTag("Player").GetComponent<ShootBall>();
        ShootBall.onShoot += OnBallShoot;
        playerBall = GameObject.FindGameObjectWithTag("Player").transform;
        tutorialAnimator.Play("TeachBallInput");
    }
    public override void OnUpdate()
    {
        base.OnUpdate();
        if (shootBall.liningUpShot)
        {
            if (tutorialParent.activeInHierarchy)
            {
                tutorialParent.SetActive(false);
            }
            return;
        }
        if (!tutorialParent.activeInHierarchy)
        {
            tutorialParent.SetActive(true);
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
    public override void EnableState()
    {
        base.EnableState();
    }
    public override void DisableState()
    {
        base.DisableState();
        ShootBall.onShoot -= OnBallShoot;//safety
    }
}
[System.Serializable]
public class TeachHoleTutorial : TutorialState
{
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
        tutorialParent.SetActive(true);
        tutorialAnimator.Play("TeachHoleTutorial");
        yield return new WaitForSeconds(3f * 0.2f);
        tutorialAnimator.Play("HideHoleTutorial");
        yield return new WaitForSeconds(0.3f * 0.2f);
        Time.timeScale = 1;
        GameManager.LevelManager.EnableBallInput(true);
        tutorialManager.ChangeTutorialState(tutorialManager.teachNothing);
    }
    public override void EnableState()
    {
        base.EnableState();
    }
    public override void DisableState()
    {
        base.DisableState();
        if(holeCoroutine != null) { tutorialManager.StopCallCoroutine(holeCoroutine); Time.timeScale = 1; }
    }
}
[System.Serializable]
public class TeachNothing : TutorialState
{

}
[System.Serializable]
public class TeachHackMode : TutorialState
{
    public override void OnEnter(Animator ta, TutorialManager tm)
    {
        base.OnEnter(ta, tm);
        HackMode.onHackMode += OnHackMode;
        tutorialParent.SetActive(true);
        GameManager.LevelManager.EnableBallInput(false);
        tutorialAnimator.Play("TeachHackMode");
        tutorialAnimator.Play("HackModeKeybindBlink");
    }
    public override void OnUpdate()
    {
        base.OnUpdate();
    }
    public override void OnExit()
    {
        tutorialParent.SetActive(false);
        HackMode.onHackMode -= OnHackMode;//safety
        base.OnExit();
    }
    void OnHackMode(bool enable, Material whiteMat)
    {
        GameManager.LevelManager.EnableBallInput(true);
        tutorialAnimator.Play("HideHackMode");
        tutorialManager.ChangeTutorialState(tutorialManager.teachPause);
        HackMode.onHackMode -= OnHackMode;
    }
    public override void EnableState()
    {
        base.EnableState();
    }
    public override void DisableState()
    {
        base.DisableState();
        HackMode.onHackMode -= OnHackMode;//safety
    }
}
[System.Serializable]
public class TeachPause : TutorialState
{
    public override void OnEnter(Animator ta, TutorialManager tm)
    {
        base.OnEnter(ta, tm);
        PauseManager.pauseGame += OnPause;
        tutorialParent.SetActive(true);

        GameManager.LevelManager.EnableBallInput(false);
        tutorialAnimator.Play("TeachPauseTutorial");
        tutorialAnimator.Play("PauseKeybindBlink");
    }
    public override void OnUpdate()
    {
        base.OnUpdate();
    }
    public override void OnExit()
    {
        tutorialParent.SetActive(false);
        PauseManager.pauseGame -= OnPause;//safety
        base.OnExit();
    }
    void OnPause()
    {
        GameManager.LevelManager.EnableBallInput(true);
        tutorialAnimator.Play("HidePauseTutorial");
        tutorialManager.ChangeTutorialState(tutorialManager.resetTutorial);
        PauseManager.pauseGame -= OnPause;
    }
    public override void EnableState()
    {
        base.EnableState();
    }
    public override void DisableState()
    {
        base.DisableState();
        PauseManager.pauseGame -= OnPause;//safety
    }
}
[System.Serializable]
public class TeachResetTutorial : TutorialState
{
    protected bool ballReset = false;
    public override void OnEnter(Animator ta, TutorialManager tm)
    {
        base.OnEnter(ta, tm);
        ShootBall.triggerReset += OnBallReset;
        ShootBall.holeAnim += OnBallReset;
        ShootBall.onShoot += OnBallShoot;
    }
    IEnumerator ResetTutorial()
    {
        tutorialParent.SetActive(true);
        tutorialAnimator.Play("TeachResetTutorial");
        tutorialAnimator.Play("ResetKeybindBlink");
        Time.timeScale = 0.2f;
        float timeOffset = Time.time;
        while (!ballReset && Time.time - timeOffset < 3 * 0.2f)
        {
            yield return null;
        }
        Time.timeScale = 1f;
        tutorialAnimator.Play("HideResetTutorial");
        tutorialManager.ChangeTutorialState(tutorialManager.teachNothing);
        tutorialManager.CompleteTutorial();
    }
    void OnBallShoot()
    {
        ShootBall.onShoot -= OnBallShoot;
        tutorialManager.CallCoroutine(ResetTutorial());
    }
    void OnBallReset()
    {
        ballReset = true;
        ShootBall.holeAnim -= OnBallReset;
        ShootBall.triggerReset -= OnBallReset;
    }
    public override void EnableState()
    {
        base.EnableState();
    }
    public override void DisableState()
    {
        base.DisableState();
        ShootBall.holeAnim -= OnBallReset;
        ShootBall.onShoot -= OnBallShoot; //safety
        ShootBall.triggerReset -= OnBallReset; //safety
    }
}





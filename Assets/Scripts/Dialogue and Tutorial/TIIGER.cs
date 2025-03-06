using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TIIGER : EventTalkableNPC
{
    public static TIIGER Instance;

    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private Animator tgrAnimator;
    [SerializeField] private Canvas dialogueCanvas;
    [SerializeField] private MeshRenderer ballRenderer;
    private Material hologramMaterial;
    private bool isTransitioning;
    public bool dialogueShown { get; private set; }
    public delegate void OnEnableDialogue(bool enable);
    public static OnEnableDialogue onEnableDialogue;
    Coroutine displayRoutine, hideRoutine;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    private void Start()
    {
        tgrAnimator.Play("DisableDialogue");
        hologramMaterial = ballRenderer.GetComponent<MeshRenderer>().material;
    }
    public void SkipConversation()
    {
        dialogueController.EndConversation();
    }
    protected override void Update()
    {
        //input
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape))
        {
            Interact();
        }

        //logic
        if (!SettingsManager.userSettings.dialogue || (GameManager.LevelManager.isReset && GameManager.LevelManager.isCampaignFinalLevel) || GameManager.LevelManager.currentUniverse.universeType != LevelManager.UniverseParams.UniverseType.home)
        {
            return;
        }
        if(dialogueController.isConversation && LevelManager.ballInput)
        {
            GameManager.LevelManager.EnableBallInput(false);
        }
        hologramMaterial.SetFloat("_UnscaledTime", Time.unscaledTime);
        if(dialogueQueued) { dialogueShown |= dialogueQueued; }
        if (dialogueShown && !dialogueController.isConversation && !dialogueQueued)
        {
            displayRoutine = StartCoroutine(HideDialogue());
        }
        if(Input.GetKeyDown(KeyCode.Escape) && dialogueShown && !dialogueQueued)
        {
            dialogueController.EndConversation();
            hideRoutine = StartCoroutine(HideDialogue());
        }
    }
    public override void Interact()
    {
        if (!SettingsManager.userSettings.dialogue || (GameManager.LevelManager.isReset && !GameManager.LevelManager.isCampaignFinalLevel) || GameManager.LevelManager.currentUniverse.universeType != LevelManager.UniverseParams.UniverseType.home)
        {
            return;
        }
        if (!isTransitioning && dialogueShown && !dialogueQueued)
        {
            dialogueController.DisplayNextParagraph();
        }
    }
    public override void Talk(DialogueText dialogueText)
    {
        if (!SettingsManager.userSettings.dialogue || (GameManager.LevelManager.isReset && !GameManager.LevelManager.isCampaignFinalLevel) || GameManager.LevelManager.currentUniverse.universeType != LevelManager.UniverseParams.UniverseType.home)
        {
            return;
        }
        displayRoutine = StartCoroutine(DisplayDialogue(dialogueText));
    }
    IEnumerator DisplayDialogue(DialogueText dialogueText)
    {
        //stop other routine if happening
        if(hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }

        isTransitioning = true;
        GameManager.LevelManager.EnableBallInput(false);
        dialogueController.EndConversation();

        Time.timeScale = 0;

        //animation
        onEnableDialogue?.Invoke(true);
        dialogueShown = true;
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z);
        tgrAnimator.Play("ShowTgrDialogue");
        tgrAnimator.SetBool("Speaking", true);
        dialogueController.StartConversation(dialogueText); //sets text

        //stop dialogue
        dialogueController.TogglePauseDialogue();
        yield return new WaitForSecondsRealtime(tgrAnimator.GetCurrentAnimatorClipInfo(0).Length);
        dialogueController.TogglePauseDialogue();

        isTransitioning = false;
    }
    IEnumerator HideDialogue()
    {
        //stop other routine if happening
        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
        }

        isTransitioning = true;
        GameManager.LevelManager.EnableBallInput(true);
        Time.timeScale = 1;

        //animation
        onEnableDialogue?.Invoke(false);
        dialogueShown = false;
        tgrAnimator.SetBool("Speaking", false);

        //stop dialogue
        dialogueController.DialogueCloseSounds();
        dialogueController.TogglePauseDialogue();
        yield return new WaitForSecondsRealtime(tgrAnimator.GetCurrentAnimatorClipInfo(0).Length);
        dialogueController.TogglePauseDialogue();

        isTransitioning = false;
    }
    protected override void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        base.OnSceneLoad(scene, loadSceneMode);
        dialogueCanvas.worldCamera = Camera.main;
        if(dialogueShown)
        {
            dialogueController.EndConversation();
            StopAllCoroutines();
            tgrAnimator.Play("DisableDialogue");
        }
    }
}

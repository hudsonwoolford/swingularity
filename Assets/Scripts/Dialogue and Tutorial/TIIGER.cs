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
    [SerializeField] private Light pointLight;
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
        //ensure dialogue is enabled
        if (!SettingsManager.userSettings.dialogue || (GameManager.LevelManager.isReset && GameManager.LevelManager.isCampaignFinalLevel))
        {
            return;
        }

        //scale tablet to camera
        if(dialogueShown || isTransitioning)
        { 
            transform.localScale = Vector3.one * Camera.main.orthographicSize / 8;
            pointLight.range = 10 * Camera.main.orthographicSize / 8;
            transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z);
        }

        //input
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape))
        {
            Interact();
        }

        //animate the hologram
        hologramMaterial.SetFloat("_UnscaledTime", Time.unscaledTime);

        //dialogueShown should be true if dialogue is queued
        if(dialogueQueued) { dialogueShown |= dialogueQueued; }

        if(!dialogueQueued && dialogueShown)
        {
            //check if talking and ball input is still true
            if(dialogueController.isConversation && LevelManager.ballInput)
            {
                GameManager.LevelManager.EnableBallInput(false);
            }
            //check if convo is complete and dialogue is still showing
            else if (!dialogueController.isConversation)
            {
                displayRoutine = StartCoroutine(HideDialogue());
            }
            //check if user escapes
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                dialogueController.EndConversation();
                hideRoutine = StartCoroutine(HideDialogue());
            }
        }
    }
    public override void Interact()
    {
        if (!SettingsManager.userSettings.dialogue || (GameManager.LevelManager.isReset && !GameManager.LevelManager.isCampaignFinalLevel))
        {
            return;
        }

        //if interacting while dialogue shown, skip dialogue paragraph
        if (!isTransitioning && dialogueShown && !dialogueQueued)
        {
            dialogueController.DisplayNextParagraph();
        }
    }
    public override void Talk(DialogueText dialogueText)
    {
        if (!SettingsManager.userSettings.dialogue || (GameManager.LevelManager.isReset && !GameManager.LevelManager.isCampaignFinalLevel))
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
        dialogueController.TogglePauseDialogue(true);
        yield return new WaitForSecondsRealtime(tgrAnimator.GetCurrentAnimatorClipInfo(0).Length);
        dialogueController.TogglePauseDialogue(false);

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
        dialogueController.TogglePauseDialogue(true);
        yield return new WaitForSecondsRealtime(tgrAnimator.GetCurrentAnimatorClipInfo(0).Length);
        dialogueController.TogglePauseDialogue(false);

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

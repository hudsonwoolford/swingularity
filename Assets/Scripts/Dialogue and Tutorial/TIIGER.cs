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
    [SerializeField] private Button skipButton;
    [SerializeField] private MeshRenderer ballRenderer;
    private Material hologramMaterial;
    private bool isTransitioning;
    public bool dialogueShown { get; private set; }
    public delegate void OnEnableDialogue(bool enable);
    public static OnEnableDialogue onEnableDialogue;


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
        skipButton.onClick.AddListener(() => dialogueController.EndConversation());
        hologramMaterial = ballRenderer.GetComponent<MeshRenderer>().material;
    }
    protected override void Update()
    {
        base.Update();
        if (!SettingsManager.userSettings.dialogue || (GameManager.LevelManager.isReset && SceneManager.GetActiveScene().name != "Mythical_4"))
        {
            return;
        }
        if (dialogueShown && !dialogueController.isConversation && !dialogueQueued)
        {
            StartCoroutine(HideDialogue());
        }
        if(dialogueController.isConversation && LevelManager.ballInput)
        {
            GameManager.LevelManager.EnableBallInput(false);
        }
        hologramMaterial.SetFloat("_UnscaledTime", Time.unscaledTime);
        if(dialogueQueued) { dialogueShown |= dialogueQueued; }
        if(Input.GetKeyDown(KeyCode.Escape) && dialogueShown && !dialogueQueued)
        {
            dialogueController.EndConversation();
            StartCoroutine(HideDialogue());
        }
    }
    public override void Interact()
    {
        if (!SettingsManager.userSettings.dialogue || (GameManager.LevelManager.isReset && SceneManager.GetActiveScene().name != "Mythical_4"))
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
        if (!SettingsManager.userSettings.dialogue || (GameManager.LevelManager.isReset && SceneManager.GetActiveScene().name != "Mythical_4"))
        {
            return;
        }
        StartCoroutine(DisplayDialogue(dialogueText));
    }
    IEnumerator DisplayDialogue(DialogueText dialogueText)
    {
        isTransitioning = true;
        GameManager.LevelManager.EnableBallInput(false);
        dialogueController.EndConversation();

        Time.timeScale = 0;

        //animation
        onEnableDialogue?.Invoke(true);
        dialogueShown = true;
        tgrAnimator.Play("ShowTgrDialogue");
        dialogueController.StartConversation(dialogueText); //sets text

        //stop dialogue
        dialogueController.TogglePauseDialogue();
        yield return new WaitForSecondsRealtime(tgrAnimator.GetCurrentAnimatorClipInfo(0).Length);
        dialogueController.TogglePauseDialogue();

        isTransitioning = false;
    }
    IEnumerator HideDialogue()
    {
        isTransitioning = true;
        GameManager.LevelManager.EnableBallInput(true);
        Time.timeScale = 1;

        //animation
        onEnableDialogue?.Invoke(false);
        dialogueShown = false;
        tgrAnimator.Play("HideTgrDialogue");

        //stop dialogue
        dialogueController.TogglePauseDialogue();
        yield return new WaitForSecondsRealtime(tgrAnimator.GetCurrentAnimatorClipInfo(0).Length);
        dialogueController.TogglePauseDialogue();

        isTransitioning = false;
    }
    protected override void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        base.OnSceneLoad(scene, loadSceneMode);
        if(dialogueShown)
        {
            dialogueController.EndConversation();
            StopAllCoroutines();
            tgrAnimator.Play("DisableDialogue");
        }
    }
}

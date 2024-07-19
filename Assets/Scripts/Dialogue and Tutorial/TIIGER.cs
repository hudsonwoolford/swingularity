using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TIIGER : EventTalkableNPC
{
    public static TIIGER Instance;

    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private Animator tgrAnimator;
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
    }
    protected override void Update()
    {
        base.Update();
        if(dialogueShown && !dialogueController.isConversation)
        {
            StartCoroutine(HideDialogue());
        }
        if(dialogueController.isConversation && LevelManager.ballInput)
        {
            GameManager.LevelManager.EnableBallInput(false);
        }
    }
    public override void Interact()
    {
        if(!isTransitioning && dialogueShown)
        {
            dialogueController.DisplayNextParagraph();
        }
    }
    public override void Talk(DialogueText dialogueText)
    {
        StartCoroutine(DisplayDialogue(dialogueText));
    }
    IEnumerator DisplayDialogue(DialogueText dialogueText)
    {
        isTransitioning = true;
        GameManager.LevelManager.EnableBallInput(false);

        //animation
        onEnableDialogue?.Invoke(true);
        dialogueShown = true;
        tgrAnimator.Play("ShowTgrDialogue");
        dialogueController.StartConversation(dialogueText); //sets text

        //stop dialogue
        dialogueController.TogglePauseDialogue();
        yield return new WaitForSeconds(tgrAnimator.GetCurrentAnimatorClipInfo(0).Length);
        dialogueController.TogglePauseDialogue();

        isTransitioning = false;
    }
    IEnumerator HideDialogue()
    {
        isTransitioning = true;
        GameManager.LevelManager.EnableBallInput(false);

        //animation
        onEnableDialogue?.Invoke(false);
        dialogueShown = false;
        tgrAnimator.Play("HideTgrDialogue");

        //stop dialogue
        dialogueController.TogglePauseDialogue();
        yield return new WaitForSeconds(tgrAnimator.GetCurrentAnimatorClipInfo(0).Length);
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

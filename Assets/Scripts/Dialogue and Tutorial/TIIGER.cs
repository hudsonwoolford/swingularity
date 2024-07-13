using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TIIGER : EventTalkableNPC
{
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private Animator tgrAnimator;
    private bool isTransitioning;
    private bool dialogueShown;
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

        //animation
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

        //animation
        dialogueShown = false;
        tgrAnimator.Play("HideTgrDialogue");

        //stop dialogue
        dialogueController.TogglePauseDialogue();
        yield return new WaitForSeconds(tgrAnimator.GetCurrentAnimatorClipInfo(0).Length);
        dialogueController.TogglePauseDialogue();

        isTransitioning = false;
    }
}

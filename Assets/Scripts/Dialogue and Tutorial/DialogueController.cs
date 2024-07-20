using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NPCNameText;
    [SerializeField] private TextMeshProUGUI NPCDialogueText;
    [SerializeField] private float typeSpeed = 5;
    [SerializeField] private float minParagraphInterval = 5;

    private Queue<string> paragraphs = new Queue<string>();
    public bool isConversation { get; private set; }
    private bool isTyping;
    private bool isPaused;
    private string p;

    private Coroutine typeDialogueCoroutine;
    private Coroutine paragraphIntervalCoroutine;

    private const string HTML_ALPHA = "<color=#00000000>";
    private const float MAX_TYPE_TIME = 0.1f;
    public void StartConversation(DialogueText dialogueText)
    {
        isConversation = true;
        //update speaker name
        NPCNameText.text = dialogueText.speakerName;

        //add dialogue text to queue
        for (int i = 0; i < dialogueText.paragraphs.Length; i++)
        {
            paragraphs.Enqueue(dialogueText.paragraphs[i]);
        }

        //start talking
        DisplayNextParagraph();
    }
    public void DisplayNextParagraph()
    {
        //nothing in queue
        if(paragraphs.Count == 0 && !isTyping)
        {
            EndConversation();
            return;
        }

        if (!isTyping)
        {
            if(paragraphIntervalCoroutine != null)
            {
                StopCoroutine(paragraphIntervalCoroutine);
            }
            p = paragraphs.Dequeue();
            typeDialogueCoroutine = StartCoroutine(TypeDialogueText(p));
        }
        else
        {
            FinishParagraph();
        }
    }
    public void TogglePauseDialogue()
    {
        //toggle pause boolean
        isPaused = !isPaused;
    }
    public void EndConversation()
    {
        //clear queue
        paragraphs.Clear();

        //return bool to false
        isConversation = false;
    }
    private IEnumerator TypeDialogueText(string p)
    {
        isTyping = true;
        NPCDialogueText.text = "";

        string originalText = p;
        string displayedText = "";
        int alphaIndex = 0;

        foreach (char c in p.ToCharArray())
        {
            yield return new WaitForSecondsRealtime(MAX_TYPE_TIME / typeSpeed);
            yield return new WaitUntil(() => !isPaused);
            alphaIndex++;
            NPCDialogueText.text = originalText;

            displayedText = NPCDialogueText.text.Insert(alphaIndex, HTML_ALPHA);
            NPCDialogueText.text = displayedText;
        }
        isTyping = false;
        FinishParagraph();
    }
    private IEnumerator ParagraphTimeInterval()
    {
        yield return new WaitForSecondsRealtime(minParagraphInterval);
        if (paragraphs.Count == 0) { EndConversation(); }
        else { DisplayNextParagraph(); }
    }
    private void FinishParagraph()
    {
        //stop coroutines
        StopCoroutine(typeDialogueCoroutine);
        paragraphIntervalCoroutine = StartCoroutine(ParagraphTimeInterval());

        //finish typing text
        NPCDialogueText.text = p;

        //update typing bool
        isTyping = false;
    }
}

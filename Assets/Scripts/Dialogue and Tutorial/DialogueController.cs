using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;
using System.Text;

public class DialogueController : MonoBehaviour
{
    [System.Serializable]
    struct CharacterTypeInfo
    {
        public char character;
        public float waitMult;
        public bool hideCharacter;
    }

    [SerializeField] private TextMeshProUGUI NPCNameText;
    [SerializeField] private TextMeshProUGUI NPCDialogueText;
    [SerializeField] private float typeSpeed = 5;
    [SerializeField] private float minParagraphInterval = 5;
    [SerializeField] private CharacterTypeInfo[] characterTypeInfo;
    [SerializeField] private char crackleCharacter;
    [SerializeField] AudioObj talkObj;
    [SerializeField] AudioObj staticObject;
    [SerializeField] AudioSource tgrAudioSrc;
    [SerializeField] char[] soundCharacters;

    private Queue<string> paragraphs = new Queue<string>();
    public bool isConversation { get; private set; }
    private bool isTyping;
    private bool isPaused;
    private string p;

    private Coroutine typeDialogueCoroutine;
    private Coroutine paragraphIntervalCoroutine;

    private DialogueText currentDialogueTxt;

    private const string HTML_ALPHA = "<color=#00000000>";
    private const float MAX_TYPE_TIME = 0.1f;
    public void StartConversation(DialogueText dialogueText)
    {
        currentDialogueTxt = dialogueText;
        isConversation = true;
        //update speaker name
        if(!dialogueText.revealNameLater)
        {
            NPCNameText.text = dialogueText.speakerName;
        }
        else
        {
            NPCNameText.text = dialogueText.hiddenSpeakerName;
        }

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
        currentDialogueTxt = null;
        StopAllCoroutines();
        NPCDialogueText.text = "";
        //clear queue
        paragraphs.Clear();
        p = "";

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
        float waitMultiplier = 1;
        int characterIteration = 0;
        foreach (char c in p.ToCharArray())
        {
            //wait multiplier
            waitMultiplier = 1;
            if(c == crackleCharacter)
            {
                originalText = originalText.Remove(characterIteration, 1);
                alphaIndex--;
                GameManager.SFXManager.PlaySFX(staticObject);
            }
            if(currentDialogueTxt.revealNameLater && c =='@')
            {
                alphaIndex--;
                NPCNameText.text = currentDialogueTxt.speakerName;
                originalText = originalText.Remove(characterIteration);
            }
            foreach (CharacterTypeInfo ct in characterTypeInfo)
            {
                if(c == ct.character) { waitMultiplier = ct.waitMult;
                    if (ct.hideCharacter)
                    {
                        originalText = originalText.Replace(c, ' ');
                    }
                }
            }

            //dialogue noise
            if (characterIteration % 4 == 0) //every fourth characteer
            {
                if (soundCharacters.Contains(char.ToUpper(c))) //if it is a sound character
                {
                    tgrAudioSrc.pitch = Mathf.Lerp(0.05f, 0.25f, Array.IndexOf(soundCharacters, char.ToUpper(c)) / (float)soundCharacters.Length);
                    GameManager.SFXManager.PlaySFX(talkObj, tgrAudioSrc, 1f);
                }
            }
            yield return new WaitForSecondsRealtime(MAX_TYPE_TIME / typeSpeed);
            yield return new WaitUntil(() => !isPaused);

            alphaIndex++;
            NPCDialogueText.text = originalText;

            displayedText = NPCDialogueText.text.Insert(alphaIndex, HTML_ALPHA);
            NPCDialogueText.text = displayedText;

            yield return new WaitForSecondsRealtime(MAX_TYPE_TIME / typeSpeed * waitMultiplier - MAX_TYPE_TIME / typeSpeed);
            characterIteration++;
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
        StringBuilder finishedPara = new StringBuilder();
        for (int i = 0; i < p.Length; i++)
        {
            bool hideChar = false;
            if (p[i] == crackleCharacter)
            {
                //GameManager.SFXManager.PlaySFX(staticObject);
                hideChar = true;
            }
            if (p[i] == '@' && currentDialogueTxt.revealNameLater)
            {
                hideChar = true;
                NPCNameText.text = currentDialogueTxt.speakerName;
            }
            foreach (CharacterTypeInfo ct in characterTypeInfo)
            {
                if (p[i] == ct.character)
                {
                    if (ct.hideCharacter)
                    {
                        hideChar = true;
                    }
                }
            }
            if(!hideChar)
            {
                finishedPara.Append(p[i]);
            }
        }
        NPCDialogueText.text = finishedPara.ToString();

        //update typing bool
        isTyping = false;
    }
}

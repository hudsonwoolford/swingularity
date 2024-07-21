using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "dialogueText", menuName = "dialogueText")]
public class DialogueText : ScriptableObject
{
    public string speakerName;
    [TextArea(5, 10)]
    public string[] paragraphs;
    [Tooltip("Name will be revealed when character is @")]
    public bool revealNameLater;
    public string hiddenSpeakerName;
}

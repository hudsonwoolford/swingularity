using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class EventTalkableNPC : NPC, ITalkable
{
    [System.Serializable]
    public enum DialogueEvents
    { 
        LevelGenerated,
        OnShoot,
        OnHole
    }

    [System.Serializable]
    public class EventTriggeredDialogue //class to avoid copying when setting value, we want it to be a reference instead
    {
        public string name;
        public DialogueEvents dialogueEvent;
        [Min(1)]
        public int numCallsRequired = 1;
        public bool resubscribeAfterCall = false;
        public float timeDelay = 0;
        public string[] allowedScenes;
        public DialogueText dialogueText;
        [HideInInspector] public int numCalls = 0;

        public override string ToString()
        {
            return string.Format("Dialogue event: {0} | Required calls: {1} | Time delay {2} | Allowed scenes {3} | Dialogue text {4} | (Hidden variable) Number of calls {5}", dialogueEvent, numCallsRequired, timeDelay, allowedScenes, dialogueText, numCalls); ;
        }
    }
    [SerializeField]protected EventTriggeredDialogue[] eventTriggeredDialogues;
    private Dictionary<EventTriggeredDialogue, Delegate> eventDelegates = new Dictionary<EventTriggeredDialogue, Delegate>();
    protected int currentDialogue;

    public virtual void Talk(DialogueText dialogueText)
    {
        //talk logic here
    }
    public override void Interact()
    {
        Debug.Log("Talk with events");
    }
    #region Delegate Logic
    IEnumerator DelegateCalled(EventTriggeredDialogue etd)
    {
        etd.numCalls++;
        Debug.Log(etd.numCalls);
        if (etd.numCalls<etd.numCallsRequired) { yield break; }
        yield return new WaitForSeconds(etd.timeDelay);
        if(etd.dialogueText == null) { Debug.LogWarning("Dialogue text is invalid. Check inspector?"); yield break; }
        currentDialogue = Array.IndexOf(eventTriggeredDialogues, etd);
        if(!etd.resubscribeAfterCall) { UnsubscribeDelegate(etd); }
        else { etd.numCalls = 0; }
        Talk(etd.dialogueText);
    }

    void SubscribeDelegate(EventTriggeredDialogue etd)
    {
        Delegate act = null;
        switch (etd.dialogueEvent)
        {
            case DialogueEvents.LevelGenerated:
                act = (LevelGenerator.LevelGenerated)(() => StartCoroutine(DelegateCalled(etd)));
                eventDelegates[etd] = act;
                LevelGenerator.levelGenerated -= (LevelGenerator.LevelGenerated)act; //for safety
                LevelGenerator.levelGenerated += (LevelGenerator.LevelGenerated)act;
                break;
            case DialogueEvents.OnShoot:
                act = (ShootBall.OnShoot)(() => StartCoroutine(DelegateCalled(etd)));
                eventDelegates[etd] = act;
                ShootBall.onShoot -= (ShootBall.OnShoot)act; //for safety
                ShootBall.onShoot += (ShootBall.OnShoot)act;
                break;
            case DialogueEvents.OnHole:
                act = (ShootBall.OnHole)(() => StartCoroutine(DelegateCalled(etd)));
                eventDelegates[etd] = act;
                ShootBall.onHole -= (ShootBall.OnHole)act; //for safety
                ShootBall.onHole += (ShootBall.OnHole)act;
                break;
            default:
                break;
        }
    }
    void UnsubscribeDelegate(EventTriggeredDialogue etd)
    {
        Delegate act = eventDelegates[etd];
        switch (etd.dialogueEvent)
        {
            case DialogueEvents.LevelGenerated:
                LevelGenerator.levelGenerated -= (LevelGenerator.LevelGenerated)act;
                break;
            case DialogueEvents.OnShoot:
                ShootBall.onShoot -= (ShootBall.OnShoot)act;
                break;
            case DialogueEvents.OnHole:
                ShootBall.onHole -= (ShootBall.OnHole)act;
                break;
            default:
                break;
        }
    }
    protected virtual void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        if(eventTriggeredDialogues == null) { return; }
        foreach (var kvp in eventDelegates)
        {
            UnsubscribeDelegate(kvp.Key);
        }
        eventDelegates.Clear();
        for (int i = 0; i < eventTriggeredDialogues.Length; i++)
        {
            for (int x = 0; x < eventTriggeredDialogues[i].allowedScenes.Length; x++)
            {
                if (scene.name == eventTriggeredDialogues[i].allowedScenes[x])
                {
                    SubscribeDelegate(eventTriggeredDialogues[i]);
                }
            }
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;

        foreach (var kvp in eventDelegates)
        {
            UnsubscribeDelegate(kvp.Key);
        }
        eventDelegates.Clear();
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Experimental.GlobalIllumination;

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


    private void Awake()
    {
        if (Instance == null)
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
        base.Update();
        if (!SettingsManager.userSettings.dialogue || (GameManager.LevelManager.isReset && SceneManager.GetActiveScene().name != "Mythical_4"))
        {
            return;
        }
        if (dialogueShown && !dialogueController.isConversation && !dialogueQueued)
        {
            StartCoroutine(HideDialogue());
        }
        if (dialogueController.isConversation && LevelManager.ballInput)
        {
            GameManager.LevelManager.EnableBallInput(false);
        }
        hologramMaterial.SetFloat("_UnscaledTime", Time.unscaledTime);
        if (dialogueQueued) { dialogueShown |= dialogueQueued; }
        if (Input.GetKeyDown(KeyCode.Escape) && dialogueShown && !dialogueQueued)
        {
            dialogueController.EndConversation();
            StartCoroutine(HideDialogue());
        }

        if(dialogueShown)
        {
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
        tgrAnimator.SetBool("Speaking", true);
        dialogueController.StartConversation(dialogueText); //sets text

        dialogueController.TogglePauseDialogue();
        float startTime = Time.unscaledTime;
        float length = 0.5f;
        while (Time.unscaledTime - startTime < length)
        {
            transform.localScale = Vector3.one * Camera.main.orthographicSize / 8;
            pointLight.range = 10 * Camera.main.orthographicSize / 8;
            transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z);
            yield return null;
        }
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z);
        transform.localScale = Vector3.one * Camera.main.orthographicSize / 8;
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
        tgrAnimator.SetBool("Speaking", false);

        //stop dialogue
        dialogueController.DialogueCloseSounds();
        dialogueController.TogglePauseDialogue();

        float startTime = Time.time;
        float length = 0.5f;
        while (Time.time - startTime < length)
        {
            transform.localScale = Vector3.one * Camera.main.orthographicSize/8;
            pointLight.range = 10 * Camera.main.orthographicSize / 8;
            transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z);
            yield return null;
        }
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, transform.position.z);
        transform.localScale = Vector3.one * Camera.main.orthographicSize / 8;

        dialogueController.TogglePauseDialogue();

        isTransitioning = false;
    }
    protected override void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        base.OnSceneLoad(scene, loadSceneMode);
        dialogueCanvas.worldCamera = Camera.main;
        if (dialogueShown)
        {
            dialogueController.EndConversation();
            StopAllCoroutines();
            tgrAnimator.Play("DisableDialogue");
        }
    }
}

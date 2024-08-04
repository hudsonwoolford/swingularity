using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneHandling : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI percentageProgress;
    [SerializeField] private TextMeshProUGUI loadingDescription;
    private Animator animator;
    private Canvas loaderCanvas;
    private bool levelGenerated;

    private const float MAX_PROGRESS = 0.98f;
    public static bool loadingScreen { get; private set; }
    MaterialPropertyBlock mpb;

    // Start is called before the first frame update
    void Start()
    {
        if(loaderCanvas == null) { loaderCanvas = GetComponent<Canvas>(); }
        if(animator == null) { animator = GetComponent<Animator>(); }
        animator.Play("DisableMenu");
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void LoadScene(string sceneName, bool waitForLevelGeneration)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName, waitForLevelGeneration));
    }
    IEnumerator LoadSceneCoroutine(string sceneName, bool waitForLevelGeneration)
    {
        loadingScreen = true;
        GameManager.LevelManager.EnableBallInput(false);
        float randMult = UnityEngine.Random.Range(0.75f,1f);
        progressBar.value = 0f;
        loadingDescription.text = "Loading Scene...";
        percentageProgress.text = Mathf.RoundToInt(progressBar.value * 100).ToString();

        levelGenerated = false;

        animator.Play("ShowMenu");
        yield return new WaitForSecondsRealtime(animator.GetCurrentAnimatorStateInfo(0).length);

        var sceneLoading = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        sceneLoading.allowSceneActivation = false;

        float progressModifier = waitForLevelGeneration ? 0.5f : 1f;
        //increment load bar
        do
        {
            progressBar.value = sceneLoading.progress * progressModifier * MAX_PROGRESS * randMult;
            percentageProgress.text = Mathf.RoundToInt(progressBar.value * 100).ToString();
            yield return null;
        } while (sceneLoading.progress < 0.9f);

        sceneLoading.allowSceneActivation = true;

        //level generation wait
        if(waitForLevelGeneration)
        {
            loadingDescription.text = GameManager.LevelManager.procedural ? "Generating level..." : "Loading level...";
            while (!levelGenerated)
            {
                if (LevelGenerator.Instance != null)
                {
                    if (LevelGenerator.Instance.generationProgress != 1) //for some reason it is 1, i reset it but still happens
                    {
                        progressBar.value = progressModifier * randMult + LevelGenerator.Instance.generationProgress / 2 * MAX_PROGRESS ;
                    }
                }
                if (ManualLevelGenerator.Instance != null)
                {
                    if (ManualLevelGenerator.Instance.generationProgress != 1) //for some reason it is 1, i reset it but still happens
                    {
                        progressBar.value = progressModifier * randMult + ManualLevelGenerator.Instance.generationProgress / 2 * MAX_PROGRESS;
                    }
                }
                percentageProgress.text = Mathf.RoundToInt(progressBar.value * 100).ToString();
                yield return null;
            }
        }

        animator.Play("HideMenu");
        GameManager.LevelManager.EnableBallInput(true);
        loadingScreen = false;
    }
    void OnLevelGenerated() => levelGenerated = true;
    private void OnEnable()
    {
        LevelGenerator.levelGenerated += () => OnLevelGenerated();

    }
    private void OnDisable()
    {
        LevelGenerator.levelGenerated -= () => OnLevelGenerated();
    }
}

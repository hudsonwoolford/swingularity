using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;

public class SceneHandling : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    private Animator animator;
    private Canvas loaderCanvas;
    private float fillTarget;
    private bool levelGenerated;
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
        progressBar.fillAmount = Mathf.MoveTowards(progressBar.fillAmount, fillTarget, Time.deltaTime);
    }
    public async void LoadScene(string sceneName, bool waitForLevelGeneration)
    {
        fillTarget = 0;
        progressBar.fillAmount = 0f;

        levelGenerated = false;

        animator.Play("ShowMenu");  
        await Task.Delay(1000);

        var scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;


        //increment load bar
        do
        {
            fillTarget = scene.progress;
        } while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;
        //level generation wait
        while(waitForLevelGeneration && !levelGenerated)
        {
            await Task.Yield();
        }
        await Task.Delay(1000);

        animator.Play("HideMenu");
    }
    void OnLevelGenerated()
    {
        levelGenerated = true;
    }
    private void OnEnable()
    {
        LevelGenerator.levelGenerated += () => OnLevelGenerated();

    }
    private void OnDisable()
    {
        LevelGenerator.levelGenerated -= () => OnLevelGenerated();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

[ExecuteInEditMode()]
public class GalaxySummary : MonoBehaviour
{
    [SerializeField] GameObject galaxySummary;
    [SerializeField] bool[] collectedStars;
    [SerializeField] Image[] starImgs;
    [SerializeField] GameObject lockImage, stars;
    [SerializeField] AudioObj lockErrorAudio;
    [Header("Universe Summary")]
    [SerializeField] GameObject universeSummary;
    [SerializeField] TextMeshProUGUI universeNameTxt;
    [SerializeField] TextMeshProUGUI universeSeedTxt;
    [SerializeField] TextMeshProUGUI universeTimeTxt;
    [SerializeField] TextMeshProUGUI universeProgressTxt;
    [SerializeField] Slider progressSlider;
    [SerializeField] Image progressSliderFill;
    [SerializeField] TextMeshProUGUI universeStarsTxt;
    [SerializeField] TextMeshProUGUI universeSupernovaeTxt;
    [SerializeField] TextMeshProUGUI universeDifficultyMult;
    [SerializeField] Gradient colorGrad;

    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();    
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnValidate()
    {
        UpdateStars();
    }
    public void Shake(){
        GameManager.SFXManager.PlaySFX(lockErrorAudio);
        animator.Play("ShakeLock");
    }
    public void DisableSummary()
    {
        animator.Play("DisableMenu");
    }
    public void InitSummary(bool[] collectedStars, int lockStars)
    {
        galaxySummary.SetActive(true);
        universeSummary.SetActive(false);
        animator = GetComponent<Animator>();
        GetComponent<Canvas>().worldCamera = Camera.main;
        if (GameManager.LevelManager.currentUniverse.numStars < lockStars){
            lockImage.SetActive(true);
            lockImage.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = (lockStars).ToString();
            stars.SetActive(false);
        }
        else{
            stars.SetActive(true);
            lockImage.SetActive(false);
            this.collectedStars = collectedStars;
            UpdateStars();
        }
    }
    public void InitUniverseSummary()
    {
        galaxySummary.SetActive(false);
        universeSummary.SetActive(true);
        LevelManager.UniverseParams uni = GameManager.LevelManager.currentUniverse;
        universeNameTxt.text = uni.name;
        float numPossibleStars = uni.numGalaxies * 5f == 0 ? 1 : uni.numGalaxies * 5f;
        float progress = (float)uni.numStars / (numPossibleStars);
        universeProgressTxt.text = "Progress: " + SetColor(Mathf.RoundToInt(progress*100).ToString(), colorGrad.Evaluate(1 - progress));
        progressSlider.value = progress;
        progressSliderFill.color = colorGrad.Evaluate(1 - progress);
        universeTimeTxt.text = "Time: " + Mathf.RoundToInt(Time.timeSinceLevelLoad).ToString();
        universeSeedTxt.text = !uni.proceduralUniverse ? "Seed: " + SetColor("Campaign", colorGrad.Evaluate(0)) :  "Seed: " + uni.seed.ToString();
        universeStarsTxt.text = uni.numStars.ToString();
        universeSupernovaeTxt.text = (uni.numStars * uni.relativeDifficulty).ToString();
        universeDifficultyMult.text = !uni.proceduralUniverse ? "x" + SetColor("5", colorGrad.Evaluate(0.5f)) : "x" + SetColor(uni.relativeDifficulty.ToString(), colorGrad.Evaluate(uni.relativeDifficulty/10));
    }
    public void EnableSummary(bool enable)
    {
        if(enable)
        {
            animator.Play("ShowMenu");
        }
        else
        {
            animator.Play("HideMenu");
        }
    }
    void UpdateStars()
    {
        for (int i = 0; i < starImgs.Length; i++)
        {
            if (i >= collectedStars.Length) { starImgs[i].enabled = false; continue; }
            starImgs[i].enabled = collectedStars[i];
        }
    }
    string SetColor(string text, Color color)
    {
        // Convert the color to a hexadecimal string
        string hexColor = ColorUtility.ToHtmlStringRGBA(color);

        // Return the text with rich text color tags
        return $"<color=#{hexColor}>{text}</color>";
    }
}

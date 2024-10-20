using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DemoCongrats : MonoBehaviour
{
    [SerializeField] Button storePageBtn;
    [SerializeField] Button escapeBtn;
    [SerializeField] TextMeshProUGUI shotsTxt;
    [SerializeField] TextMeshProUGUI starsTxt;
    [SerializeField] Gradient colorGrad;
    Animator menuAnim;

    private void Awake()
    {
        menuAnim = GetComponent<Animator>();
    }
    private void Start()
    {
        storePageBtn.onClick.AddListener(() => StorePageButton());
        escapeBtn.onClick.AddListener(() => EscapeButton());
        menuAnim.Play("DisableMenu");
    }
    public void ShowMenu()
    {
        menuAnim.Play("ShowMenu");

        shotsTxt.text = "Shots: " + SaveLoadManager.currentUniverseSave.totalShots;

        LevelManager.UniverseParams uni = GameManager.LevelManager.currentUniverse;
        float numPossibleStars = uni.numGalaxies * 5f == 0 ? 1 : uni.numGalaxies * 5f;
        float progress = (float)uni.numStars / (numPossibleStars);
        starsTxt.text = "Stars: " +  SetColor(uni.numStars.ToString() + " : " + numPossibleStars.ToString(), colorGrad.Evaluate(1-progress));
    }

    void StorePageButton()
    {
        Application.OpenURL("https://store.steampowered.com/app/3272450/Swingularity/");
    }
    void EscapeButton()
    {
        menuAnim.Play("HideMenu");
    }

    string SetColor(string text, Color color)
    {
        // Convert the color to a hexadecimal string
        string hexColor = ColorUtility.ToHtmlStringRGBA(color);

        // Return the text with rich text color tags
        return $"<color=#{hexColor}>{text}</color>";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveElement : MonoBehaviour
{
    public LevelManager.UniverseParams uniParams { get; private set; }
    [Header("Values")]
    [SerializeField] Color difficultyCol;
    [SerializeField] Gradient progressGrad;
    [SerializeField] Color sizeCol;

    [Header("References")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI difficultyText;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] TextMeshProUGUI sizeText;
    [SerializeField] UniverseGenerator universeGenerator;
    [SerializeField] Button deleteButton;
    // Start is called before the first frame update
    void Start()
    {
        SetElementValues();
        
    }
    void OnValidate()
    {
        SetElementValues();
    }
    void SetElementValues()
    {
        nameText.text = uniParams.name;
        timeText.text = "time";
        difficultyText.text = uniParams.relativeDifficulty.ToString();
        int numGalaxies = uniParams.numGalaxies == 0 ? 10 : uniParams.numGalaxies; //avoid division by zero
        float progress = (float)(uniParams.completedGalaxies / numGalaxies);
        progressText.text = Mathf.RoundToInt(progress* 100).ToString() + "%";
        sizeText.text = uniParams.size;

        difficultyText.color = difficultyCol;
        progressText.color = progressGrad.Evaluate(progress);
        sizeText.color = sizeCol;

        universeGenerator.ChangeUniverse(uniParams.seed);
    }
    public Button InitValues(LevelManager.UniverseParams universeParams, Color difficultyCol, Color sizeCol)
    {
        this.uniParams = universeParams;
        this.difficultyCol = difficultyCol;
        this.sizeCol = sizeCol;
        return deleteButton;
    }
}

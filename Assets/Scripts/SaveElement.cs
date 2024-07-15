using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

[ExecuteInEditMode()]
public class SaveElement : MonoBehaviour
{
    public LevelManager.UniverseParams uniParams { get; private set; }
    [Header("Values")]
    [SerializeField] Color difficultyCol;
    [SerializeField] Gradient progressGrad;
    [SerializeField] Color sizeCol;
    [SerializeField] string lastAccessTime;
    [SerializeField] bool procedural;

    [Header("References")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI progressText;
    [SerializeField] TextMeshProUGUI progressPercentText;
    [Header("Procedural")]
    [SerializeField] TMP_InputField seedText;
    [SerializeField] GameObject proceduralInfo;
    [SerializeField] TextMeshProUGUI difficultyText;
    [SerializeField] TextMeshProUGUI sizeText;
    [Header("Campaign")]
    [SerializeField] GameObject mainCampaignInfo;
    //[SerializeField] TextMeshProUGUI progressText;
    //[SerializeField] TextMeshProUGUI progressPercentText;
    [Header("Miscellaneous")]
    [SerializeField] UniverseGenerator universeGenerator;
    [SerializeField] GameObject ballImaging;
    [SerializeField] Image ballImage;
    [SerializeField] Camera ballCam;
    [SerializeField] Button deleteButton;
    RenderTexture ballTexture;
    Texture2D ballTexture2D;
    // Start is called before the first frame update
    void Start()
    {
        CreateBallImage();
        SetElementValues();
    }
    void OnValidate()
    {
        SetElementValues();
    }
    void SetElementValues()
    {
        nameText.text = uniParams.name;
        timeText.text = lastAccessTime;
        if(procedural)
        {
            proceduralInfo.SetActive(true);
            mainCampaignInfo.SetActive(false);
            difficultyText.text = uniParams.relativeDifficulty.ToString();
            seedText.text = "Seed: " + uniParams.seed.ToString();
            sizeText.text = uniParams.size;
            difficultyText.color = difficultyCol;
            sizeText.color = sizeCol;

            progressText.transform.localPosition = new Vector3(progressText.transform.localPosition.x, 0, progressText.transform.localPosition.z);
        }
        else
        {
            proceduralInfo.SetActive(false);
            mainCampaignInfo.SetActive(true);
            progressText.transform.localPosition = new Vector3(progressText.transform.localPosition.x, 15, progressText.transform.localPosition.z);
        }

        //progress
        int numGalaxies = uniParams.numGalaxies == 0 ? 1 : uniParams.numGalaxies; //avoid division by zero
        float progress = (float)uniParams.numStars / ((float)numGalaxies * 5);
        progressText.text = Mathf.RoundToInt((progress*100)).ToString();
        progressText.color = progressGrad.Evaluate(progress);
        progressPercentText.color = progressText.color;

    }
    void CreateBallImage()
    {
        if (!Application.isPlaying) { return; }

        universeGenerator.ChangeUniverse(uniParams.seed);

        if (ballTexture == null)
        {
            ballTexture = new RenderTexture(512, 512, 24); // 24-bit depth buffer for better precision
            ballTexture.name = "Ball Image";
        }

        ballCam.targetTexture = ballTexture;
        ballCam.Render(); // Force render

        Sprite newSprite = ConvertRenderTextureToSprite(ballTexture);
        ballImage.sprite = newSprite;

        Destroy(ballImaging); // Ensure ballImaging is valid before destroying
    }

    Sprite ConvertRenderTextureToSprite(RenderTexture renderTexture)
    {
        RenderTexture prevActive = RenderTexture.active;
        RenderTexture.active = renderTexture;

        ballTexture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        ballTexture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        ballTexture2D.Apply();

        RenderTexture.active = prevActive; // Reset immediately after Apply()

        Sprite sprite = Sprite.Create(ballTexture2D, new Rect(0, 0, ballTexture2D.width, ballTexture2D.height), Vector2.one * 0.5f);
        sprite.name = "BallSprite";

        return sprite;
    }
    public Button InitValues(LevelManager.UniverseParams universeParams, Color difficultyCol, Color sizeCol, string lastAccessTime, bool procedural)
    {
        this.lastAccessTime = lastAccessTime;
        this.uniParams = universeParams;
        this.difficultyCol = difficultyCol;
        this.sizeCol = sizeCol;
        this.procedural = procedural;
        return deleteButton;
    }
}

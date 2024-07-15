using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.IO;
using System.Linq;
using System;


public class TitleScreenButtons : MonoBehaviour
{
    [SerializeField] Animator cameraAnim, menuAnim, ngMenuAnim;
    [Header("Main Screen UI")]
    [SerializeField] Button continueBtn;
    [SerializeField] Button newGameBtn;
    [SerializeField] Button loadGameBtn;
    [SerializeField] Button settingsBtn;
    [SerializeField] Button quitBtn;
    [Header("Save Load UI")]
    [SerializeField] Scrollbar slScrollBar;
    [SerializeField] Button slBackBtn;
    [SerializeField] Button slCreateBtn;
    [SerializeField] Button slLoadBtn;
    [SerializeField] float elementHeightJump;
    [SerializeField] Transform saveElementParent;
    [SerializeField] GameObject saveElementObj;
    [Header("Options UI")]
    [SerializeField] Button campaignBtn;
    [SerializeField] Button infiniteBtn;
    [SerializeField] Button optionsBackBtn;
    [Header("Campaign Creation UI")]
    [SerializeField] TMP_InputField ccNameFld;
    [SerializeField] Button ccCreateBtn;
    [Header("New Game UI")]
    [SerializeField] Button ngBackBtn;
    [SerializeField] Button ngCreateBtn;
    [SerializeField] Button[] ngSizeBtns;
    [SerializeField] TMP_InputField ngNameFld;
    [SerializeField] TMP_InputField ngSeedFld;
    [SerializeField] Slider ngDifficultySldr;
    [SerializeField] TextMeshProUGUI difficultyNumber;
    [SerializeField] Gradient difficultyColorGrad;
    [Header("Miscellaneous Params")]
    [SerializeField] LevelManager.UniverseParams campaignUniverseParams;
    [SerializeField] LevelGenStaticReferences levelStatic;
    [SerializeField] Vector2Int[] minMaxGalaxies;
    [SerializeField] string proceduralGalaxiesScene;
    [SerializeField] Material skybox;
    [SerializeField] float speed;
    Vector3 saveElementParentOrigin;
    float rotation = 0.02f;
    bool ngMenu;
    bool newGameNGMenu;
    NGMenuState currentNGMenuState;
    bool settingsMenu;
    int currentUniverseSize = 1; //0, 1, 2, for small, medium, large
    EventSystem eventSystem;
    GameObject lastSelectedGameObject, currentSelectedGameObject_Recent;

    private enum NGMenuState
    {
        SaveLoadMenu,
        OptionsMenu,
        CreationMenu,
        CampaignCreationMenu
    }

    void Start()
    {
        saveElementParentOrigin = saveElementParent.localPosition;
        eventSystem = EventSystem.current;
        SetGalaxySize(1);
        UpdateDifficulty();
        ngMenu = false;
        skybox.SetFloat("_Rotation", rotation);
        //normal ui
        newGameBtn.onClick.AddListener(() => NewGame());
        loadGameBtn.onClick.AddListener(() => LoadGame());
        settingsBtn.onClick.AddListener(() => OpenSettings());
        quitBtn.onClick.AddListener(() => QuitGame());
        //options ui
        optionsBackBtn.onClick.AddListener(() => OptionBackBtn());
        campaignBtn.onClick.AddListener(() => SwitchCurrentNGMenu(NGMenuState.CampaignCreationMenu));
        infiniteBtn.onClick.AddListener(() => SwitchCurrentNGMenu(NGMenuState.CreationMenu));
        //save load ui
        slBackBtn.onClick.AddListener(() => StartCoroutine(EnableNewGameMenu(false)));
        slCreateBtn.onClick.AddListener(() => SwitchCurrentNGMenu(NGMenuState.OptionsMenu));
        slLoadBtn.onClick.AddListener(() => LoadUniverse());
        slScrollBar.onValueChanged.AddListener((v) => OnSaveLoadScrollbar(v));
        //campaign creation ui
        ccCreateBtn.onClick.AddListener(() => CreateNewManualUniverse());
        //new game ui
        ngBackBtn.onClick.AddListener(() => SwitchCurrentNGMenu(NGMenuState.OptionsMenu));
        ngCreateBtn.onClick.AddListener(() => CreateNewProceduralUniverse());
        ngSizeBtns[0].onClick.AddListener(() => SetGalaxySize(0));
        ngSizeBtns[1].onClick.AddListener(() => SetGalaxySize(1));
        ngSizeBtns[2].onClick.AddListener(() => SetGalaxySize(3));

        if (SaveLoadManager.saves.Length > 0) {
            continueBtn.transform.GetChild(0).gameObject.SetActive(true);
            continueBtn.transform.GetChild(1).gameObject.SetActive(false);
            continueBtn.transform.GetChild(2).gameObject.SetActive(true);
            continueBtn.onClick.AddListener(() => Continue());
        }
        else {
            continueBtn.transform.GetChild(0).gameObject.SetActive(false);
            continueBtn.transform.GetChild(1).gameObject.SetActive(true);
            continueBtn.transform.GetChild(2).gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && ngMenu)
        {
            StartCoroutine(EnableNewGameMenu(false));
        }
        if (Input.GetKeyDown(KeyCode.Escape) && settingsMenu)
        {
            DisableSettings(false);
        }
        if (ngMenu && currentNGMenuState == NGMenuState.SaveLoadMenu)
        {
            slScrollBar.value += -Input.mouseScrollDelta.y / 15;
            slScrollBar.value = Mathf.Clamp(slScrollBar.value, 0f, 1f);
        }
        if (ngMenu) {
            rotation += Time.deltaTime * speed;
            if (rotation > 360) {
                rotation = 0;
            }
            skybox.SetFloat("_Rotation", rotation);
        }
        GetLastGameObjectSelected();
    }


    void GetLastGameObjectSelected() {
        if (eventSystem == null) { eventSystem = EventSystem.current; }
        if (eventSystem.currentSelectedGameObject != currentSelectedGameObject_Recent) {

            lastSelectedGameObject = currentSelectedGameObject_Recent;

            currentSelectedGameObject_Recent = eventSystem.currentSelectedGameObject;
        }
    }
    #region Main Screen UI
    void Continue() {

        SceneManager.LoadScene("Galaxies");

    }
    void NewGame() {
        StartCoroutine(EnableNewGameMenu(true));
        ngMenuAnim.Play("HideCreationMenu");
        ngMenuAnim.Play("ShowOptionMenu");
        ngMenuAnim.Play("HideSaveLoadMenu");
        ngMenuAnim.Play("HideCampaignCreationMenu");
        menuAnim.Play("HideMenu");
        currentNGMenuState = NGMenuState.OptionsMenu;
        newGameNGMenu = true;
    }
    void LoadGame() {
        StartCoroutine(EnableNewGameMenu(true));
        ngMenuAnim.Play("HideCreationMenu");
        ngMenuAnim.Play("HideOptionMenu");
        ngMenuAnim.Play("ShowSaveLoadMenu");
        ngMenuAnim.Play("HideCampaignCreationMenu");
        menuAnim.Play("HideMenu");
        currentNGMenuState = NGMenuState.SaveLoadMenu;
        newGameNGMenu = false;
    }
    void OpenSettings() {
        StartCoroutine(SettingsAnimDelay(true));
        menuAnim.Play("HideMenu");
    }
    void DisableSettings(bool enable) {
        if (enable) { return; }
        settingsMenu = false;
        StartCoroutine(SettingsAnimDelay(false));

        cameraAnim.Play("SettingsToMenu");
    }
    void QuitGame() {
        Application.Quit();
    }

    IEnumerator SettingsAnimDelay(bool enable) {
        if (enable)
        {
            menuAnim.Play("HideMenu");
            cameraAnim.Play("MenuToSettings");
        }
        else
        {
            cameraAnim.Play("SettingsToMenu");
        }

        yield return new WaitForSeconds(cameraAnim.GetCurrentAnimatorStateInfo(0).length);
        settingsMenu = enable;
        if (enable) {
            SettingsManager.enableSettingsMenu?.Invoke(true);
        }
        else
        {
            menuAnim.Play("ShowMenu");
        }

    }

    #endregion
    #region Save Load UI
    void OnSaveLoadScrollbar(float value)
    {
        float maxValue = -(SaveLoadManager.saves.Length - 1) * elementHeightJump;
        saveElementParent.localPosition = Vector3.Lerp(saveElementParentOrigin, saveElementParentOrigin + new Vector3(0, maxValue, 0), value);
    }
    void DeleteSave(SaveElement se)
    {
        GameManager.SaveLoadManager.DeleteSave(se.uniParams.name + ".txt");
        Destroy(se.gameObject);
        PopulateSaves();
    }
    void LoadUniverse()
    {
        SaveElement se;
        if (lastSelectedGameObject == null || !lastSelectedGameObject.TryGetComponent<SaveElement>(out se)) { return; }
        LevelManager.notifyProcedural?.Invoke(se.uniParams.proceduralUniverse);
        GameManager.SaveLoadManager.Load(se.uniParams.name + ".txt");
        SceneManager.LoadScene(proceduralGalaxiesScene);
    }
    void PopulateSaves()
    {
        if (SaveLoadManager.saves == null || SaveLoadManager.saves.Length == 0) { return; }
        FileInfo[] saveInfos = SaveLoadManager.saves;

        SaveElement[] allElements = saveElementParent.GetComponentsInChildren<SaveElement>(true);
        Array.Sort(saveInfos, (s1, s2) => s2.LastAccessTime.CompareTo(s1.LastAccessTime));
        for (int i = 0; i < allElements.Length; i++)
        {
            Destroy(allElements[i].gameObject);
        }
        for (int i = 0; i < saveInfos.Length; i++)
        {
            RectTransform tr = Instantiate(saveElementObj, saveElementParent).GetComponent<RectTransform>();
            tr.localPosition += new Vector3(0, elementHeightJump * i, 0);

            //Temporary bc no save load functionality
            LevelManager.UniverseParams uni = GameManager.SaveLoadManager.RetrieveSave(saveInfos[i].Name).universeParams;
            if (uni == null) { continue; }
            Color sizeColor = Color.white;
            if (uni.size == "Small")
            {
                sizeColor = difficultyColorGrad.Evaluate(0f);
            }
            else if (uni.size == "Medium")
            {
                sizeColor = difficultyColorGrad.Evaluate(0.5f);
            }
            else if (uni.size == "Large")
            {
                sizeColor = difficultyColorGrad.Evaluate(1f);
            }

            //script setting
            SaveElement seScript = tr.GetComponent<SaveElement>();
            seScript.InitValues(uni, difficultyColorGrad.Evaluate((float)uni.relativeDifficulty / 10), sizeColor, saveInfos[i].LastAccessTime.ToString("dd.MM.yy hh:mm"), uni.proceduralUniverse).onClick.AddListener(() => DeleteSave(seScript));
        }
    }
    #endregion
    #region New Game UI
    public void UpdateDifficulty()
    {
        if (ngDifficultySldr.value <= 0) {
            ngDifficultySldr.value = 1;
        }
        difficultyNumber.text = ngDifficultySldr.value.ToString();
        ngDifficultySldr.targetGraphic.color = difficultyColorGrad.Evaluate(ngDifficultySldr.value / 10);
        difficultyNumber.color = difficultyColorGrad.Evaluate(ngDifficultySldr.value / 10);
    }
    public void RandomizeSeed()
    {
        UnityEngine.Random.InitState(Mathf.RoundToInt(System.DateTime.Now.Millisecond * System.DateTime.Now.Second * Time.time));
        ngSeedFld.text = UnityEngine.Random.Range(-2147483647, 2147483647).ToString();
    }
    void OptionBackBtn()
    {
        if (newGameNGMenu) { StartCoroutine(EnableNewGameMenu(false)); } else { SwitchCurrentNGMenu(NGMenuState.SaveLoadMenu); }
    }
    /// <param name="size">0, 1 or 2 | small, medium, or large</param>
    void SetGalaxySize(int size)
    {
        size = Mathf.Clamp(size, 0, 2);
        currentUniverseSize = size;
        for (int i = 0; i<ngSizeBtns.Length; i++)
        {
            bool enableText = i==size ? true : false;
            if(enableText) {ngSizeBtns[i].GetComponent<Animator>().Play("Select");}
            else {ngSizeBtns[i].GetComponent<Animator>().Play("Unselect");}
            
        }
    }
    void CreateNewManualUniverse()
    {
        LevelManager.UniverseParams up = campaignUniverseParams;
        if(up.name == string.Empty) { up.name = "MyUniverse"; }
        up.proceduralUniverse = false;
        up.name = EnsureUniqueFileName(SaveLoadManager.saves, ccNameFld.text + ".txt");
        up.name = up.name.Substring(0, up.name.LastIndexOf(".txt"));
        GameManager.SaveLoadManager.CreateSave(up.name + ".txt", new SaveLoadManager.UniverseSave { universeParams = up });
        LevelManager.notifyProcedural?.Invoke(false);
        SceneManager.LoadScene(proceduralGalaxiesScene);
    }
    void CreateNewProceduralUniverse() //create button in new game menu
    {
        LevelManager.UniverseParams up = SynthesizeUniverseParams();
        up.name = EnsureUniqueFileName(SaveLoadManager.saves, up.name + ".txt");
        up.name = up.name.Substring(0,up.name.LastIndexOf(".txt"));
        GameManager.SaveLoadManager.CreateSave(up.name + ".txt", new SaveLoadManager.UniverseSave { universeParams = up });
        LevelManager.notifyProcedural?.Invoke(true);
        SceneManager.LoadScene(proceduralGalaxiesScene);
    }
    string EnsureUniqueFileName(FileInfo[] existingFiles, string newFileName)
    {
        // Extract existing file names
        var existingFileNames = existingFiles.Select(file => file.Name).ToHashSet();

        // Get file name without extension and its extension
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(newFileName);
        string extension = Path.GetExtension(newFileName);

        // Check if the new file name already exists
        if (!existingFileNames.Contains(newFileName))
        {
            return newFileName;
        }

        // If it exists, append a number to the new file name
        int counter = 1;
        string uniqueFileName;
        do
        {
            uniqueFileName = $"{fileNameWithoutExtension}({counter}){extension}";
            counter++;
        } while (existingFileNames.Contains(uniqueFileName));

        return uniqueFileName;
    }
    void SwitchCurrentNGMenu(NGMenuState targetState)
    {
        switch (targetState)
        {
            case NGMenuState.SaveLoadMenu:
                if(currentNGMenuState == NGMenuState.OptionsMenu)
                {
                    ngMenuAnim.Play("HideOptionMenu");
                }
                else if(currentNGMenuState == NGMenuState.CreationMenu)
                {
                    ngMenuAnim.Play("HideCreationMenu");
                }
                ngMenuAnim.Play("ShowSaveLoadMenu");
                break;
            case NGMenuState.OptionsMenu:
                if(currentNGMenuState == NGMenuState.CreationMenu)
                {
                    ngMenuAnim.Play("HideCreationMenu");
                }
                if (currentNGMenuState == NGMenuState.SaveLoadMenu)
                {
                    ngMenuAnim.Play("HideSaveLoadMenu");
                }
                ngMenuAnim.Play("ShowOptionMenu");
                break;
            case NGMenuState.CreationMenu:
                if (currentNGMenuState == NGMenuState.OptionsMenu)
                {
                    ngMenuAnim.Play("HideOptionMenu");
                }
                if (currentNGMenuState == NGMenuState.SaveLoadMenu)
                {
                    ngMenuAnim.Play("HideSaveLoadMenu");
                }
                ngMenuAnim.Play("ShowCreationMenu");
                break;
            case NGMenuState.CampaignCreationMenu:
                if(currentNGMenuState == NGMenuState.OptionsMenu)
                {
                    ngMenuAnim.Play("HideOptionMenu");
                }
                ngMenuAnim.Play("ShowCampaignCreationMenu");
                break;
            default:
                break;
        }
        currentNGMenuState = targetState;
    }
    IEnumerator EnableNewGameMenu(bool enable)
    {
        if(enable)
        {
            PopulateSaves();
            ngMenu = true;
            menuAnim.Play("HideMenu");
            cameraAnim.Play("MenuToNGMenu");
            ngMenuAnim.Play("ShowNGMenu");
        }
        else if (!enable)
        {
            ngMenu = false;
            ngMenuAnim.Play("HideNGMenu");
            cameraAnim.Play("NGMenuToMenu");
            menuAnim.Play("ShowMenu");
        }
        yield return null;
    }
    LevelManager.UniverseParams SynthesizeUniverseParams()
    {
        UnityEngine.Random.InitState(Mathf.RoundToInt(System.DateTime.Now.Millisecond * System.DateTime.Now.Second * Time.time));
        string seedInput = ngSeedFld.text == string.Empty ? UnityEngine.Random.Range(-2147483647, 2147483647).ToString() : ngSeedFld.text; //failsafe if no seed input - randomizes
        int seed = LevelSettingsManager.ConvertTextToSeed(seedInput); //seed field
        UnityEngine.Random.InitState(seed);
        int numGalaxies = UnityEngine.Random.Range(minMaxGalaxies[currentUniverseSize].x, minMaxGalaxies[currentUniverseSize].y); //random number of galaxies
        numGalaxies = (numGalaxies<=0) ? 1 : numGalaxies; //failsafe for zero galaxies
        string universeName = ngNameFld.text; //save name of universe from field
        universeName = universeName == string.Empty ? "MyUniverse" : universeName; //failsafe - needs to not be the same name as other saves - not implemented
        return new LevelManager.UniverseParams(universeName, seed, (int)ngDifficultySldr.value, ngSizeBtns[currentUniverseSize].gameObject.name, 0, numGalaxies, 0, true);
    }

    void OnEnable(){
        SettingsManager.enableSettingsMenu += DisableSettings;
    }

    void OnDisable(){
        SettingsManager.enableSettingsMenu -= DisableSettings;
    }
    #endregion
}

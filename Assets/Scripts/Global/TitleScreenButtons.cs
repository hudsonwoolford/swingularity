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
using static SaveLoadManager;


public class TitleScreenButtons : MonoBehaviour
{
    [SerializeField] Animator cameraAnim, menuAnim, ngMenuAnim, shopAnim;
    [Header("Main Screen UI")]
    [SerializeField] Button continueBtn;
    [SerializeField] Button myUniverseBtn;
    [SerializeField] Button newUniverseBtn;
    [SerializeField] Button customiseBtn;
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
    [SerializeField] CanvasGroup infiniteGroup;
    [SerializeField] Button optionsSelectBtn;
    [SerializeField] Button optionsBackBtn;
    [Header("Campaign Creation UI")]
    [SerializeField] TMP_InputField ccNameFld;
    [SerializeField] Button ccCreateBtn;
    [SerializeField] Button ccBackBtn;
    [SerializeField] UniverseButton[] uniButtons;
    [Header("New Game UI")]
    [SerializeField] Button ngBackBtn;
    [SerializeField] Button ngCreateBtn;
    [SerializeField] Button[] ngSizeBtns;
    [SerializeField] Image[] ngSizeHoverImgs;
    [SerializeField] TMP_InputField ngNameFld;
    [SerializeField] TMP_InputField ngSeedFld;
    [SerializeField] Slider ngDifficultySldr;
    [SerializeField] TextMeshProUGUI difficultyNumber;
    [SerializeField] Gradient difficultyColorGrad;
    [Header("Miscellaneous Params")]
    [SerializeField] ShopManager shopManager;
    [SerializeField] LevelManager.UniverseParams manualUniverseParams;
    [SerializeField] Dictionary <LevelManager.UniverseParams.UniverseType, int> manualDifficulties = new Dictionary<LevelManager.UniverseParams.UniverseType, int>();
    [SerializeField] LevelGenStaticReferences levelStatic;
    [SerializeField] Vector2Int[] minMaxGalaxies;
    [SerializeField] string proceduralGalaxiesScene;
    [SerializeField] float speed;
    Vector3 saveElementParentOrigin;
    float rotation = 0.02f;
    bool mainMenuShown = false;
    bool ngMenu;
    bool onlyNGMenu;
    NGMenuState currentNGMenuState;
    bool settingsMenu;
    int currentUniverseSize = 1; //0, 1, 2, for small, medium, large
    EventSystem eventSystem;
    GameObject lastSelectedGameObject, currentSelectedGameObject_Recent;
    LevelManager.UniverseParams.UniverseType currentUniverseType = LevelManager.UniverseParams.UniverseType.home;

    [Serializable]
    struct UniverseButton
    {
        public LevelManager.UniverseParams.UniverseType type;
        public Button button;
        public Image hoverImage;
    }

    private enum NGMenuState
    {
        SaveLoadMenu,
        OptionsMenu,
        CreationMenu,
        CampaignCreationMenu
    }
    private void Awake()
    {

    }
    void Start()
    {
        saveElementParentOrigin = saveElementParent.localPosition;
        eventSystem = EventSystem.current;
        SetGalaxySize(1);
        SetUniverseType(uniButtons[0]);
        UpdateDifficulty();


        if(Time.time == 0)
        {
            mainMenuShown = false;
        }
        else
        {
            menuAnim.Play("ContinueShowMenu", 0, 1);
            mainMenuShown = true;
        }
        ngMenu = false;

        //relative difficulties of manual universes
        manualDifficulties.Add(LevelManager.UniverseParams.UniverseType.home, 5);
        manualDifficulties.Add(LevelManager.UniverseParams.UniverseType.challenge, 9);
        manualDifficulties.Add(LevelManager.UniverseParams.UniverseType.parallel, 7);
        manualDifficulties.Add(LevelManager.UniverseParams.UniverseType.recursive, 6);

        GameManager.SkyboxManager._Rotation = rotation;
        //normal ui
        myUniverseBtn.onClick.AddListener(() => LoadGame());
        settingsBtn.onClick.AddListener(() => OpenSettings());
        customiseBtn.onClick.AddListener(() => Customise());
        quitBtn.onClick.AddListener(() => QuitGame());
        //options ui
        optionsBackBtn.onClick.AddListener(() => OptionBackBtn());
        optionsSelectBtn.onClick.AddListener(() => OptionSelectButton());
        //save load ui
        slBackBtn.onClick.AddListener(() => StartCoroutine(EnableNewGameMenu(false)));
        slCreateBtn.onClick.AddListener(() => SwitchCurrentNGMenu(NGMenuState.OptionsMenu));
        slLoadBtn.onClick.AddListener(() => LoadUniverse());
        slScrollBar.onValueChanged.AddListener((v) => OnSaveLoadScrollbar(v));
        //campaign creation ui
        ccCreateBtn.onClick.AddListener(() => CreateNewManualUniverse());
        ccBackBtn.onClick.AddListener(() => SwitchCurrentNGMenu(NGMenuState.OptionsMenu));
        for (int i = 0; i < uniButtons.Length; i++)
        {
            UniverseButton captured = uniButtons[i];
            uniButtons[i].button.onClick.AddListener(() => SetUniverseType(captured));
        }
        //new game ui
        ngBackBtn.onClick.AddListener(() => SwitchCurrentNGMenu(NGMenuState.OptionsMenu));
        ngCreateBtn.onClick.AddListener(() => CreateNewProceduralUniverse());
        ngSizeBtns[0].onClick.AddListener(() => SetGalaxySize(0));
        ngSizeBtns[1].onClick.AddListener(() => SetGalaxySize(1));
        ngSizeBtns[2].onClick.AddListener(() => SetGalaxySize(3));

        EnableDisableContinueBtn();
    }
    private void Update()
    {
        if(Input.anyKeyDown && !mainMenuShown)
        {
            menuAnim.Play("ContinueShowMenu");
            mainMenuShown = true;
        }
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
            GameManager.SkyboxManager._Rotation = rotation;
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
        onlyNGMenu = true;
        ngMenuAnim.Play("HideCreationMenu");
        ngMenuAnim.Play("ShowOptionMenu");
        ngMenuAnim.Play("HideSaveLoadMenu");
        ngMenuAnim.Play("HideCampaignCreationMenu");
        menuAnim.Play("HideMenu");
        UpdateDifficulty();
        currentNGMenuState = NGMenuState.OptionsMenu;
    }
    void Customise(){
        menuAnim.Play("HideMenuSpeed");
        cameraAnim.Play("CameraToShop");
        ngMenuAnim.Play("HideCreationMenu");
        ngMenuAnim.Play("HideSaveLoadMenu");
        shopAnim.Play("ShowShopDelay");
        ngMenuAnim.Play("HideCampaignCreationMenu");
        shopManager.EnableShop(0.75f, true);
    }
    //unity event button, called by subscribing in inspector
    public void QuitCustomise(){
        menuAnim.Play("ShowMenuWithDelay");
        shopAnim.Play("FadeShopDelay");
        cameraAnim.Play("ShopToCamera");
        shopManager.EnableShop(0f, false);
    }
    void LoadGame() {
        onlyNGMenu = false;
        StartCoroutine(EnableNewGameMenu(true));
        ngMenuAnim.Play("HideCreationMenu");
        ngMenuAnim.Play("HideOptionMenu");
        ngMenuAnim.Play("ShowSaveLoadMenu");
        ngMenuAnim.Play("HideCampaignCreationMenu");
        menuAnim.Play("HideMenu");
        currentNGMenuState = NGMenuState.SaveLoadMenu;
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
    void EnableDisableContinueBtn()
    {
        //continue button
        if (GameManager.LevelManager.currentUniverse.name != null && GameManager.SaveLoadManager.RetrieveSave(GameManager.LevelManager.currentUniverse.name + ".txt") != null)
        {
            continueBtn.gameObject.SetActive(true);
            newUniverseBtn.gameObject.SetActive(false);
            continueBtn.onClick.AddListener(() => Continue());
        }
        else
        {
            continueBtn.gameObject.SetActive(false);
            newUniverseBtn.gameObject.SetActive(true);
            newUniverseBtn.onClick.AddListener(() => NewGame());
        }

        //load universebutton
        if (SaveLoadManager.saves == null || SaveLoadManager.saves.Length == 0)
        {
            myUniverseBtn.interactable = false;
        }
        else
        {
            myUniverseBtn.interactable = true;
        }
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
        float maxValue = -Mathf.Clamp(SaveLoadManager.saves.Length - 3, 0, SaveLoadManager.saves.Length) * elementHeightJump;
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
        LevelManager.notifyProcedural?.Invoke(se.uniParams.universeType == LevelManager.UniverseParams.UniverseType.procedural);
        GameManager.SaveLoadManager.Load(se.uniParams.name + ".txt");
        SceneManager.LoadScene(proceduralGalaxiesScene);
    }
    void PopulateSaves()
    {
        if (SaveLoadManager.saves == null || SaveLoadManager.saves.Length == 0) { return; }
        SaveLoadManager.UniverseSave[] uniSaves = new SaveLoadManager.UniverseSave[SaveLoadManager.saves.Length];
        for (int i = 0; i < uniSaves.Length; i++)
        {
            uniSaves[i] = GameManager.SaveLoadManager.RetrieveSave(SaveLoadManager.saves[i].Name);
        }

        SaveElement[] allElements = saveElementParent.GetComponentsInChildren<SaveElement>(true);
        Array.Sort(uniSaves, (s1, s2) => s2.lastSaveTime.CompareTo(s1.lastSaveTime));
        for (int i = 0; i < allElements.Length; i++)
        {
            Destroy(allElements[i].gameObject);
        }
        for (int i = 0; i < uniSaves.Length; i++)
        {
            RectTransform tr = Instantiate(saveElementObj, saveElementParent).GetComponent<RectTransform>();
            tr.localPosition += new Vector3(0, elementHeightJump * i, 0);

            //Temporary bc no save load functionality
            LevelManager.UniverseParams uni = uniSaves[i].universeParams;
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
            seScript.InitValues(uni, difficultyColorGrad.Evaluate((float)uni.relativeDifficulty / 10), sizeColor, uniSaves[i].lastSaveTime, uni.universeType == LevelManager.UniverseParams.UniverseType.procedural).onClick.AddListener(() => DeleteSave(seScript));
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
    void OptionSelectButton()
    {
        Button b;
        if (lastSelectedGameObject == null || !lastSelectedGameObject.TryGetComponent<Button>(out b)) { return; }
        if (b == infiniteBtn) { SwitchCurrentNGMenu(NGMenuState.CreationMenu); }
        if (b == campaignBtn) { SwitchCurrentNGMenu(NGMenuState.CampaignCreationMenu); }
    }
    void OptionBackBtn()
    {
        if(!onlyNGMenu)
        {
            SwitchCurrentNGMenu(NGMenuState.SaveLoadMenu);
        }
        else
        {
            StartCoroutine(EnableNewGameMenu(false));
        }
    }
    /// <param name="size">0, 1 or 2 | small, medium, or large</param>
    void SetGalaxySize(int size)
    {
        size = Mathf.Clamp(size, 0, 2);

        for (int i = 0; i<ngSizeBtns.Length; i++)
        {
            bool enableText = i==size ? true : false;
            if(i==currentUniverseSize && enableText){
                ngSizeBtns[i].GetComponent<Animator>().Play("Reselect"); ngSizeHoverImgs[i].gameObject.SetActive(false);
            }
            else if(enableText) {ngSizeBtns[i].GetComponent<Animator>().Play("Select"); ngSizeHoverImgs[i].gameObject.SetActive(false);}
            else {ngSizeBtns[i].GetComponent<Animator>().Play("Unselect"); ngSizeHoverImgs[i].gameObject.SetActive(true);}
            
        }
        currentUniverseSize = size;
    }
    void SetUniverseType(UniverseButton uniButton)
    {
        if(uniButton.button.transform.GetComponent<UniButton>()._IsLocked) { return; }

        foreach (var ub in uniButtons)
        {
            if (ub.button == uniButton.button && currentUniverseType == uniButton.type)
            {
                ub.button.GetComponent<Animator>().Play("Reselect"); ub.hoverImage.gameObject.SetActive(false);
            }
            else if (ub.button == uniButton.button) { ub.button.GetComponent<Animator>().Play("Select"); ub.hoverImage.gameObject.SetActive(false); }
            else { ub.button.GetComponent<Animator>().Play("Unselect"); ub.hoverImage.gameObject.SetActive(true); }
        }

        currentUniverseType = uniButton.type;
    }
    void CreateNewManualUniverse()
    {
        LevelManager.UniverseParams up = manualUniverseParams;
        up.universeType = currentUniverseType;
        up.name = ccNameFld.text == string.Empty ? "MyUniverse" : ccNameFld.text; //failsafe
        up.universeType = currentUniverseType;
        up.relativeDifficulty = manualDifficulties[currentUniverseType];
        up.name = EnsureUniqueFileName(SaveLoadManager.saves, up.name + ".txt");
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
                if(currentNGMenuState == NGMenuState.CampaignCreationMenu)
                {
                    ngMenuAnim.Play("HideCampaignCreationMenu");
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
        if (SaveLoadManager.currentGlobalSave != null)
        {
            infiniteGroup.interactable = SaveLoadManager.currentGlobalSave.unlockedInfinite;
            infiniteGroup.alpha = !infiniteGroup.interactable ? 0.25f : 1f;
        }
        else
        {
            infiniteGroup.interactable = false;
            infiniteGroup.alpha = 0.25f;
        }
        if (enable)
        {
            PopulateSaves();
            ngMenu = true;
            menuAnim.Play("HideMenu");
            cameraAnim.Play("MenuToNGMenu");
            ngMenuAnim.Play("ShowNGMenu");
        }
        else if (!enable)
        {
            EnableDisableContinueBtn();
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
        universeName = universeName == string.Empty ? "MyUniverse" : universeName; //failsafe
        return new LevelManager.UniverseParams(universeName, seed, (int)ngDifficultySldr.value, ngSizeBtns[currentUniverseSize].gameObject.name, 0, numGalaxies, 0, LevelManager.UniverseParams.UniverseType.procedural);
    }

    void OnEnable(){
        SettingsManager.enableSettingsMenu += DisableSettings;
    }

    void OnDisable(){
        SettingsManager.enableSettingsMenu -= DisableSettings;
    }
    #endregion
}

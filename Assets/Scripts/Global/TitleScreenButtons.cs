using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreenButtons : MonoBehaviour
{
    [SerializeField] Animator cameraAnim, menuAnim, ngMenuAnim;
    [Header("Main Screen UI")]
    [SerializeField] Button continueBtn;
    [SerializeField] Button newGameBtn;
    [SerializeField] Button loadGameBtn;
    [SerializeField] Button settingsBtn;
    [Header("Save Load UI")]
    [SerializeField] Scrollbar slScrollBar;
    [SerializeField] Button slBackBtn;
    [SerializeField] Button slCreateBtn;
    [SerializeField] Button slLoadBtn;
    [SerializeField] float elementHeightJump;
    [SerializeField] Transform saveElementParent;
    [SerializeField] GameObject saveElementObj;
    [SerializeField] int numSaves;
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
    [SerializeField] LevelGenStaticReferences levelStatic;
    [SerializeField] Vector2Int[] minMaxGalaxies;
    [SerializeField] string proceduralGalaxiesScene;
    [SerializeField] Material skybox;
    [SerializeField] float speed;
    float rotation = 0.02f;
    bool ngMenu;
    int currentUniverseSize = 1; //0, 1, 2, for small, medium, large
    SaveElement[] saveElements;
    int currentSaveElement;
    void Start()
    {
        SetGalaxySize(1);
        UpdateDifficulty();
        PopulateSaves();
        ngMenu = false;
        skybox.SetFloat("_Rotation", rotation);
        //normal ui
        continueBtn.onClick.AddListener(() => Continue());
        newGameBtn.onClick.AddListener(() => NewGame());
        loadGameBtn.onClick.AddListener(() => LoadGame());
        settingsBtn.onClick.AddListener(() => OpenSettings());
        //save load ui
        slBackBtn.onClick.AddListener(() => StartCoroutine(EnableNewGameMenu(false)));
        slCreateBtn.onClick.AddListener(() => ngMenuAnim.Play("ShowCreationMenu"));
        slLoadBtn.onClick.AddListener(() => LoadProceduralUniverse());
        //new game ui
        ngBackBtn.onClick.AddListener(() => ngMenuAnim.Play("HideCreationMenu"));
        ngCreateBtn.onClick.AddListener(() => CreateNewProceduralUniverse());
        ngSizeBtns[0].onClick.AddListener(() => SetGalaxySize(0));
        ngSizeBtns[1].onClick.AddListener(() => SetGalaxySize(1));
        ngSizeBtns[2].onClick.AddListener(() => SetGalaxySize(3));
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && ngMenu) 
        {
            StartCoroutine(EnableNewGameMenu(false));
        }
        if(ngMenu){
            rotation += Time.deltaTime * speed;
            if(rotation > 360){
                rotation = 0;
            }
            skybox.SetFloat("_Rotation", rotation);
        }
    }
    #region Main Screen UI
    void Continue(){
        SceneManager.LoadScene("Galaxies");

    }
    void NewGame(){
        StartCoroutine(EnableNewGameMenu(true));
    }
    void LoadGame(){
        
    }
    void OpenSettings(){
        
    }
    #endregion
    #region Save Load UI
    
    void SelectSaveElement(int elementIndex)
    {
        currentSaveElement = elementIndex;
    }
    void DeleteSave(int elementIndex)
    {
        Destroy(saveElements[elementIndex].gameObject);
        LevelManager.setProceduralUniverse?.Invoke(new LevelManager.UniverseParams());
        LevelManager.notifyProcedural?.Invoke(false);
        PopulateSaves();
        currentSaveElement = 0;
    }
    void LoadProceduralUniverse()
    {
        if(saveElements[currentSaveElement] == null || currentSaveElement >= saveElements.Length) {return;}
        LevelManager.setProceduralUniverse?.Invoke(saveElements[currentSaveElement].uniParams);
        SceneManager.LoadScene(proceduralGalaxiesScene);
    }
    void PopulateSaves()
    {
        numSaves = GameManager.LevelManager.currentUniverse == new LevelManager.UniverseParams() ? 0 : 1;
        saveElements = new SaveElement[numSaves];
        for(int i = 0; i < numSaves; i++)
        {
            RectTransform tr = Instantiate(saveElementObj, saveElementParent).GetComponent<RectTransform>();
            tr.position += new Vector3(0,elementHeightJump * i,0);

            //Temporary bc no save load functionality
            LevelManager.UniverseParams uni = GameManager.LevelManager.currentUniverse;
            Color sizeColor = Color.white;
            if (uni.numGalaxies >= minMaxGalaxies[0].x && uni.numGalaxies <= minMaxGalaxies[0].y)
            {
                sizeColor = difficultyColorGrad.Evaluate(0f);
            }
            else if (uni.numGalaxies >= minMaxGalaxies[1].x && uni.numGalaxies <= minMaxGalaxies[1].y)
            {
                sizeColor = difficultyColorGrad.Evaluate(0.5f);
            }
            else if (uni.numGalaxies >= minMaxGalaxies[2].x && uni.numGalaxies <= minMaxGalaxies[2].y)
            {
                sizeColor = difficultyColorGrad.Evaluate(1f);
            }

            //button listening
            tr.GetComponent<Button>().onClick.AddListener(() => SelectSaveElement(i-1)); //wtf

            //script setting
            SaveElement seScript = tr.GetComponent<SaveElement>();
            seScript.InitValues(uni, difficultyColorGrad.Evaluate((float)uni.relativeDifficulty/10), sizeColor).onClick.AddListener(() => DeleteSave(i-1));
            saveElements[i] = seScript;
        }
    }
    #endregion
    #region New Game UI
    public void UpdateDifficulty()
    {
        difficultyNumber.text = ngDifficultySldr.value.ToString();
        difficultyNumber.color = difficultyColorGrad.Evaluate(ngDifficultySldr.value/10);
    }
    public void RandomizeSeed()
    {
        UnityEngine.Random.InitState(Mathf.RoundToInt(System.DateTime.Now.Millisecond * System.DateTime.Now.Second * Time.time));
        ngSeedFld.text = UnityEngine.Random.Range(-2147483647, 2147483647).ToString();
    }
    /// <param name="size">0, 1 or 2 | small, medium, or large</param>
    void SetGalaxySize(int size)
    {
        size = Mathf.Clamp(size, 0, 2);
        currentUniverseSize = size;
        for (int i = 0; i<ngSizeBtns.Length; i++)
        {
            bool enableText = i==size ? true : false;
            if(enableText) {ngSizeBtns[i].transform.GetChild(0).GetComponent<Animator>().Play("FadeText");}
            else {ngSizeBtns[i].transform.GetChild(0).GetComponent<Animator>().Play("FadeTextOut");}
            
        }
    }
    void CreateNewProceduralUniverse() //create button in new game menu
    {
        LevelManager.UniverseParams universeParams = SynthesizeUniverseParams();
        LevelManager.setProceduralUniverse?.Invoke(universeParams);
        SceneManager.LoadScene(proceduralGalaxiesScene);
    }
    IEnumerator EnableNewGameMenu(bool enable)
    {
        if(enable)
        {
            ngMenu = true;
            menuAnim.Play("HideMenu");
            // yield return new WaitForSeconds(menuAnim.GetCurrentAnimatorStateInfo(0).length/3);
            cameraAnim.Play("MenuToNGMenu");
            // yield return new WaitForSeconds(cameraAnim.GetCurrentAnimatorStateInfo(0).length/3);
            ngMenuAnim.Play("ShowNGMenu");
        }
        else if (!enable)
        {
            ngMenu = false;
            ngMenuAnim.Play("HideNGMenu");
            // yield return new WaitForSeconds(ngMenuAnim.GetCurrentAnimatorStateInfo(0).length/3);
            cameraAnim.Play("NGMenuToMenu");
            // yield return new WaitForSeconds(cameraAnim.GetCurrentAnimatorStateInfo(0).length/3);
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
        return new LevelManager.UniverseParams(universeName, seed, (int)ngDifficultySldr.value, ngSizeBtns[currentUniverseSize].gameObject.name, 0, numGalaxies, levelStatic);
    }
    #endregion
}

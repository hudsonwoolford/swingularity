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
    [Header("New Game UI")]
    [SerializeField] Button ngBackBtn;
    [SerializeField] Button ngCreateBtn;
    [SerializeField] TMP_InputField ngNameFld;
    [SerializeField] TMP_InputField ngSeedFld;
    [SerializeField] Slider ngDifficultySldr;
    [SerializeField] TextMeshProUGUI difficultyNumber;
    [SerializeField] Gradient difficultyColorGrad;
    [Header("Miscellaneous Params")]
    [SerializeField] LevelGenStaticReferences levelStatic;
    [SerializeField] Vector2Int minMaxGalaxies;
    [SerializeField] string proceduralGalaxiesScene;
    bool ngMenu;
    void Start()
    {
        UpdateDifficulty();
        ngMenu = false;
        continueBtn.onClick.AddListener(() => Continue());
        newGameBtn.onClick.AddListener(() => NewGame());
        loadGameBtn.onClick.AddListener(() => LoadGame());
        settingsBtn.onClick.AddListener(() => OpenSettings());
        ngBackBtn.onClick.AddListener(() => StartCoroutine(EnableNewGameMenu(false)));
        ngCreateBtn.onClick.AddListener(() => CreateNewProceduralUniverse());
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && ngMenu) 
        {
            StartCoroutine(EnableNewGameMenu(false));
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
    void CreateNewProceduralUniverse() //create button in new game menu
    {
        LevelManager.UniverseParams universeParams = SynthesizeUniverseParams();
        LevelManager.setProceduralUniverse(universeParams);
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
        int numGalaxies = UnityEngine.Random.Range(minMaxGalaxies.x, minMaxGalaxies.y); //random number of galaxies
        numGalaxies = (numGalaxies<=0) ? 1 : numGalaxies; //failsafe for zero galaxies
        string universeName = ngNameFld.text; //save name of universe from field
        universeName = universeName == string.Empty ? "MyUniverse" : universeName; //failsafe - needs to not be the same name as other saves - not implemented
        return new LevelManager.UniverseParams(universeName, seed, (int)ngDifficultySldr.value, numGalaxies, levelStatic);
    }
    #endregion
}

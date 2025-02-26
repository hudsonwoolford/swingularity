using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using static GalaxyGenerator;
using static LevelGenerator;
using static LevelManager;

public class GalaxyMenu : MonoBehaviour
{
    [System.Serializable]
    struct GalaxyInfoType
    {
        public ColorSchemeStaticReference.PlanetType type;
        public LevelGenerator.SpecialType specialType;
    }
    [System.Serializable]
    struct SpecialObjectIcon
    {
        public SpecialType specialType;
        public Sprite icon;
        public Color color;
    }

    [SerializeField] LevelManager.GalaxyParams[] galaxyList;
    [Header("Important Positions and Rotations")]
    [SerializeField] Vector3 focusPosition; //focused centre, not zoomed in
    [SerializeField] Vector3 zoomGalaxyPos; //zoomed position of galaxy
    [SerializeField] Vector3 zoomCameraPos; //zoomed position of camera
    [SerializeField] Vector3 linePos; //position of galaxy line, x does not matter
    [SerializeField] Vector3 minRotation; //minimum rotation of galaxies
    [SerializeField] Vector3 maxRotation; //maximum rotation of galaxies
    [Header("Animations")]
    [SerializeField] float scrollSmoothness = 0.15f; //scales scrolling speed
    [SerializeField] float scrollDelayTime = 0.15f; //scales scrolling speed
    [SerializeField] float zoomSmoothness = 0.2f; //scales zooming speed
    [SerializeField] float buttonThresholdTime; //time holding down key before starts scrolling smoothly
    [SerializeField] float starScaleDuration;
    [SerializeField] float starIntervalDuration;
    [SerializeField] AnimationCurve starScaleCurve;
    [SerializeField] float starPitchInterval;
    [SerializeField] float starStartPitch;
    [SerializeField] float coinFillSpeed = 0.1f;
    [Header("Main UI")]
    [SerializeField] Button rightBtn; //move buttons
    [SerializeField] Button leftBtn; //move buttons
    [SerializeField] Button zoomBtn; //move buttons
    [SerializeField] TextMeshProUGUI starCount;
    [SerializeField] TextMeshProUGUI starCoinCount;
    [SerializeField] GameObject dotButton; //created in create galaxy funciton, one dot per galaxy
    [SerializeField] Vector3 dotMiddle;
    [SerializeField] float dotDistanceInterval;
    [SerializeField] Canvas galaxyMenuCanvas;
    [Header("Galaxy Obj")]
    [SerializeField] float distInterval; //distance between galaxies
    [SerializeField] float sizeScalar; //scales galaxy size
    [SerializeField] Transform galaxyObjParent; //parent of galaxy controllers
    [SerializeField] GameObject galaxyObj; //prefab for instantiation
    [SerializeField] GameObject galaxyUISummary; //prefab for instantiation
    [Header("Miscellaneous")]
    [SerializeField] GalaxyInfoType[] galaxyTypes; //possible types for galaxies
    [SerializeField] SpecialType[] allSpecialTypes;
    [Header("Info Menu")]
    [SerializeField] Animator infoCardAnim;
    [SerializeField] TextMeshProUGUI galaxyTitle;
    [SerializeField] Button playGalaxyBtn, infoBackBtn;
    [SerializeField] TextMeshProUGUI difficultyNum;
    [SerializeField] Slider difficultySlider;
    [SerializeField] Gradient difficultyGradient;
    [SerializeField] TextMeshProUGUI astroBiomeTxt;
    [SerializeField] TextMeshProUGUI noEncontersTxt;
    [SerializeField] Image[] iconImgs;
    [SerializeField] Image[] starImgs;
    [SerializeField] Sprite[] starIcon;
    [SerializeField] SpecialObjectIcon[] iconSprites;
    [SerializeField] TextMeshProUGUI bestScoreTxt;
    [SerializeField] TextMeshProUGUI coinsCollectionText;
    [Header("Sounds")]
    [SerializeField] AudioObj scrollGalaxyAudio;
    [SerializeField] AudioObj zoomGalaxyAudio;
    [SerializeField] AudioObj[] starAudios = new AudioObj[5];
    [SerializeField] AudioObj fiveStarAudio;
    [SerializeField] AudioObj singleCoinAudio;
    [SerializeField] AudioObj cashoutAudio;
    [SerializeField] AudioSource galaxySrc;
    Transform[] galaxyDots;
    Transform selectedDot;
    Vector3 cameraOrigin;
    Transform mainCam;
    GalaxyController[] galaxyCons; //array of controllers in the scene
    GalaxySummary[] galaxySummaries; //array of summary ui
    Animator menuAnim;
    int focusedGalaxy; //index of focused galaxy
    bool zoomedIn;
    bool infoCardEnabled;
    bool started;
    bool scrollStopper;
    Vector2 mouseStartPos;
    Coroutine delayZoomout;

    Vector3 dotVel;
    Vector3[] galaxyVel;
    Vector3 cameraVel;
    // Start is called before the first frame update
    void Start()
    {
        if(started) { return; }

        menuAnim = GetComponent<Animator>();
        mainCam = Camera.main.transform;
        cameraOrigin = mainCam.position;
        if(GameManager.LevelManager.procedural && GameManager.LevelManager.currentUniverse.proceduralUniverse)
        {
            CreateProceduralGalaxies();
        }
        else
        {
            bool notLMSet = false;
            if(GameManager.LevelManager.currentProceduralUniverse != null && GameManager.LevelManager.currentProceduralUniverse.Length > 0)
            {
                for (int i = 0; i < galaxyList.Length; i++)
                {
                    if (i < GameManager.LevelManager.currentProceduralUniverse.Length)
                    {
                        notLMSet |= !galaxyList[i].EqualNoStar(GameManager.LevelManager.currentProceduralUniverse[i], galaxyList[i]);
                    }
                    else { notLMSet = true; }

                }
            }
            else { notLMSet = true; }
            if (notLMSet)
            {
                LevelManager.setGalaxies?.Invoke(galaxyList);
            }
            else
            {
                galaxyList = GameManager.LevelManager.currentProceduralUniverse;
            }
        }
        CreateControllers();
        rightBtn.onClick.AddListener(() => MoveButton(1));
        leftBtn.onClick.AddListener(() => MoveButton(-1));
        zoomBtn.onClick.AddListener(() => ZoomButton(true));
        playGalaxyBtn.onClick.AddListener(() => PlayGalaxy());
        infoBackBtn.onClick.AddListener(() => InfoCardBackButton());

        focusedGalaxy = Mathf.Clamp(GameManager.LevelManager.galaxyIndex, 0, galaxyList.Length);
        galaxyVel = new Vector3[galaxyCons.Length];
        Vector3[] galaxyPos = GalaxyPositions(galaxyCons.Length, focusedGalaxy);
        for (int i = 0; i < galaxyCons.Length; i++)
        {
            galaxyCons[i].transform.position = galaxyPos[i];
        }
        started = true;
    }
    void Update()
    {
        MenuInput();
        MoveGalaxies();
        MoveCamera();
    }
    void MoveGalaxies()
    {
        Vector3[] galaxyPos = GalaxyPositions(galaxyCons.Length, focusedGalaxy);

        for (int i = 0; i < galaxyCons.Length; i++)
        {
            galaxyCons[i].transform.position = Vector3.SmoothDamp(galaxyCons[i].transform.position, galaxyPos[i], ref galaxyVel[i],  scrollSmoothness);
        }
        selectedDot.transform.position = Vector3.SmoothDamp(selectedDot.transform.position, galaxyDots[focusedGalaxy].position, ref dotVel, scrollSmoothness);
    }
    void MoveCamera()
    {
        Vector3 targetPos = zoomedIn ? zoomCameraPos : cameraOrigin;

        if(targetPos != mainCam.transform.position)
        {
            mainCam.transform.position = Vector3.SmoothDamp(mainCam.transform.position, targetPos, ref cameraVel, zoomSmoothness);

            if(infoCardEnabled == zoomedIn) { return; }

            float progress = (mainCam.transform.position - cameraOrigin).magnitude / (zoomCameraPos - cameraOrigin).magnitude;
            if(infoCardEnabled == false && zoomedIn && progress > 0.9f)
            {
                EnableAndSetInfoCard(true, galaxyList[focusedGalaxy]);
            }
            else if(infoCardEnabled == true && !zoomedIn)
            {
                EnableAndSetInfoCard(false, galaxyList[focusedGalaxy]);
            }
        }



    }
    #region Buttons
    void MoveButton(int dir)
    {
        int realMovement = Mathf.Clamp((focusedGalaxy + dir), 0, galaxySummaries.Length - 1) - focusedGalaxy;
        focusedGalaxy += realMovement;
        if(realMovement != 0)
        {
            GameManager.SFXManager.PlaySFX(scrollGalaxyAudio);
        }
    }
    void ZoomButton(bool zoomIn)
    {
        if(focusedGalaxy == galaxySummaries.Length-1) { return; }

        ZoomGalaxy(zoomIn, false);
    }
    void InfoCardBackButton()
    {
        ZoomButton(false);
    }
    void PlayGalaxy()
    {
        if(GameManager.LevelManager.procedural)
        {
            SetProceduralGalaxy(galaxyList[focusedGalaxy]);
            GameManager.SceneHandling.LoadScene("LevelGen", true);
        }
        else
        {
            SetManualGalaxy();
        }
    }
    void DotButton(int dotIndex)
    {
        int movement = dotIndex - focusedGalaxy;
        if(movement != 0)
        {
            GameManager.SFXManager.PlaySFX(scrollGalaxyAudio);
            focusedGalaxy = LoopValue(focusedGalaxy+movement, 0, galaxySummaries.Length);
        }
    }
    void MenuInput()
    {
        if(!zoomedIn)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveButton(1);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveButton(-1);
            }
            if (Input.mouseScrollDelta.y != 0 && !scrollStopper)
            {
                if (Input.mouseScrollDelta.y < 0)
                {
                    MoveButton(1);
                }
                else
                {
                    MoveButton(-1);
                }
                scrollStopper = true;
                StartCoroutine(ScrollDelay());
            }
            if (Input.GetMouseButtonDown(0))
            {
                mouseStartPos = Input.mousePosition;
            }
            if (Input.GetMouseButton(0))
            {
                if (Mathf.Abs(Input.mousePosition.x - mouseStartPos.x) > 100)
                {
                    MoveButton(-(int)Mathf.Sign(Input.mousePosition.x - mouseStartPos.x));
                    mouseStartPos = Input.mousePosition;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) && zoomedIn)
        {
            ZoomButton(false);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && !zoomedIn)
        {
            ZoomButton(true);
        }
    }
    IEnumerator ScrollDelay(){
        yield return new WaitForSeconds(scrollDelayTime);
        scrollStopper = false;
    }

    void SetManualGalaxy()
    {
        string[] galaxyLevels = new string[5];
        string sceneSuffix = galaxyList[focusedGalaxy].possibleTypes[0].ToString();
        sceneSuffix = char.ToUpper(sceneSuffix[0]) + sceneSuffix.Substring(1);
        for (int i = 0; i < 5; i++)
        {
            galaxyLevels[i] = sceneSuffix + "_" + i.ToString();
        }
        initSystem?.Invoke(galaxyLevels, galaxyList[focusedGalaxy], LevelManager.allGalaxyData[galaxyList[focusedGalaxy].galaxyDataIndex]);
        GameManager.SceneHandling.LoadScene(galaxyLevels[0], true);
    }
    void SetProceduralGalaxy(LevelManager.GalaxyParams gp)
    {
        LevelManager.LevelParams[] galaxyLevelParams = new LevelManager.LevelParams[5];
        string[] galaxyLevels = new string[5];
        UnityEngine.Random.InitState(gp.seed);
        int collectibleIndex = UnityEngine.Random.Range(0, 5);
        int numOnes = 0;
        for (int i = 0; i < 5; i++) //Creating levels as level params
        {
            UnityEngine.Random.InitState(i * 120523 + gp.seed);
            int levelSeed = UnityEngine.Random.Range(-2147483647, 2147483647);
            int levelDifficulty = Mathf.Clamp(gp.relativeDifficulty, 1, 10);
            int par = LevelManager.levelStatic.difficultyPars[levelDifficulty - 1] + UnityEngine.Random.Range(-1, 2);
            if(par == 1){
                if(numOnes == 2){
                    int u = 0;
                    while(par == 1){
                        UnityEngine.Random.InitState(i * 120523 + gp.seed + u);
                        par = LevelManager.levelStatic.difficultyPars[levelDifficulty - 1] + UnityEngine.Random.Range(-1, 2);
                        u++;
                    }
                }
                else{
                    numOnes++;
                }
            }
            LevelManager.LevelParams systemParams = new LevelManager.LevelParams(levelSeed, levelDifficulty, par, gp.possibleTypes, gp.possibleSpecial, i == collectibleIndex);


            galaxyLevelParams[i] = systemParams;
            galaxyLevels[i] = "LevelGen";
        }
        initProceduralSystem?.Invoke(galaxyLevels, galaxyLevelParams, galaxyList[focusedGalaxy], LevelManager.allGalaxyData[gp.galaxyDataIndex]);
        LevelSettingsManager.onLevelSettingChange?.Invoke(galaxyLevelParams[0]);
        GameManager.LevelSettingsManager.UIMatchParams();
    }//Sets the procedural galaxy in level manager through event
    #endregion
    #region Transitions
    void EnableAndSetInfoCard(bool enable, LevelManager.GalaxyParams gp)
    {
        infoCardEnabled = enable;
        if (enable)
        {
            infoCardAnim.Play("ShowInfoCard");
            galaxyTitle.text = gp.name;
            difficultyNum.text = gp.relativeDifficulty.ToString();
            difficultyNum.color = difficultyGradient.Evaluate(gp.relativeDifficulty / difficultySlider.maxValue);
            difficultySlider.value = gp.relativeDifficulty + 1;
            ColorBlock cB = difficultySlider.colors;
            cB.disabledColor = difficultyNum.color;
            difficultySlider.colors = cB;
            astroBiomeTxt.text = gp.possibleTypes[0].ToString();
            if(gp.possibleTypes[0] == ColorSchemeStaticReference.PlanetType.goop){
                astroBiomeTxt.text = "Alien";
            }
            astroBiomeTxt.text = astroBiomeTxt.text.ToUpper();
            astroBiomeTxt.color = LevelManager.allGalaxyData[gp.galaxyDataIndex].mainColor;
            bestScoreTxt.text = gp.bestScore;
            if(gp.bestScore != "——"){
                if(float.Parse(gp.bestScore) > 0){
                    bestScoreTxt.color = difficultyGradient.Evaluate(1f);
                }
                else if (float.Parse(gp.bestScore) < 0) {
                    bestScoreTxt.color = difficultyGradient.Evaluate(0f);
                }
                else{
                    bestScoreTxt.color = difficultyGradient.Evaluate(0.5f);
                }
            }
            else{
                bestScoreTxt.color = new Color(1,1,1,1);
            }

            noEncontersTxt.enabled = gp.possibleSpecial == null || gp.possibleSpecial.Length == 0 ? true : false;
            SpecialObjectIcon[] icons = new SpecialObjectIcon[gp.possibleSpecial.Length];
            for (int i = 0; i < icons.Length; i++)
            {
                for (int x = 0; x < iconSprites.Length; x++)
                {
                    if (iconSprites[x].specialType == gp.possibleSpecial[i])
                    {
                        icons[i] = iconSprites[x];
                    }
                }
            }
            for (int i = 0; i < iconImgs.Length; i++)
            {
                if(i>=icons.Length)
                {
                    iconImgs[i].gameObject.SetActive(false);
                    continue;
                }
                iconImgs[i].gameObject.SetActive(true);
                iconImgs[i].color = icons[i].color;
                iconImgs[i].sprite = icons[i].icon;
            }
            for(int i = 0; i < starImgs.Length; i++){
                if(i >= gp.collectedStars.Length) { continue; }
                if(gp.collectedStars[i]){
                    starImgs[i].sprite = starIcon[i];
                }
                else{
                    starImgs[i].sprite = starIcon[starImgs.Length];
                }
            }
        }
        else
        {
            infoCardAnim.Play("HideInfoCard");
        }
    }
    void ZoomGalaxy(bool zoomIn, bool instant)
    {
        if (galaxyList[focusedGalaxy].requiredStars > GameManager.LevelManager.currentUniverse.numStars) { galaxySummaries[focusedGalaxy].Shake(); return; }
        if(delayZoomout!=null)
        {
            StopCoroutine(delayZoomout);
        }

        if(instant)
        {
            zoomedIn = zoomIn;
            galaxyCons[focusedGalaxy].transform.position = zoomGalaxyPos;
            mainCam.transform.position = zoomCameraPos;
            EnableAndSetInfoCard(zoomIn, galaxyList[focusedGalaxy]);
            return;
        }

        if (zoomIn)
        {
            GameManager.SFXManager.PlaySFX(zoomGalaxyAudio);
            zoomedIn = true;
            galaxySummaries[focusedGalaxy].EnableSummary(false);
            menuAnim.Play("HideMenu");
        }
        else if(!instant)
        {
            delayZoomout = StartCoroutine(DelayZoomout(0.3f));
        }

    }
    IEnumerator DelayZoomout(float delay)
    {
        EnableAndSetInfoCard(false, galaxyList[focusedGalaxy]);
        yield return new WaitForSeconds(delay);

        GameManager.SFXManager.PlaySFX(zoomGalaxyAudio);
        zoomedIn = false;
        galaxySummaries[focusedGalaxy].EnableSummary(true);
        menuAnim.Play("ShowMenu");
    }
    #endregion
    #region Galaxy Configuration
    Vector3[] GalaxyPositions(int numGalaxies, int focus) //returns positions of the galaxies based on the focus
    {
        Vector3[] targetPositions = new Vector3[numGalaxies];
        for (int i = 0; i < numGalaxies; i++)
        {
            targetPositions[i] = new Vector3((i-focus) * distInterval + focusPosition.x, linePos.y, linePos.z);
            if(i == focus)
            {
                if(zoomedIn)
                {
                    targetPositions[i] = zoomGalaxyPos;
                }
                else
                {
                    targetPositions[i] = focusPosition;
                }
            }
        }
        return targetPositions;
    }
    void CreateControllers() //Instantiates all galaxies
    {
        starCount.text = GameManager.LevelManager.currentUniverse.numStars.ToString();
        starCoinCount.text = SaveLoadManager.currentGlobalSave.totalSupernovae.ToString();
        galaxyCons = new GalaxyController[galaxyList.Length + 1];
        galaxySummaries = new GalaxySummary[galaxyList.Length + 1];
        Vector3[] positions = GalaxyPositions(galaxyList.Length + 1, focusedGalaxy);
        for (int i = 0; i < galaxyList.Length+1; i++) //galaxy obj
        {
            //instantiating galaxies
            galaxyCons[i] = Instantiate(galaxyObj, galaxyObjParent).GetComponent<GalaxyController>();
            galaxyCons[i].transform.localScale = galaxyCons[i].transform.localScale * sizeScalar;
            if (i != galaxyList.Length)
            {
                UnityEngine.Random.InitState(galaxyList[i].seed);
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                galaxyCons[i].transform.rotation = Quaternion.Euler(Vector3.Lerp(minRotation, maxRotation, randomValue));
                galaxyCons[i].transform.position = positions[i];
                galaxyCons[i].SetProceduralScheme(galaxyList[i]);
            }
            else
            {
                galaxyCons[i].enabled = false;
                foreach (var pr in galaxyCons[i].GetComponentsInChildren<ParticleSystem>())
                {
                    Destroy(pr.gameObject);
                }
                galaxyCons[i].transform.position = positions[i];
            }

            //instantiating UI for galaxies
            galaxySummaries[i] = Instantiate(galaxyUISummary, galaxyCons[i].transform).GetComponent<GalaxySummary>();

            if (i != galaxyList.Length)
            {
                galaxySummaries[i].InitSummary(galaxyList[i].collectedStars, galaxyList[i].requiredStars);
            }
            else { galaxySummaries[i].InitUniverseSummary();}

            galaxySummaries[i].transform.rotation = Quaternion.identity;
        }

        float halfLength = ((galaxyCons.Length-1) / 2) * dotDistanceInterval;
        galaxyDots = new Transform[galaxyCons.Length];
        for (int i = 0; i < galaxyCons.Length; i++) //dot button creation
        {
            RectTransform t = Instantiate(dotButton, transform).GetComponent<RectTransform>();
            galaxyDots[i] = t;
            t.GetChild(0).GetComponent<Image>().enabled = false;
            t.localPosition = new Vector3(dotMiddle.x - halfLength + dotDistanceInterval * i, dotMiddle.y, dotMiddle.z);
            int callIndex = i;
            t.gameObject.GetComponent<Button>().onClick.AddListener(() => DotButton(callIndex));
        }
        selectedDot = Instantiate(dotButton, transform).transform;
        selectedDot.GetChild(0).GetComponent<Image>().enabled = true;
        selectedDot.position = galaxyDots[focusedGalaxy].position;
    }
    #endregion
    #region Utility
    (float, float) SetDifficultyMinMax(float relativeDifficulty){
        float galaxyDifficultyMin = relativeDifficulty - 2;
        float galaxyDifficultyMax = relativeDifficulty + 2;

        if(relativeDifficulty == 1){
            galaxyDifficultyMin = 1;
            galaxyDifficultyMax = 1;
        }
        if(relativeDifficulty == 2){
            galaxyDifficultyMin = 1;
            galaxyDifficultyMax = 3;
        }
        if(relativeDifficulty == 9){
            galaxyDifficultyMin = 8;
            galaxyDifficultyMax = 10;
        }
        if(relativeDifficulty == 10){
            galaxyDifficultyMin = 10;
            galaxyDifficultyMax = 10;
        }
        return(galaxyDifficultyMin, galaxyDifficultyMax);
    }
    void CreateProceduralGalaxies()
    {
        string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        string numbers = "1234567890";
        string[] startingLetters = new string[6]{
            "NGC",
            "IC",
            "CGCG",
            "UGC",
            "PGC",
            "LEDA",
        };

        LevelManager.UniverseParams universe = GameManager.LevelManager.currentUniverse; //retrieves current universe
        UnityEngine.Random.InitState(universe.seed);
        LevelManager.GalaxyParams[] galaxies = new LevelManager.GalaxyParams[universe.numGalaxies]; //initialize galaxy array

        (float, float) galaxyDifficulties = SetDifficultyMinMax(universe.relativeDifficulty);

        float galaxyDifficultyMin = galaxyDifficulties.Item1;
        float galaxyDifficultyMax = galaxyDifficulties.Item2;

        int galaxyStarLockTracker = 0;
        bool notLMSet = false;
        for (int i = 0; i < galaxies.Length; i++) //for each galaxy index, create one
        {
            UnityEngine.Random.InitState(universe.seed + i); //different random values

            //random name, temporary
            #region Random Name
            string name = "";
            int type = UnityEngine.Random.Range(0, 2); //nametype
            if(type == 0){
                int numParts = UnityEngine.Random.Range(2,4);
                for (int c = 0; c < numParts; c++) 
                {
                    if(name.Length < 12){
                        int roll = UnityEngine.Random.Range(0,160);
                        if (roll <= LevelManager.levelStatic.GalaxyWords.Length){
                            string addend = LevelManager.levelStatic.GalaxyWords[UnityEngine.Random.Range(0, LevelManager.levelStatic.GalaxyWords.Length)];
                            if(c == 0){
                                if (lowerCase.Contains(addend[0])){
                                    int index = lowerCase.IndexOf(addend[0]);
                                    char[] addendChars = addend.ToCharArray();
                                    addendChars[0] = upperCase[index];
                                    addend = new string(addendChars);
                                }
                            }
                            name += addend;
                            name += " ";
                        }
                        else if (roll > LevelManager.levelStatic.GalaxyWords.Length && roll < 145){
                            int numNumbers = UnityEngine.Random.Range(3,6);
                            for (int k = 0; k < numNumbers; k++){
                                name += numbers[UnityEngine.Random.Range(0,numbers.Length)];
                            }
                            name += " ";
                        }
                        else{       
                            int numChars = UnityEngine.Random.Range(3,6);
                            for (int k = 0; k < numChars; k++){
                                bool letter = UnityEngine.Random.Range(0,2) == 1;
                                if(letter){
                                    name += upperCase[UnityEngine.Random.Range(0,upperCase.Length)];
                                }
                                else{
                                    name += numbers[UnityEngine.Random.Range(0,numbers.Length)];
                                }
                            }
                            name += " ";
                        }
                    }
                }
            }
            if (type == 1){
                name += startingLetters[UnityEngine.Random.Range(0,startingLetters.Length)];
                name += " ";

                int numNumbers = UnityEngine.Random.Range(3,7);
                for (int c = 0; c < numNumbers; c++){
                    name += numbers[UnityEngine.Random.Range(0,numbers.Length)];
                }
            }
            #endregion

            //random planet types
            ColorSchemeStaticReference.PlanetType[] possibleTypes = new ColorSchemeStaticReference.PlanetType[1];
            int numSpecial = UnityEngine.Random.Range(2, 5);
            LevelGenerator.SpecialType[] specialTypes = new LevelGenerator.SpecialType[numSpecial];
            List<LevelGenerator.SpecialType> allSpecial = allSpecialTypes.ToList();
            int gDataIndex = 0;
            for (int t = 0; t < possibleTypes.Length; t++) 
            {
                int chosenIndex = UnityEngine.Random.Range(0, galaxyTypes.Length);
                possibleTypes[0] = galaxyTypes[chosenIndex].type;
                specialTypes[0] = galaxyTypes[chosenIndex].specialType;
                allSpecial.Remove(specialTypes[0]);
                for (int x = 1; x < numSpecial; x++)
                {
                    int chosenSpecial = UnityEngine.Random.Range(0, allSpecial.Count);
                    specialTypes[x] = allSpecial[chosenSpecial];
                    allSpecial.Remove(specialTypes[x]);
                }
                for (int x = 0; x < LevelManager.allGalaxyData.Length; x++)
                {
                    if (LevelManager.allGalaxyData[x].type == possibleTypes[t]) { gDataIndex = x; }
                }
            }


            float difficultyLerpVal = (float)i/(float)galaxies.Length;
            int galaxyDifficulty = (int)Mathf.Lerp(galaxyDifficultyMin, galaxyDifficultyMax, difficultyLerpVal);

            galaxies[i] = new LevelManager.GalaxyParams(name.ToUpper(), UnityEngine.Random.Range(-2147483647, 2147483647), Mathf.Clamp(galaxyDifficulty,1,10), gDataIndex, possibleTypes, specialTypes, new bool[5], galaxyStarLockTracker, "——", false); //setting galaxy at index
            galaxyStarLockTracker += UnityEngine.Random.Range(2, 4);


            if (GameManager.LevelManager.currentProceduralUniverse != null && GameManager.LevelManager.currentProceduralUniverse.Length > 0)
            {
                if(i < GameManager.LevelManager.currentProceduralUniverse.Length)
                {
                    notLMSet |= !galaxies[i].EqualNoStar(GameManager.LevelManager.currentProceduralUniverse[i], galaxies[i]);
                }
                else { notLMSet = true; }
            }
            else { notLMSet = true; }
        }
        if (notLMSet) { LevelManager.setGalaxies?.Invoke(galaxies); LevelManager.notifyProcedural?.Invoke(true); }
        galaxyList = GameManager.LevelManager.currentProceduralUniverse;
        GameManager.SaveLoadManager.Save();
    }
    void OnDrawGizmos() //Visual aid
    {
        Gizmos.color = new Color(1,0,0,0.25f);
        Gizmos.DrawLine(new Vector3(-1000, linePos.y, linePos.z), new Vector3(1000, linePos.y, linePos.z));
        Gizmos.color = new Color(0,1,0,0.25f);
        Gizmos.DrawCube(focusPosition, Vector3.one);
        Gizmos.DrawCube(new Vector3(focusPosition.x-distInterval, linePos.y, linePos.z), Vector3.one);
        Gizmos.DrawCube(new Vector3(focusPosition.x+distInterval, linePos.y, linePos.z), Vector3.one);
        Gizmos.color = new Color(1, 0, 0, 0.25f);
        Gizmos.DrawCube(zoomGalaxyPos, Vector3.one);
        Gizmos.DrawSphere(zoomCameraPos, 1);
    }
    int LoopValue(int value, int min, int max) //utility to loop values
    {
        int range = max - min;
        if (range <= 0) return min; // Handle edge case if min and max are equal or max is less than min

        int loopedValue = (value - min) % range;
        if (loopedValue < 0)
            loopedValue += range;

        return min + loopedValue;
    }
    void ShowScoredGalaxy(int scoreSum, int shotsSum, int parSum, bool starCollected, int prevGalaxyScore)
    {
        Start();

        ZoomGalaxy(true, true);
        galaxySummaries[focusedGalaxy].DisableSummary();
        menuAnim.Play("DisableMenu");
        EnableAndSetInfoCard(true, galaxyList[focusedGalaxy]);
        bool[] starScore = new bool[]
        {
            true,
            scoreSum <= 0,
            scoreSum <= -3 || shotsSum == 5,
            starCollected,
            shotsSum == 5
        };
        StartCoroutine(AnimateStars(starScore, prevGalaxyScore));
        galaxyList = GameManager.LevelManager.currentProceduralUniverse;
    }
    IEnumerator AnimateStars(bool[] stars, int prevScore)
    {
        playGalaxyBtn.interactable = false;
        infoBackBtn.interactable = false;
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i]) 
            {
                starImgs[i].transform.localScale = Vector3.zero;
            }
        }
        yield return new WaitForSecondsRealtime(0.5f);
        int realIteration = 0;
        for (int i = 0; i < stars.Length; i++)
        {
            if (!stars[i]) { continue; }
            GameManager.SFXManager.PlaySFX(starAudios[realIteration]);
            float timeTracker = 0;
            while (timeTracker < starScaleDuration)
            {
                starImgs[i].transform.localScale = Vector3.one * starScaleCurve.Evaluate((timeTracker / starScaleDuration));
                timeTracker += Time.deltaTime;
                yield return null;
            }
            realIteration++;
        }
        if(realIteration == 5)
        {
            GameManager.SFXManager.PlaySFX(fiveStarAudio);
            yield return new WaitForSecondsRealtime(0.5f);
        }

        //COIN FEEDBACK
        //Coins going up and down with pitch
        int totalCoins = Mathf.RoundToInt(GameManager.LevelManager._DifficultyStarMult[GameManager.LevelManager.currentUniverse.relativeDifficulty] * Mathf.Max(realIteration - prevScore, 0));

        if(totalCoins > 0)
        {
            int displayCoins = 0;
            int consecutiveCoinMoves = 0;
            float coinTracker = 0;
            int prevDisplay = 0;
            infoCardAnim.Play("ShowCoins");
            do
            {
                coinTracker = Mathf.Lerp(coinTracker, totalCoins, coinFillSpeed);
                displayCoins = Mathf.RoundToInt(coinTracker);
                coinsCollectionText.text = "+" + displayCoins.ToString();

                if(prevDisplay != displayCoins)
                {
                    GameManager.SFXManager.PlaySFX(singleCoinAudio, galaxySrc, singleCoinAudio.inherentVol);
                    galaxySrc.pitch = 1 + consecutiveCoinMoves * 0.01f;
                }

                prevDisplay = displayCoins;
                consecutiveCoinMoves += 1;
                yield return new WaitForFixedUpdate();
            } while (displayCoins < totalCoins);

            yield return new WaitForSecondsRealtime(0.25f);

            infoCardAnim.Play("CollectCoins");
            GameManager.SFXManager.PlaySFX(cashoutAudio, galaxySrc, cashoutAudio.inherentVol);
        }
        yield return new WaitForSecondsRealtime(infoCardAnim.GetCurrentAnimatorStateInfo(1).length);
        coinsCollectionText.transform.parent.gameObject.SetActive(false);


        yield return new WaitForSecondsRealtime(0.25f);
        ZoomGalaxy(false, false);

        playGalaxyBtn.interactable = true;
        infoBackBtn.interactable = true;
    }
    private void OnEnable()
    {
        PointCardManager.onGalaxyScore += ShowScoredGalaxy;
    }
    private void OnDisable()
    {
        PointCardManager.onGalaxyScore -= ShowScoredGalaxy;
    }
    #endregion
}

// public class GalaxyMenu : MonoBehaviour
// {
//     [SerializeField] ColorSchemeStaticReference.PlanetType[] galaxyTypes;
//     [SerializeField] Animator galaxy1, galaxy2;
//     [SerializeField] GalaxyGenerator gGen1, gGen2;
//     [SerializeField] TextMeshProUGUI nameText;
//     [SerializeField] Button right, left;
//     LevelManager.GalaxyParams[] proceduralGalaxyList;
//     EventSystem eventSystem;
//     //List<LineRenderer> hexagons = new List<LineRenderer>();
//     int currentGalaxy;
//     bool swap;
//     // Start is called before the first frame update
//     void Start()
//     {
//         eventSystem = FindObjectOfType<EventSystem>();
//         currentGalaxy = 0;
//         swap = false;
//         galaxy1.Play("GalaxyDisplay");
//         galaxy2.Play("GalaxyIdle");
//         nameText.text = gGen1.SetScheme(galaxyTypes[currentGalaxy]).name;
//         right.onClick.AddListener(RightGalaxy);
//         left.onClick.AddListener(LeftGalaxy);

//         if(GameManager.LevelManager.procedural)
//         {
//             CreateProceduralGalaxies();
//             LevelManager.GalaxyParams galaxyParams = proceduralGalaxyList[0];
//             gGen1.SetProceduralScheme(galaxyParams);
//             nameText.text = galaxyParams.name;
//         }
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         if(eventSystem.currentSelectedGameObject != null) { eventSystem.SetSelectedGameObject(null); }

//     }
//     void RightGalaxy()
//     {
//         currentGalaxy += 1;
//         currentGalaxy %= !GameManager.LevelManager.procedural ? galaxyTypes.Length : proceduralGalaxyList.Length;
//         Animator x = swap ? galaxy2 : galaxy1; //Enter galaxy
//         Animator y = swap ? galaxy1 : galaxy2; //Exit galaxy
//         GalaxyGenerator gen = swap ? gGen1 : gGen2;
//         if(GameManager.LevelManager.procedural)
//         {
//             LevelManager.GalaxyParams galaxyParams = proceduralGalaxyList[currentGalaxy];
//             gen.SetProceduralScheme(galaxyParams);
//             nameText.text = galaxyParams.name;
//         }
//         else
//         {
//             Galaxy.GalaxyScheme scheme = gen.SetScheme(galaxyTypes[currentGalaxy]);
//             nameText.text = scheme.name;
//         }

//         y.Play("GalaxyEnter");
//         x.Play("GalaxyExit");
//         swap = !swap;
//     }
//     void LeftGalaxy()
//     {
//         currentGalaxy += -1;
//         if(!GameManager.LevelManager.procedural && currentGalaxy < 0) { currentGalaxy = galaxyTypes.Length-1; }
//         if(GameManager.LevelManager.procedural && currentGalaxy < 0) { currentGalaxy = proceduralGalaxyList.Length-1; }
//         Animator x = swap ? galaxy2 : galaxy1; //Exit galaxy
//         Animator y = swap ? galaxy1 : galaxy2; //Enter galaxy
//         GalaxyGenerator gen = swap ? gGen1 : gGen2;
//         if (GameManager.LevelManager.procedural)
//         {
//             LevelManager.GalaxyParams galaxyParams = proceduralGalaxyList[currentGalaxy];
//             gen.SetProceduralScheme(galaxyParams);
//             nameText.text = galaxyParams.name;
//         }
//         else
//         {
//             Galaxy.GalaxyScheme scheme = gen.SetScheme(galaxyTypes[currentGalaxy]);
//             nameText.text = scheme.name;
//         }

//         x.Play("GalaxyEnterR");
//         y.Play("GalaxyExitR");
//         swap = !swap;
//     }
//     void CreateProceduralGalaxies()
//     {
//         string lowerCase = "abcdefghijklmnopqrstuvwxyz";
//         string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

//         string numbers = "1234567890";
//         string[] startingLetters = new string[6]{
//             "NGC",
//             "IC",
//             "CGCG",
//             "UGC",
//             "PGC",
//             "LEDA",
//         };

//         LevelManager.UniverseParams universe = GameManager.LevelManager.currentUniverse; //retrieves current universe
//         UnityEngine.Random.InitState(universe.seed);
//         LevelManager.GalaxyParams[] galaxies = new LevelManager.GalaxyParams[universe.numGalaxies]; //initialize galaxy array
//         for (int i = 0; i < galaxies.Length; i++) //for each galaxy index, create one
//         {
//             UnityEngine.Random.InitState(universe.seed + i); //different random values
//             //random name, temporary
//             string name = "";

//             int type = UnityEngine.Random.Range(0, 2);

//             if(type == 0){
//                 int numParts = UnityEngine.Random.Range(2,3);
//                 for (int c = 0; c < numParts; c++) 
//                 {
//                     int roll = UnityEngine.Random.Range(0,160);
//                     if (roll < universe.levelStatic.GalaxyWords.Length){
//                         string addend = universe.levelStatic.GalaxyWords[UnityEngine.Random.Range(0, universe.levelStatic.GalaxyWords.Length)];
//                         if(c == 0){
//                             if (lowerCase.Contains(addend[0])){
//                                 int index = lowerCase.IndexOf(addend[0]);
//                                 char[] addendChars = addend.ToCharArray();
//                                 addendChars[0] = upperCase[index];
//                                 addend = new string(addendChars);
//                             }
//                         }
//                         name += addend;
//                         name += " ";
//                     }
//                     else if (roll > universe.levelStatic.GalaxyWords.Length && roll < 145){
//                         int numNumbers = UnityEngine.Random.Range(3,6);
//                         for (int k = 0; k < numNumbers; k++){
//                             name += numbers[UnityEngine.Random.Range(0,numbers.Length)];
//                         }
//                         name += " ";
//                     }
//                     else{       
//                         int numChars = UnityEngine.Random.Range(3,6);
//                         for (int k = 0; k < numChars; k++){
//                             bool letter = UnityEngine.Random.Range(0,2) == 1;
//                             if(letter){
//                                 name += upperCase[UnityEngine.Random.Range(0,upperCase.Length)];
//                             }
//                             else{
//                                 name += numbers[UnityEngine.Random.Range(0,numbers.Length)];
//                             }
//                         }
//                         name += " ";
//                     }

//                 }
//             }
//             if (type == 1){
//                 name += startingLetters[UnityEngine.Random.Range(0,startingLetters.Length)];
//                 name += " ";

//                 int numNumbers = UnityEngine.Random.Range(3,7);
//                 for (int c = 0; c < numNumbers; c++){
//                     name += numbers[UnityEngine.Random.Range(0,numbers.Length)];
//                 }
//             }


//             //random planet types
//             ColorSchemeStaticReference.PlanetType[] possibleTypes = new ColorSchemeStaticReference.PlanetType[UnityEngine.Random.Range(1,3)];
//             for (int t = 0; t < possibleTypes.Length; t++) 
//             {
//                 possibleTypes[t] = galaxyTypes[UnityEngine.Random.Range(0, galaxyTypes.Length)];
//             }
//             galaxies[i] = new LevelManager.GalaxyParams(name, UnityEngine.Random.Range(-2147483647, 2147483647), universe.relativeDifficulty, possibleTypes, universe.levelStatic); //setting galaxy at index
//         }
//         LevelManager.setProceduralGalaxies(galaxies);
//         proceduralGalaxyList = galaxies;
//     }
// }

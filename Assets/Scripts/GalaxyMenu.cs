using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
    [SerializeField] float transitionDuration = 0.5f;
    [SerializeField] AnimationCurve transitionCurve;
    [Header("Main UI")]
    [SerializeField] Button rightBtn; //move buttons
    [SerializeField] Button leftBtn; //move buttons
    [SerializeField] Button zoomBtn; //move buttons
    [SerializeField] GameObject dotButton; //created in create galaxy funciton, one dot per galaxy
    [SerializeField] Vector3 dotMiddle;
    [SerializeField] float dotDistanceInterval;
    [SerializeField] Canvas galaxyMenuCanvas;
    [Header("Galaxy Obj")]
    [SerializeField] float distInterval; //distance between galaxies
    [SerializeField] float sizeScalar; //scales galaxy size
    [SerializeField] Transform galaxyObjParent; //parent of galaxy controllers
    [SerializeField] GameObject galaxyObj; //prefab for instantiation
    [Header("Miscellaneous")]
    [SerializeField] GalaxyInfoType[] galaxyTypes; //possible types for galaxies
    [SerializeField] SpecialType[] allSpecialTypes;
    [SerializeField] GalaxyData[] possibleGalaxyData; //possible galaxy data
    [Header("Info Menu")]
    [SerializeField] Animator infoCardAnim;
    [SerializeField] TextMeshProUGUI galaxyTitle;
    [SerializeField] Button playGalaxyBtn, infoBackBtn;
    [SerializeField] TextMeshProUGUI seedNum;
    [SerializeField] TextMeshProUGUI difficultyNum;
    [SerializeField] Slider difficultySlider;
    [SerializeField] Gradient difficultyGradient;
    [SerializeField] TextMeshProUGUI astroBiomeTxt;
    [SerializeField] Image[] iconImgs;
    [SerializeField] SpecialObjectIcon[] iconSprites;
    Image[] dotSelectedImgs;
    Vector3 cameraOrigin;
    Transform mainCam;
    GalaxyController[] galaxyCons; //array of controllers in the scene
    int focusedGalaxy; //index of focused galaxy
    bool isMoving; //whether it is in transition
    bool zoomedIn;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main.transform;
        cameraOrigin = mainCam.position;
        if(GameManager.LevelManager.procedural)
        {
            CreateProceduralGalaxies();
        }
        CreateControllers();
        EnableDisableGalaxies(false);
        rightBtn.onClick.AddListener(() => MoveButton(1));
        leftBtn.onClick.AddListener(() => MoveButton(-1));
        zoomBtn.onClick.AddListener(() => ZoomButton(true));
        playGalaxyBtn.onClick.AddListener(() => PlayGalaxy());
        infoBackBtn.onClick.AddListener(() => InfoCardBackButton());
    }
    void Update()
    {
        MenuInput();
    }
    #region Buttons
    void MoveButton(int dir)
    {
        if(!isMoving)
        {
            StartCoroutine(MoveGalaxies(dir));
        }
    }
    void ZoomButton(bool zoomIn)
    {
        if (!isMoving)
        {
            StartCoroutine(ZoomGalaxy(zoomIn));
        }
    }
    void InfoCardBackButton()
    {
        ZoomButton(false);
    }
    void PlayGalaxy()
    {
        SetProceduralGalaxy(galaxyList[focusedGalaxy]);
        SceneManager.LoadScene("LevelGen");
    }
    void DotButton(int dotIndex)
    {
        int movement = dotIndex - focusedGalaxy;
        StartCoroutine(MoveGalaxies(movement));
    }
    void MenuInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveButton(1);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveButton(-1);
        }
        if (Input.GetKeyDown(KeyCode.Escape) && zoomedIn)
        {
            ZoomButton(false);
        }
        if (Input.GetKeyDown(KeyCode.Space) && !zoomedIn)
        {
            ZoomButton(true);
        }
    }
    void SetProceduralGalaxy(LevelManager.GalaxyParams gp)
    {
        LevelManager.LevelParams[] galaxyLevelParams = new LevelManager.LevelParams[5];
        string[] galaxyLevels = new string[5];
        for (int i = 0; i < 5; i++) //Creating levels as level params
        {
            UnityEngine.Random.InitState(i * 120523 + gp.seed);
            int levelSeed = UnityEngine.Random.Range(-2147483647, 2147483647);
            int levelDifficulty = Mathf.Clamp(gp.relativeDifficulty + UnityEngine.Random.Range(-1, 1), 1, 10);
            LevelManager.LevelParams systemParams = new LevelManager.LevelParams(levelSeed, levelDifficulty, gp.levelStatic.difficultyPars[levelDifficulty - 1], gp.possibleTypes, gp.possibleSpecial, gp.levelStatic);

            galaxyLevelParams[i] = systemParams;
            galaxyLevels[i] = "LevelGen";
        }
        initProceduralSystem?.Invoke(galaxyLevels, galaxyLevelParams, gp.galaxyData);
        LevelSettingsManager.onLevelSettingChange?.Invoke(galaxyLevelParams[0]);
        GameManager.LevelSettingsManager.UIMatchParams();
    }//Sets the procedural galaxy in level manager through event
    #endregion
    #region Transitions
    void EnableAndSetInfoCard(bool enable, LevelManager.GalaxyParams gp)
    {
        if (enable)
        {
            infoCardAnim.Play("ShowInfoCard");
            galaxyTitle.text = gp.name;
            seedNum.text = gp.seed.ToString();
            difficultyNum.text = gp.relativeDifficulty.ToString();
            difficultyNum.color = difficultyGradient.Evaluate(gp.relativeDifficulty / difficultySlider.maxValue);
            difficultySlider.value = gp.relativeDifficulty;
            ColorBlock cB = difficultySlider.colors;
            cB.disabledColor = difficultyNum.color;
            difficultySlider.colors = cB;
            astroBiomeTxt.text = gp.possibleTypes[0].ToString();
            astroBiomeTxt.color = gp.galaxyData.mainColor;
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
        }
        else
        {
            infoCardAnim.Play("HideInfoCard");
        }
    }
    IEnumerator ZoomGalaxy(bool zoomIn)
    {
        isMoving = true;
        zoomedIn = zoomIn;
        galaxyMenuCanvas.enabled = !zoomIn;
        Vector3 galaxyTarget = zoomIn ? zoomGalaxyPos : focusPosition;
        Vector3 cameraTarget = zoomIn ? zoomCameraPos : cameraOrigin;

        if(!zoomIn)
        {
            EnableAndSetInfoCard(zoomIn, galaxyList[focusedGalaxy]);
            yield return new WaitForSeconds(infoCardAnim.GetCurrentAnimatorStateInfo(0).length/2);
        }

        float timeOfTransition = 0f; //timer
        while(timeOfTransition < transitionDuration)
        {
            float progress = timeOfTransition / transitionDuration;
            mainCam.position = Vector3.Lerp(mainCam.position, cameraTarget, transitionCurve.Evaluate(progress));
            galaxyCons[focusedGalaxy].transform.position = Vector3.Lerp(galaxyCons[focusedGalaxy].transform.position , galaxyTarget, transitionCurve.Evaluate(progress));

            timeOfTransition += Time.deltaTime;
            yield return null;
        }

        isMoving = false;
        if (zoomIn)
        {
            EnableAndSetInfoCard(zoomIn, galaxyList[focusedGalaxy]);
            yield return new WaitForSeconds(infoCardAnim.GetCurrentAnimatorStateInfo(0).length);
        }
    }
    IEnumerator MoveGalaxies(int dir)
    {
        isMoving = true;
        bool loopMove = focusedGalaxy + dir < 0 || focusedGalaxy + dir >= galaxyCons.Length ? true : false;
        focusedGalaxy = LoopValue(focusedGalaxy+dir, 0, galaxyCons.Length);
        Vector3[] targetPositions = GalaxyPositions(galaxyCons.Length, focusedGalaxy);
        float timeOfTransition = 0f; //timer
        while(timeOfTransition < transitionDuration) //loop to move galaxies in a certain timeframe
        {
            if (timeOfTransition > transitionDuration / 3) { EnableDisableGalaxies(loopMove); }
            for (int i = 0; i < galaxyCons.Length; i++) //move each galaxy
            {
                galaxyCons[i].transform.position = Vector3.Lerp(galaxyCons[i].transform.position, targetPositions[i], transitionCurve.Evaluate(timeOfTransition/transitionDuration));
            }
            timeOfTransition += Time.deltaTime;
            yield return null;
        }
        isMoving = false;
        EnableDisableGalaxies(false);
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
                targetPositions[i] = focusPosition;
            }
        }
        return targetPositions;
    }
    void EnableDisableGalaxies(bool enableAll) //enables or disables galaxies, onscreen or offscreen
    {
        for (int i = 0; i < galaxyCons.Length; i++)
        {
            if(Mathf.Abs(i-focusedGalaxy)<=1 || enableAll)
            {
                galaxyCons[i].gameObject.SetActive(true);
            }
            else
            {
                galaxyCons[i].gameObject.SetActive(false);
            }
            if (i != focusedGalaxy) { dotSelectedImgs[i].enabled = false; }
            else { dotSelectedImgs[i].enabled = true; }
        }
    }
    void CreateControllers() //Instantiates all galaxies
    {
        galaxyCons = new GalaxyController[galaxyList.Length];
        Vector3[] positions = GalaxyPositions(galaxyList.Length, focusedGalaxy);
        for (int i = 0; i < galaxyList.Length; i++) //dots
        {
            UnityEngine.Random.InitState(galaxyList[i].seed);
            galaxyCons[i] = Instantiate(galaxyObj, galaxyObjParent).GetComponent<GalaxyController>();
            galaxyCons[i].transform.localScale = galaxyCons[i].transform.localScale * sizeScalar;
            galaxyCons[i].transform.rotation = Quaternion.Euler(Vector3.Lerp(minRotation, maxRotation,UnityEngine.Random.value));
            galaxyCons[i].transform.position = positions[i];
            galaxyCons[i].SetProceduralScheme(galaxyList[i]);
        }
        float halfLength = ((galaxyList.Length - 1) / 2) * dotDistanceInterval;
        dotSelectedImgs = new Image[galaxyList.Length];
        for (int i = 0; i < galaxyList.Length; i++) //dot button creation
        {
            RectTransform t = Instantiate(dotButton, transform).GetComponent<RectTransform>();
            t.localPosition = new Vector3(dotMiddle.x - halfLength + dotDistanceInterval * i, dotMiddle.y, dotMiddle.z);
            int callIndex = i;
            t.gameObject.GetComponent<Button>().onClick.AddListener(() => DotButton(callIndex));
            dotSelectedImgs[i] = t.GetChild(0).GetComponent<Image>();
            if(i != focusedGalaxy) { dotSelectedImgs[i].enabled = false; }
            else { dotSelectedImgs[i].enabled = true; }
        }
    }
    #endregion
    #region Utility
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
        float galaxyDifficultyStep = (float)universe.relativeDifficulty / (float)universe.numGalaxies;
        float galaxyDifficultyTracker = galaxyDifficultyStep;
        for (int i = 0; i < galaxies.Length; i++) //for each galaxy index, create one
        {
            UnityEngine.Random.InitState(universe.seed + i); //different random values

            //random name, temporary
            #region Random Name
            string name = "";
            int type = UnityEngine.Random.Range(0, 2); //nametype
            if(type == 0){
                int numParts = UnityEngine.Random.Range(2,3);
                for (int c = 0; c < numParts; c++) 
                {
                    int roll = UnityEngine.Random.Range(0,160);
                    if (roll < universe.levelStatic.GalaxyWords.Length){
                        string addend = universe.levelStatic.GalaxyWords[UnityEngine.Random.Range(0, universe.levelStatic.GalaxyWords.Length)];
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
                    else if (roll > universe.levelStatic.GalaxyWords.Length && roll < 145){
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
            GalaxyData gData = null;
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
                for (int x = 0; x < possibleGalaxyData.Length; x++)
                {
                    if (possibleGalaxyData[x].type == possibleTypes[t]) { gData = possibleGalaxyData[x]; }
                }
            }

            galaxyDifficultyTracker += galaxyDifficultyStep;
            galaxies[i] = new LevelManager.GalaxyParams(name, UnityEngine.Random.Range(-2147483647, 2147483647), Mathf.Clamp(Mathf.CeilToInt(galaxyDifficultyTracker),0,10), gData, possibleTypes, specialTypes, universe.levelStatic); //setting galaxy at index
        }
        LevelManager.setProceduralGalaxies(galaxies);
        galaxyList = galaxies;
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

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GalaxyMenu : MonoBehaviour
{

    [SerializeField] LevelManager.GalaxyParams[] galaxyList;
    [SerializeField] Vector3[] focusPositions; //offscreen left, left, focused centre, right, offscreen right
    [SerializeField] int focusedGalaxy;
    [SerializeField] float sizeScalar;
    [SerializeField] Button right, left;
    [SerializeField] Transform galaxyObjParent;
    [SerializeField] GameObject galaxyObj;
    [SerializeField] ColorSchemeStaticReference.PlanetType[] galaxyTypes;
    GalaxyController[] galaxyCons;
    EventSystem eventSystem;
    //List<LineRenderer> hexagons = new List<LineRenderer>();
    int currentGalaxy;
    float transitionDuration = 0.5f;
    bool swap, isMoving;
    // Start is called before the first frame update
    void Start()
    {
        CreateControllers();
        right.onClick.AddListener(() => StartCoroutine(MoveGalaxies(-1)));
        left.onClick.AddListener(() => StartCoroutine(MoveGalaxies(1)));
    }
    void Update()
    {
        SetGalaxyPositions();
    }
    IEnumerator MoveGalaxies(int direction)
    {
        if (isMoving) yield break;

        isMoving = true;
        int previousFocusedGalaxyIndex = focusedGalaxy;
        focusedGalaxy = LoopValue(focusedGalaxy + direction, 0, galaxyCons.Length);

        // float elapsedTime = 0f;
        // while (elapsedTime < transitionDuration)
        // {
        //     elapsedTime += Time.deltaTime;
        //     float t = Mathf.Clamp01(elapsedTime / transitionDuration);

        //     for (int i = 0; i < galaxyCons.Length; i++)
        //     {
        //         int targetIndex = LoopValue(previousFocusedGalaxyIndex - 2 + i + direction, 0, galaxyCons.Length);
        //         galaxyCons[i].transform.position = Vector3.Lerp(
        //             focusPositions[LoopValue(i - direction, 0, focusPositions.Length)],
        //             focusPositions[i],
        //             t
        //         );
        //     }

        //     yield return null;
        // }

        SetGalaxyPositions();
        isMoving = false;
    }

    void SetGalaxyPositions()
    {
        for (int i = 0; i < focusPositions.Length; i++)
        {
            int galaxyIndex = LoopValue(focusedGalaxy - 2 + i, 0, galaxyCons.Length);
            galaxyCons[galaxyIndex].transform.position = focusPositions[i];
        }
    }

    void CreateControllers()
    {
        galaxyCons = new GalaxyController[galaxyList.Length];
        for (int i = 0; i < galaxyList.Length; i++)
        {
            galaxyCons[i] = Instantiate(galaxyObj, galaxyObjParent).GetComponent<GalaxyController>();
            galaxyCons[i].transform.localScale = galaxyCons[i].transform.localScale * sizeScalar;
            galaxyCons[i].SetProceduralScheme(galaxyList[i]);
        }
        SetGalaxyPositions();
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
        for (int i = 0; i < galaxies.Length; i++) //for each galaxy index, create one
        {
            UnityEngine.Random.InitState(universe.seed + i); //different random values
            //random name, temporary
            string name = "";

            int type = UnityEngine.Random.Range(0, 2);

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


            //random planet types
            ColorSchemeStaticReference.PlanetType[] possibleTypes = new ColorSchemeStaticReference.PlanetType[UnityEngine.Random.Range(1,3)];
            for (int t = 0; t < possibleTypes.Length; t++) 
            {
                possibleTypes[t] = galaxyTypes[UnityEngine.Random.Range(0, galaxyTypes.Length)];
            }
            galaxies[i] = new LevelManager.GalaxyParams(name, UnityEngine.Random.Range(-2147483647, 2147483647), universe.relativeDifficulty, possibleTypes, universe.levelStatic); //setting galaxy at index
        }
        LevelManager.setProceduralGalaxies(galaxies);
        galaxyList = galaxies;
    }
    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < focusPositions.Length; i++)
        {
            Gizmos.DrawCube(focusPositions[i], Vector3.one);
        }
    }
    int LoopValue(int value, int min, int max)
    {
        int range = max - min;
        if (range <= 0) return min; // Handle edge case if min and max are equal or max is less than min

        int loopedValue = (value - min) % range;
        if (loopedValue < 0)
            loopedValue += range;

        return min + loopedValue;
    }
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

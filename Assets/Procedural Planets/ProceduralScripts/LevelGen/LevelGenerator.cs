using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System;
using Unity.VisualScripting;

public class LevelGenerator : MonoBehaviour
{
        /*Level Gen settings
    0: minimum number of planets
    1: maximum number of planets
    2: minimum radius for each poisson point
    3: maximum radius for each poisson point
    4: bias towards or against planet size
    5: IGNORE
    6: x dimension of size of level
    7: y dimension of size of level
    8: number of poisson disk samples before rejecting the point as a spawner
    9: offset scale for determining random values of planet
    */
    public int seed;
    //Level gen params
    [SerializeField]
    float minPlanets, maxPlanets, minSize, maxSize, sizeBias, sizeScale, xRegionSize, yRegionSize, samplesBeforeRejection, offsetScale;
    [SerializeField]
    GameObject planetGeneratorPrefab, forcefieldPrefab, audioSrcPrefab, ballPrefab;
    [SerializeField]
    float planetScale;
    [SerializeField]
    ColorSchemeStaticReference.PlanetType[] possiblePlanetTypes;

    [Serializable]
    public struct Planet{
        public GameObject planet;
        public ColorSchemeStaticReference.PlanetType planetType;
        public Planet(GameObject planet, ColorSchemeStaticReference.PlanetType planetType){
            this.planet = planet;
            this.planetType = planetType;
        }
    }

    public delegate void LevelInit();
    public static LevelInit levelInit;

    void Start()
    {
        float[] levelGenSettings = new float[10]{
            minPlanets,
            maxPlanets,
            minSize,
            maxSize,
            sizeBias,
            sizeScale,
            xRegionSize - 2 * maxSize,
            yRegionSize - 2 * maxSize,
            samplesBeforeRejection,
            offsetScale
        };
        ProceduralLevelGeneration.Level level = ProceduralLevelGeneration.CalculateLevel(seed, possiblePlanetTypes, levelGenSettings);
        Generatelevel(level);
    }
    void Generatelevel(ProceduralLevelGeneration.Level level){
        FullPlanetGeneration generator = Instantiate(planetGeneratorPrefab).GetComponent<FullPlanetGeneration>();
        Planet[] planetList = new Planet[level.numPlanets];
        Instantiate(audioSrcPrefab);
        GameObject env = GameObject.FindGameObjectWithTag("Environment");
        for (int i = 0; i < level.numPlanets; i++){
            Vector3 levelSize = new Vector3(level.levelSize.x, level.levelSize.y, 0f);
            planetList[i] = GeneratePlanet(generator, level.planetSizes[i], i, level.planetPoints[i], level.planetSeeds[i], levelSize, level.planetTypes[i]);
            print(level.planetSizes[i]);
            GameObject forcefield = GenerateForcefield(planetList[i]);
            planetList[i].planet.SetActive(true);
            forcefield.transform.parent = env.transform;
        }
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<ShootBall>().planetTransform = env.transform;   
        player.GetComponent<ShootBall>().Init();
    }


    GameObject GenerateForcefield(Planet planet){
        GameObject forcefield = this.gameObject;
        if (planet.planetType != ColorSchemeStaticReference.PlanetType.blackhole || planet.planetType != ColorSchemeStaticReference.PlanetType.star){  
            forcefield = Instantiate(forcefieldPrefab);

            forcefield.GetComponent<ForceField>().planet = planet.planet.transform;
            forcefield.GetComponent<ForceField>().enabled = true;
            
        }
        return forcefield;
    }

    Planet GeneratePlanet(FullPlanetGeneration generator, float size, int index, Vector3 position, Vector3 seed, Vector3 levelSize, ColorSchemeStaticReference.PlanetType planetType){
        generator.RandomizeColorScheme(seed, planetType, index);
        generator.ApplyColorScheme();
        generator.ClearAllTrees();
        generator.UpdateAtmosphere();
        generator.UpdateOcean();
        generator.UpdateTrees();
        generator.UpdateData();

        GameObject newPlanet = ConvertToImage(generator.gameObject, "ImagingScene" + seed.x);
        newPlanet.transform.position = position - levelSize/2;
        newPlanet.transform.localScale = Vector3.one * size * planetScale * 0.2f;
        return new Planet (newPlanet, ColorSchemeStaticReference.PlanetType.blackhole);
    }

    GameObject ConvertToImage(GameObject selectedObj, string sceneName)
    {
        MeshRenderer oceanRend = selectedObj.transform.Find("Ocean").GetComponent<MeshRenderer>();
        bool activeOcean = oceanRend.gameObject.activeInHierarchy;

        Material oceanMat = oceanRend.material;
        Material cullMat = new Material(Shader.Find("Custom/OceanInvisMask"));
        oceanRend.sharedMaterial = cullMat;

        Camera cam = Instantiate(Camera.main).GetComponent<Camera>();
        cam.orthographicSize = 1.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0, 0, 0, 0);
        cam.cullingMask = LayerMask.GetMask("Planet Front Capture");


        Scene currentScene = SceneManager.GetActiveScene();
        Scene emptyScene = SceneManager.CreateScene(sceneName);
        SceneManager.MoveGameObjectToScene(cam.gameObject, emptyScene);
        SceneManager.MoveGameObjectToScene(selectedObj, emptyScene);
        SceneManager.SetActiveScene(currentScene);

        //Front Texture
        RenderTexture frontTexture = new RenderTexture(1000, 1000, 0);
        cam.targetTexture = frontTexture;
        cam.Render();

        Sprite frontSprite = ConvertRenderTextureToSprite(frontTexture);

        //Ocean disabled
        oceanRend.gameObject.SetActive(false);

        //Back Texture
        cam.cullingMask = LayerMask.GetMask("Planet Back Capture", "Planet Front Capture");

        RenderTexture backTexture = new RenderTexture(1000, 1000, 0);
        cam.targetTexture = backTexture;
        cam.Render();

        Sprite backSprite = ConvertRenderTextureToSprite(backTexture);
        backSprite.name = "dsad";

        //Creation of prefab object
        
        //Parent planet
        GameObject planetParent = new GameObject("Planet Parent");
        planetParent.transform.position = Vector3.zero;
        planetParent.transform.localScale = Vector3.one * 0.3f;
        SortingGroup sr = planetParent.AddComponent<SortingGroup>();
        sr.sortingLayerName = "Planet Sprites";
        PlanetDataHolder pd = planetParent.AddComponent<PlanetDataHolder>();
        PlanetDataHolder cd = selectedObj.GetComponent<PlanetDataHolder>();
        pd.biomeA =  cd.biomeA;
        pd.biomeB =  cd.biomeB;
        pd.oBiomeA = cd.oBiomeA;
        pd.oBiomeB = cd.oBiomeB;
        pd.aBiomeA = cd.aBiomeA;
        pd.aBiomeB = cd.aBiomeB;

        //Front sprite
        GameObject frontObject = new GameObject("frontObject");
        frontObject.transform.parent = planetParent.transform;
        frontObject.transform.localScale = Vector3.one;
        SpriteRenderer frontSR = frontObject.AddComponent<SpriteRenderer>();
        frontSR.sprite = frontSprite;
        frontSR.sortingOrder = 1;

        //Back sprite
        GameObject backObject = new GameObject("backObject");
        backObject.transform.parent = planetParent.transform;
        backObject.transform.position += new Vector3(0, 0, 2);
        backObject.transform.localScale = Vector3.one;
        SpriteRenderer backSR = backObject.AddComponent<SpriteRenderer>();
        backSR.sprite = backSprite;

        //ocean
        GameObject ocean = Instantiate(oceanRend.gameObject, planetParent.transform);
        ocean.GetComponent<MeshRenderer>().material = oceanMat;
        ocean.transform.position += new Vector3(0, 0, 2);
        ocean.transform.localScale = Vector3.one * 6.9f;
        ocean.SetActive(activeOcean);

        //Atmospheres
        GameObject[] atmospheres = { selectedObj.transform.GetChild(1).gameObject, selectedObj.transform.GetChild(2).gameObject, selectedObj.transform.GetChild(3).gameObject };
        for (int i = 0; i < 3; i++)
        {
            GameObject instaAtmosphere = Instantiate(atmospheres[i], planetParent.transform);
            instaAtmosphere.AddComponent<AtmosphereHandler>();
            float depth = i == 2 ? 0 : 2;
            instaAtmosphere.transform.localScale = Vector3.one;
            instaAtmosphere.transform.position += new Vector3(0, 0, depth);
        }

        oceanRend.sharedMaterial = oceanMat;
        oceanRend.gameObject.SetActive(true);

        Destroy(cam.gameObject);
        SceneManager.UnloadSceneAsync(emptyScene);
        planetParent.SetActive(false);
        return planetParent;
    }
    public static Sprite ConvertRenderTextureToSprite(RenderTexture renderTexture)
    {
        Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        return sprite;
    }

    void OnDrawGizmos(){
        if (!Application.isPlaying){
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(Vector2.zero, new Vector2(xRegionSize - maxSize, yRegionSize - maxSize));
        }
    }

}

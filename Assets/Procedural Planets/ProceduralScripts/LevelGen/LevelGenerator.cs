using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.Linq.Expressions;

public class LevelGenerator : MonoBehaviour
{

    /*Level Gen params settings
    0: minimum number of planets
    1: maximum number of planets
    2: minimum size for each planet
    3: maximum size for each planet
    4: bias towards or against planet size
    5: x dimension of size of level
    6: y dimension of size of level
    7: number of poisson disk samples before rejecting the point as a spawner
    8: offset scale for determining random values of planet
    */
    public int seed;
    //Level gen params
    [SerializeField]
    float minPlanets, maxPlanets, minSize, maxSize, sizeBias, xRegionSize, yRegionSize, samplesBeforeRejection, offsetScale;
    [SerializeField]
    GameObject planetGeneratorPrefab;
    [SerializeField]
    float planetScale;

    void Start()
    {
        float[] levelGenSettings = new float[9]{
            minPlanets,
            maxPlanets,
            minSize,
            maxSize,
            sizeBias,
            xRegionSize,
            yRegionSize,
            samplesBeforeRejection,
            offsetScale
        };
        ProceduralLevelGeneration.Level level = ProceduralLevelGeneration.CalculateLevel(seed, levelGenSettings);
        Generatelevel(level);
    }
    void Generatelevel(ProceduralLevelGeneration.Level level){
        for (int i = 0; i < level.numPlanets; i++){
            Vector3 levelSize = new Vector3(level.levelSize.x, level.levelSize.y, 0f);
            GeneratePlanet(1f, level.planetPoints[i], level.planetSeeds[i], levelSize);
        }
    }

    void GeneratePlanet(float size, Vector3 position, Vector3 seed, Vector3 levelSize){
        GameObject generator = Instantiate(planetGeneratorPrefab);
        generator.GetComponent<FullPlanetGeneration>().RandomizeTerrainScheme(seed);
        generator.GetComponent<FullPlanetGeneration>().RandomizeColorScheme(seed);
        generator.GetComponent<FullPlanetGeneration>().ClearAllTrees();

        GameObject newPlanet = ConvertToImage(generator, "ImagingScene" + seed.x);
        newPlanet.transform.position = position - levelSize/2;
        newPlanet.transform.localScale = Vector3.one * size * planetScale;
        Destroy(generator);
    }

    GameObject ConvertToImage(GameObject selectedObj, string sceneName)
    {
        MeshRenderer oceanRend = selectedObj.transform.Find("Ocean").GetComponent<MeshRenderer>();

        Material oceanMat = oceanRend.material;
        Material cullMat = new Material(Shader.Find("Custom/OceanInvisMask"));
        oceanRend.sharedMaterial = cullMat;

        Camera cam = Instantiate(Camera.main).GetComponent<Camera>();
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
        ocean.SetActive(true);

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
}

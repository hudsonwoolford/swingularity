using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkyboxManager : MonoBehaviour
{
    public float _Rotation;

    [SerializeField] private Material skyboxMat;  // The original material (reference to asset)
    private Material runtimeSkyboxMat;  // A runtime instance of the material
    float lastRotation;
    void Awake()
    {
        // Create a runtime instance of the material (not modifying the original asset)
        runtimeSkyboxMat = new Material(skyboxMat);
        RenderSettings.skybox = runtimeSkyboxMat;
    }

    void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        RenderSettings.skybox = runtimeSkyboxMat;

        if (scene.name == "Galaxies")
        {
            UnityEngine.Random.InitState(GameManager.LevelManager.currentUniverse.seed);
        }
        else if (GameManager.LevelManager.procedural)
        {
            UnityEngine.Random.InitState(GameManager.LevelManager.currentProceduralGalaxy[GameManager.LevelManager.levelIndex].seed);
        }
        else
        {
            string nameOfScene = scene.name == string.Empty ? "Blah Blah" : scene.name;
            UnityEngine.Random.InitState(LevelSettingsManager.ConvertTextToSeed(nameOfScene));
        }

        // Apply changes to the runtime instance, not the original material
        _Rotation = UnityEngine.Random.Range(0f, 360f);
    }
    private void Update()
    {
        if(lastRotation!=_Rotation)
        {
            runtimeSkyboxMat.SetFloat("_Rotation", _Rotation);
        }
        lastRotation = _Rotation;
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    private void OnApplicationQuit()
    {

    }
}

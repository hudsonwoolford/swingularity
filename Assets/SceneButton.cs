using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Unity.VisualScripting;

[ExecuteInEditMode()]
public class SceneButton : MonoBehaviour
{
    [SerializeField] string sceneName = "Scene", levelName = "Level";
    [ColorUsage(true, true)]
    [SerializeField] Color color = Color.white;
    public bool unlocked = false;
    Image image;
    TextMeshProUGUI nameText;
    // Start is called before the first frame update
    void Start()
    {
        nameText = GetComponentInChildren<TextMeshProUGUI>();
        image = GetComponentInChildren<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if(nameText == null) { nameText = GetComponentInChildren<TextMeshProUGUI>(); }
        nameText.text = levelName;
        image.color = color;
    }
    private void OnValidate()
    {

    }
    public void LoadScene()
    {
        if(string.IsNullOrEmpty(sceneName)) { return; }
        SceneManager.LoadScene(sceneName);
    }
}

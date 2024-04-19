using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

[ExecuteInEditMode()]
public class SceneButton : MonoBehaviour
{
    public bool unlocked = false;
    public string sceneName = "Scene";

    [SerializeField] string requiredScene = "Main Menu", stringName = "Name";
    [ColorUsage(true, true)]
    [SerializeField] Color color = Color.white;
    [SerializeField] bool scaleText = false;
    Button button;
    Image image;
    TextMeshProUGUI nameText;
    private void Awake()
    {
        if(!unlocked && Application.isPlaying)
        {
            unlocked = GameManager.LevelManager.LevelCompleted(requiredScene);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        nameText = GetComponentInChildren<TextMeshProUGUI>(true);
        image = GetComponentInChildren<Image>(true);
        button = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if(nameText == null) { nameText = GetComponentInChildren<TextMeshProUGUI>(true); }
        if(image == null) {  image = GetComponentInChildren<Image>(true); }
        if(button == null) { button = GetComponent<Button>(); }
        nameText.text = stringName;
        image.color = color;
        button.enabled = unlocked;
        if (scaleText)
        {
            nameText.transform.localScale =  image.transform.localScale;
        }
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

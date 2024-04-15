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
    [SerializeField] string sceneName = "Scene", stringName = "Name";
    [ColorUsage(true, true)]
    [SerializeField] Color color = Color.white;
    [SerializeField] bool scaleText = false;
    public bool unlocked = false;
    Button button;
    Image image;
    TextMeshProUGUI nameText;
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
        if(nameText == null) { nameText = GetComponentInChildren<TextMeshProUGUI>(); }
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

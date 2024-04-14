using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[ExecuteInEditMode()]
public class SceneButton : MonoBehaviour
{
    [SerializeField] string sceneName;
    Image image;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
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

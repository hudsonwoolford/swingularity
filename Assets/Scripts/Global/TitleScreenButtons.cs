using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreenButtons : MonoBehaviour
{
    [SerializeField] Button continueBtn;
    [SerializeField] Button newGameBtn;
    [SerializeField] Button loadGameBtn;
    [SerializeField] Button settingsBtn;

    int index;
    void Start()
    {
        continueBtn.onClick.AddListener(() => Continue());
        newGameBtn.onClick.AddListener(() => NewGame());
        loadGameBtn.onClick.AddListener(() => LoadGame());
        settingsBtn.onClick.AddListener(() => OpenSettings());
    }
    void Continue(){

    }
    void NewGame(){
        SceneManager.LoadScene("UniverseSelect");
    }
    void LoadGame(){
        
    }
    void OpenSettings(){
        
    }
}

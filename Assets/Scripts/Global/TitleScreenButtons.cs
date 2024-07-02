using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreenButtons : MonoBehaviour
{
    [SerializeField] Animator cameraAnim, menuAnim;
    [SerializeField] Button continueBtn;
    [SerializeField] Button newGameBtn;
    [SerializeField] Button loadGameBtn;
    [SerializeField] Button settingsBtn;
    bool ngMenu;
    void Start()
    {
        ngMenu = true;
        continueBtn.onClick.AddListener(() => Continue());
        newGameBtn.onClick.AddListener(() => NewGame());
        loadGameBtn.onClick.AddListener(() => LoadGame());
        settingsBtn.onClick.AddListener(() => OpenSettings());
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && ngMenu) 
        {
            EnableNewGameMenu(false);
        }
    }
    void Continue(){
        SceneManager.LoadScene("Galaxies");

    }
    void NewGame(){
        EnableNewGameMenu(true);
    }
    void LoadGame(){
        
    }
    void OpenSettings(){
        
    }
    void EnableNewGameMenu(bool enable)
    {
        if(enable)
        {
            cameraAnim.Play("MenuToNGMenu");
            menuAnim.Play("ShowNGMenu");
            ngMenu = true;
        }
        else if (!enable)
        {
            cameraAnim.Play("NGMenuToMenu");
            menuAnim.Play("HideNGMenu");
            ngMenu = false;
        }

    }
}

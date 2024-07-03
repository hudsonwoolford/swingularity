using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScreenButtons : MonoBehaviour
{
    [SerializeField] Animator cameraAnim, menuAnim, ngMenuAnim;
    [SerializeField] Button continueBtn;
    [SerializeField] Button newGameBtn;
    [SerializeField] Button loadGameBtn;
    [SerializeField] Button settingsBtn;
    [SerializeField] Button ngBackBtn;
    bool ngMenu;
    void Start()
    {
        ngMenu = true;
        continueBtn.onClick.AddListener(() => Continue());
        newGameBtn.onClick.AddListener(() => NewGame());
        loadGameBtn.onClick.AddListener(() => LoadGame());
        settingsBtn.onClick.AddListener(() => OpenSettings());
        ngBackBtn.onClick.AddListener(() => StartCoroutine(EnableNewGameMenu(false)));
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && ngMenu) 
        {
            StartCoroutine(EnableNewGameMenu(false));
        }
    }
    void Continue(){
        SceneManager.LoadScene("Galaxies");

    }
    void NewGame(){
        StartCoroutine(EnableNewGameMenu(true));
    }
    void LoadGame(){
        
    }
    void OpenSettings(){
        
    }
    IEnumerator EnableNewGameMenu(bool enable)
    {
        if(enable)
        {
            ngMenu = true;
            menuAnim.Play("HideMenu");
            // yield return new WaitForSeconds(menuAnim.GetCurrentAnimatorStateInfo(0).length/3);
            cameraAnim.Play("MenuToNGMenu");
            // yield return new WaitForSeconds(cameraAnim.GetCurrentAnimatorStateInfo(0).length/3);
            ngMenuAnim.Play("ShowNGMenu");
        }
        else if (!enable)
        {
            ngMenu = false;
            ngMenuAnim.Play("HideNGMenu");
            // yield return new WaitForSeconds(ngMenuAnim.GetCurrentAnimatorStateInfo(0).length/3);
            cameraAnim.Play("NGMenuToMenu");
            // yield return new WaitForSeconds(cameraAnim.GetCurrentAnimatorStateInfo(0).length/3);
            menuAnim.Play("ShowMenu");
        }
        yield return null;
    }
}

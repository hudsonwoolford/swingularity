using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

[ExecuteInEditMode()]
public class GalaxySummary : MonoBehaviour
{
    [SerializeField] bool[] collectedStars;
    [SerializeField] Image[] starImgs;
    [SerializeField] GameObject lockImage, stars;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();    
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnValidate()
    {
        UpdateStars();
    }
    public void Shake(){
        animator.Play("ShakeLock");
    }
    public void InitSummary(bool[] collectedStars, int lockStars)
    {
        if(GameManager.LevelManager.currentUniverse.numStars < lockStars){
            lockImage.SetActive(true);
            lockImage.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = (lockStars).ToString();
            stars.SetActive(false);
        }
        else{
            stars.SetActive(true);
            lockImage.SetActive(false);
            this.collectedStars = collectedStars;
            UpdateStars();
        }
    }
    public void EnableSummary(bool enable)
    {
        if(enable)
        {
            animator.Play("ShowMenu");
        }
        else
        {
            animator.Play("HideMenu");
        }
    }
    void UpdateStars()
    {
        for (int i = 0; i < starImgs.Length; i++)
        {
            if (i >= collectedStars.Length) { starImgs[i].enabled = false; continue; }
            starImgs[i].enabled = collectedStars[i];
        }
    }
}

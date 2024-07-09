using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[ExecuteInEditMode()]
public class GalaxySummary : MonoBehaviour
{
    [SerializeField] bool[] collectedStars;
    [SerializeField] Image[] starImgs;
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
    public void InitSummary(bool[] collectedStars)
    {
        this.collectedStars = collectedStars;
        UpdateStars();
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

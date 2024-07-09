using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[ExecuteInEditMode()]
public class GalaxySummary : MonoBehaviour
{
    [SerializeField] bool[] collectedStars;
    [SerializeField] Image[] starImgs;
    // Start is called before the first frame update
    void Start()
    {
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
    public void EnableStars()
    {
        for (int i = 0; i < starImgs.Length; i++)
        {
            starImgs[i].enabled = false;
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

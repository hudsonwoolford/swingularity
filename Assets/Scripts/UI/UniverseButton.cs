using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UniverseButton : MonoBehaviour
{
    [SerializeField] private bool unlockedByInfinite;
    [SerializeField] private GameObject lockVisual;
    private Button uniButton;
    void Start()
    {
        uniButton = GetComponent<Button>();

        UpdateLock();
    }

    void Update()
    {
        if(Input.anyKey)
        {
            UpdateLock();
        }
    }
    private void UpdateLock()
    {
        uniButton.interactable = true;
        lockVisual.SetActive(false);
        if (SaveLoadManager.currentGlobalSave != null && unlockedByInfinite)
        {
            if(SaveLoadManager.currentGlobalSave.unlockedInfinite)
            {
                uniButton.interactable = true;
                lockVisual.SetActive(false);
            }
            else
            {
                uniButton.interactable = false;
                lockVisual.SetActive(true);
            }
        }
    }
}

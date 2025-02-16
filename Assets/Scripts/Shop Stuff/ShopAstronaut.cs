using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopAstronaut : MonoBehaviour
{
    [SerializeField] private Transform hatParent;
    private GameObject currentHat;
    void Start()
    {
        UpdateHat();
    }

    void Update()
    {
        
    }


    void UpdateHat()
    {
        GameObject hatObj = GameManager.CustomizationManager.RetrieveShopItem(SaveLoadManager.currentGlobalSave.astronautHatElement).astronautHatElement.hatObj;
        if (hatObj != currentHat)
        {
            if(currentHat != null)
            {
                Destroy(currentHat);
            }
            if(hatObj != null)
            {
                currentHat = Instantiate(hatObj, hatParent);
            }
        }
    }
    private void OnEnable()
    {
        SettingsManager.applySettingsVar += UpdateHat;
    }
    private void OnDisable()
    {
        SettingsManager.applySettingsVar -= UpdateHat;
    }
}

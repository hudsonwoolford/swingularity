using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomizeElement : MonoBehaviour
{
    public ShopItem _ShopItem;
    public bool isUnlocked;
    public Animator buttonAnimator;

    [Header("Element References")]
    [SerializeField] Material[] whiteMaterials;
    [SerializeField] Animator lockAnim;
    [SerializeField] Image lockImg;
    [SerializeField] Image itemVisualImg;
    [SerializeField] Image supernovaeIcon;
    [SerializeField] TextMeshProUGUI requiredSupernovaeTxt;
    [SerializeField] Button equipButton;
    [SerializeField] Vector2 supernovaeIconOffset;
    [SerializeField] AudioObj buyItemObj;
    [SerializeField] AudioObj equipItemObj;
    [SerializeField] AudioObj lockedErrorObj;
    [SerializeField] AudioObj selectItemObj;
    [SerializeField] Color selectedText;
    [SerializeField] Color unselectedText;

    public delegate void ItemEquip();
    public ItemEquip itemEquip;

    public void InitElement(System.Action<CustomizeElement> buttonCall)
    {
        UpdateUIElements();
        SetItemUpdates();

        equipButton.onClick.AddListener(() => GameManager.SFXManager.PlaySFX(selectItemObj));
        equipButton.onClick.AddListener(() => buttonCall(this));
    }
    void UpdateEquipElement()
    {
        if (SaveLoadManager.currentGlobalSave != null)
        {
            if (SaveLoadManager.currentGlobalSave.astronautHatElement == _ShopItem.name || SaveLoadManager.currentGlobalSave.astronautMaterialElement == _ShopItem.name || SaveLoadManager.currentGlobalSave.golfBallTrailElement == _ShopItem.name)
            {
                requiredSupernovaeTxt.text = "Equipped";
                requiredSupernovaeTxt.color = selectedText;
                supernovaeIcon.enabled = false;
            }
            else
            {
                requiredSupernovaeTxt.text = "Unlocked";
                requiredSupernovaeTxt.color = unselectedText;

                supernovaeIcon.enabled = false;
            }
        }

    }
    void UpdateUIElements()
    {
        if (_ShopItem == null) { return; } //no item

        //item visual img
        itemVisualImg.sprite = _ShopItem.itemVisual;

        //button interactable
        if (SaveLoadManager.currentGlobalSave != null)
        {
            isUnlocked = false;
            foreach (var item in SaveLoadManager.currentGlobalSave.unlockedShopItems)
            {
                isUnlocked |= item == _ShopItem.name;
            }

            if (_ShopItem.requiredSupernovae <= 0 && !isUnlocked) //has not been placed in unlocked items yet
            {
                GameManager.SaveLoadManager.UnlockShopItem(_ShopItem.name);
                isUnlocked |= true;
            }
            lockImg.gameObject.SetActive(!isUnlocked);
        }

        if (!isUnlocked)
        {
            //cost string
             requiredSupernovaeTxt.color = selectedText;

            supernovaeIcon.enabled = true;
            string cost = _ShopItem.requiredSupernovae.ToString();
            requiredSupernovaeTxt.text = cost;
            requiredSupernovaeTxt.ForceMeshUpdate();
            if (requiredSupernovaeTxt.textInfo != null && requiredSupernovaeTxt.textInfo.characterCount > 0)
            {
                Vector2 midLeft = (requiredSupernovaeTxt.textInfo.characterInfo[0].bottomLeft + requiredSupernovaeTxt.textInfo.characterInfo[0].topLeft) / 2;
                supernovaeIcon.transform.localPosition = midLeft + (Vector2)requiredSupernovaeTxt.transform.localPosition + supernovaeIconOffset;
            }
        }
        else
        {
            UpdateEquipElement();
        }
    }
    private void FixedUpdate()
    {
        if(isUnlocked)
        {
            UpdateEquipElement();
        }
    }
    void Start()
    {
    }
    private void OnValidate()
    {
        UpdateUIElements();
    }
    #region Buy Functions
    public bool TryBuyItem()
    {
        if(SaveLoadManager.currentGlobalSave.totalSupernovae < _ShopItem.requiredSupernovae)
        {
            GameManager.SFXManager.PlaySFX(lockedErrorObj);
            lockAnim.Play("LockShake");
            return false;
        }
        GameManager.SFXManager.PlaySFX(buyItemObj);
        GameManager.SaveLoadManager.IncrementSupernovae(-(int)_ShopItem.requiredSupernovae);
        GameManager.SaveLoadManager.UnlockShopItem(_ShopItem.name);
        isUnlocked = true;
        UpdateUIElements();
        return true;
    }
    #endregion
    #region Equip Functions
    void SetGolfBallTrailMaterials(ShopItem shopItem)
    {
        SaveLoadManager.currentGlobalSave.golfBallTrailElement = shopItem.name;
        SettingsManager.applySettingsVar?.Invoke();
    }
    void SetAstronautMaterials(ShopItem shopItem)
    {
        foreach (Material wm in whiteMaterials)
        {
            wm.SetTexture("_MainTex", shopItem.materialCustomizationElement.texture);
            wm.SetColor("_Color", shopItem.materialCustomizationElement.matColor);
            wm.SetFloat("_Metallic", shopItem.materialCustomizationElement.metallic);
            wm.SetFloat("_Glossiness", shopItem.materialCustomizationElement.smoothness);
        }
        SaveLoadManager.currentGlobalSave.astronautMaterialElement = shopItem.name;
        SettingsManager.applySettingsVar?.Invoke();
    }
    void SetAstronautHat(ShopItem shopItem)
    {
        SaveLoadManager.currentGlobalSave.astronautHatElement = shopItem.name;
        SettingsManager.applySettingsVar?.Invoke();
    }
    void SetItemUpdates()
    {
        switch (_ShopItem.itemType)
        {
            case ShopItem.ItemType.AstronautMaterial:
                itemEquip = () => SetAstronautMaterials(_ShopItem);
                break;
            case ShopItem.ItemType.GolfBallTrail:
                itemEquip = () => SetGolfBallTrailMaterials(_ShopItem);
                break;
            case ShopItem.ItemType.AstronautHat:
                itemEquip = () => SetAstronautHat(_ShopItem);
                break;
            default:
                itemEquip = null;
                break;
        }

        itemEquip += () => GameManager.SFXManager.PlaySFX(equipItemObj);
    }
    #endregion
    private void OnEnable()
    {
    }
    private void OnDisable()
    {
    }
}

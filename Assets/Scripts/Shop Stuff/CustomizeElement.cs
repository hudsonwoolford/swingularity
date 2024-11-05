using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomizeElement : MonoBehaviour
{
    public ShopItem _ShopItem;

    [Header("Element References")]
    [SerializeField] Material[] whiteMaterials;
    [SerializeField] Image itemVisualImg;
    [SerializeField] Image supernovaeIcon;
    [SerializeField] TextMeshProUGUI requiredSupernovaeTxt;
    [SerializeField] Button equipButton;
    [SerializeField] Vector2 supernovaeIconOffset;

    private delegate void ItemEquip();
    private ItemEquip itemEquip;

    public void InitElement()
    {
        UpdateUIElements();
        SetItemUpdates();

        equipButton.onClick.AddListener(() => itemEquip.Invoke());
    }
    void UpdateUIElements()
    {
        if(_ShopItem == null) { return; } //no item

        //item visual img
        itemVisualImg.sprite = _ShopItem.itemVisual;

        //cost string
        string cost = _ShopItem.requiredSupernovae.ToString();
        requiredSupernovaeTxt.text = cost;
        requiredSupernovaeTxt.ForceMeshUpdate();
        if (requiredSupernovaeTxt.textInfo != null && requiredSupernovaeTxt.textInfo.characterCount > 0)
        {
            Vector2 midLeft = (requiredSupernovaeTxt.textInfo.characterInfo[0].bottomLeft + requiredSupernovaeTxt.textInfo.characterInfo[0].topLeft) / 2;
            supernovaeIcon.transform.localPosition = midLeft + (Vector2)requiredSupernovaeTxt.transform.localPosition + supernovaeIconOffset;
        }

        //button interactable
        if(SaveLoadManager.currentGlobalSave!=null)
        {
            equipButton.interactable = SaveLoadManager.currentGlobalSave.totalSupernovae >= _ShopItem.requiredSupernovae;
        }
    }
    void Start()
    {
    }
    private void OnValidate()
    {
        UpdateUIElements();
    }
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
            wm.SetColor("_Color", shopItem.materialCustomizationElement.matColor);
            wm.SetFloat("_Metallic", shopItem.materialCustomizationElement.metallic);
            wm.SetFloat("_Glossiness", shopItem.materialCustomizationElement.metallic);
        }
        SaveLoadManager.currentGlobalSave.astronautMaterialElement = shopItem.name;
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
                itemEquip = null;
                break;
            default:
                itemEquip = null;
                break;
        }

    }
    #endregion
    private void OnEnable()
    {
    }
    private void OnDisable()
    {
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI starCoinTxt;
    [SerializeField] private float coinFillSpeed;
    [SerializeField] private AudioObj coinFillObj;
    [SerializeField] private GameObject customizationElement;
    [SerializeField] private float customizationHeightJump, customizationWidthJump;
    [SerializeField] private int customizationColumns;
    [Header("Shopping")]
    [SerializeField] private Button buyEquipButton;
    [SerializeField] private TextMeshProUGUI buyEquipText;
    [Header("Tabs")]
    [SerializeField] private Button astronautMaterialBtn;
    [SerializeField] private Button golfTrailBtn;
    [SerializeField] private Button astronautHatBtn;
    [SerializeField] private CanvasGroup astronautMaterialCG;
    [SerializeField] private CanvasGroup golfTrailCG;
    [SerializeField] private CanvasGroup astronautHatCG;
    [SerializeField] private float tabTransitionDuration;
    [SerializeField] private AnimationCurve transitionCurve;
    [SerializeField] private Scrollbar scrollbar;

    ShopItem[] shopItems;
    CustomizeElement currentSelectedElement;
    CustomizeElement[] customizeElements;
    CanvasGroup currentTab;
    Button currentButton;
    AudioSource shopSrc;
    int displayCoins, consecutiveCoinMoves;
    bool shopShown;
    Vector3 customizationParentDefaultPos;
    Dictionary<CanvasGroup, int> groupItemAmounts = new Dictionary<CanvasGroup, int>();

    void Start()
    {
        shopSrc = GetComponent<AudioSource>();
        shopItems = GameManager.CustomizationManager._AllShopItems;
        
        PopulateCustomizationElements();

        astronautMaterialBtn.onClick.AddListener(() => StartCoroutine(SwitchTab(ShopItem.ItemType.AstronautMaterial, astronautMaterialBtn, false)));
        golfTrailBtn.onClick.AddListener(() => StartCoroutine(SwitchTab(ShopItem.ItemType.GolfBallTrail, golfTrailBtn, false)));
        astronautHatBtn.onClick.AddListener(() => StartCoroutine(SwitchTab(ShopItem.ItemType.AstronautHat, astronautHatBtn, false)));
        buyEquipButton.onClick.AddListener(() => BuyEquipButton());
        scrollbar.onValueChanged.AddListener((v) => ScrollbarValueChanged(v));

        DisableAllTabs();
        astronautMaterialCG.alpha = 1.0f; astronautMaterialCG.interactable = true; astronautMaterialCG.blocksRaycasts = true;
        currentTab = astronautMaterialCG;
        currentButton = astronautMaterialBtn;
        currentButton.GetComponent<Animator>().Play("Select");
        customizationParentDefaultPos = astronautMaterialCG.transform.localPosition;
    }
    private void Update()
    {
        //
        if(shopShown)
        {
            scrollbar.value += -Input.mouseScrollDelta.y / 10;
            scrollbar.value = Mathf.Clamp(scrollbar.value, 0f, 1f);
        }

        if (shopShown)
        {
            buyEquipButton.gameObject.SetActive(currentSelectedElement != null);
            if (currentSelectedElement != null)
            {
                buyEquipText.text = currentSelectedElement.isUnlocked ? "EQUIP" : "BUY";
            }
        }
    }
    void FixedUpdate()
    {
        //Coins going up and down with pitch
        if(shopShown)
        {
            int nextDisplay = Mathf.RoundToInt(Mathf.Lerp(displayCoins, SaveLoadManager.currentGlobalSave.totalSupernovae, coinFillSpeed));
            int dif = nextDisplay - displayCoins;
            if(dif != 0)
            {
                consecutiveCoinMoves += (int)Mathf.Sign(dif);
                shopSrc.pitch = 1 + consecutiveCoinMoves * 0.01f;
                GameManager.SFXManager.PlaySFX(coinFillObj, shopSrc, coinFillObj.inherentVol);
                displayCoins = Mathf.Max(nextDisplay, 0);
            }
            else
            {
                displayCoins = SaveLoadManager.currentGlobalSave.totalSupernovae;
                consecutiveCoinMoves = 0;
            }
            starCoinTxt.text = displayCoins.ToString();
        }
    }
    void ScrollbarValueChanged(float value)
    {
        Transform parent = currentTab.transform;

        parent.transform.localPosition = Vector3.Lerp(customizationParentDefaultPos, customizationParentDefaultPos - new Vector3(0, Mathf.Max(0, Mathf.CeilToInt((float)groupItemAmounts[currentTab] / customizationColumns) - 2) * customizationHeightJump, 0), value);
    }
    void SelectButton(CustomizeElement element)
    {
        if (element==currentSelectedElement)
        {
            //element.buttonAnimator.Play("Reselect");
        }
        else
        {
            currentSelectedElement?.buttonAnimator.Play("Unselect");
            element.buttonAnimator.Play("Select");
        }
        currentSelectedElement = element;
    }
    void BuyEquipButton()
    {
        if(currentSelectedElement == null) { return; }
        //buy
        if(!currentSelectedElement.isUnlocked) //locked
        {
            if (!currentSelectedElement.TryBuyItem()) //failed purchase
            {
                return;
            }
        }
        currentSelectedElement.itemEquip?.Invoke();
        currentSelectedElement?.buttonAnimator.Play("Unselect");
        currentSelectedElement = null;
    }
    IEnumerator SwitchTab(ShopItem.ItemType itemType, Button thisButton, bool instant)
    {
        CanvasGroup currentGroup = currentTab;
        CanvasGroup nextGroup = null;
        currentSelectedElement?.buttonAnimator.Play("Unselect");
        currentSelectedElement = null;

        nextGroup = GetTabGroup(itemType);
        if (nextGroup == currentTab) { yield break; }

        currentGroup.interactable = false;
        currentGroup.blocksRaycasts = false;

        thisButton.GetComponent<Animator>().Play("Select");
        currentButton.GetComponent<Animator>().Play("Unselect");
        currentButton = thisButton;

        float timer = 0;
        while (timer < tabTransitionDuration && !instant)
        {
            currentGroup.alpha = 1 - transitionCurve.Evaluate(timer / tabTransitionDuration);
            nextGroup.alpha = transitionCurve.Evaluate(timer / tabTransitionDuration);

            timer += Time.deltaTime;
            yield return null;
        }
        timer = tabTransitionDuration;
        WhileLoopFunc();

        nextGroup.interactable = true;
        nextGroup.blocksRaycasts = true;

        currentTab = nextGroup;
        void WhileLoopFunc()
        {
            currentGroup.alpha = 1 - timer / tabTransitionDuration;
            nextGroup.alpha = timer / tabTransitionDuration;
        }

    }

    public void EnableShop(float delay, bool enable)
    {
        StartCoroutine(AnimateCustomizeMaterials(delay, enable));
    }
    IEnumerator AnimateCustomizeMaterials(float delay, bool enable)
    {
        displayCoins = 0;
        shopShown = !enable ? enable : shopShown;
        yield return new WaitForSeconds(delay);
        if (SaveLoadManager.currentGlobalSave != null && enable)
        {
            shopShown = enable;
        }
    }
    void DisableAllTabs()
    {
        astronautMaterialCG.alpha = 0; golfTrailCG.alpha = 0; astronautHatCG.alpha = 0;
        astronautMaterialCG.interactable = false; golfTrailCG.interactable = false; astronautHatCG.interactable = false;
        astronautMaterialCG.blocksRaycasts = false; golfTrailCG.blocksRaycasts = false; astronautHatCG.blocksRaycasts = false;

        astronautMaterialBtn.GetComponent<Animator>().Play("Unselect");
        golfTrailBtn.GetComponent<Animator>().Play("Unselect");
        astronautHatBtn.GetComponent<Animator>().Play("Unselect");
    }
    void PopulateCustomizationElements()
    {
        ShopItem[] sItems = shopItems;
        Array.Sort(sItems, (a, b) => a.requiredSupernovae.CompareTo(b.requiredSupernovae));
        customizeElements = new CustomizeElement[shopItems.Length];

        PopulateTab(ShopItem.ItemType.AstronautMaterial);
        PopulateTab(ShopItem.ItemType.GolfBallTrail);
        PopulateTab(ShopItem.ItemType.AstronautHat);
    }
    void PopulateTab(ShopItem.ItemType filter)
    {
        CanvasGroup currentCG = GetTabGroup(filter);
        Transform parent = currentCG.transform;
        if (!groupItemAmounts.ContainsKey(currentCG))
        {
            groupItemAmounts.Add(currentCG, 0);
        }

        int columnTracker = 0;
        int rowTracker = 0;
        for (int i = 0; i < shopItems.Length; i++)
        {
            if (shopItems[i].itemType != filter) { continue; }
            groupItemAmounts[currentCG]++;

            CustomizeElement cE = Instantiate(customizationElement, parent).GetComponent<CustomizeElement>();
            customizeElements[i] = cE;

            cE.transform.localPosition = new Vector3(columnTracker * customizationWidthJump, rowTracker * customizationHeightJump, 0);

            cE._ShopItem = shopItems[i];
            cE.InitElement((x)=>SelectButton(x));

            columnTracker++;
            if (columnTracker % customizationColumns == 0)
            {
                columnTracker %= customizationColumns;
                rowTracker++;
            }
        }
    }
    CanvasGroup GetTabGroup(ShopItem.ItemType tabType)
    {
        CanvasGroup group = null;
        switch (tabType)
        {
            case ShopItem.ItemType.AstronautMaterial:
                group = astronautMaterialCG;
                break;
            case ShopItem.ItemType.GolfBallTrail:
                group = golfTrailCG;
                break;
            case ShopItem.ItemType.AstronautHat:
                group = astronautHatCG;
                break;
            default:
                Debug.LogWarning("No parent set for tab, add to this switch case");
                break;
        }
        return group;
    }
}

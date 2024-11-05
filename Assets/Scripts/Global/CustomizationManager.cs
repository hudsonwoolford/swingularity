using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizationManager : MonoBehaviour
{
    [SerializeField] private ShopItem defaultShopItem;
    [SerializeField] private ShopItem[] allShopItems;

    public ShopItem _DefaultShopItem => defaultShopItem;
    public ShopItem[] _AllShopItems => allShopItems;
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public ShopItem RetrieveShopItem(string ID)
    {
        for (int i = 0; i < allShopItems.Length; i++)
        {
            if(allShopItems[i].name == ID)
            {
                return allShopItems[i];
            }
        }
        return defaultShopItem;
    }
}

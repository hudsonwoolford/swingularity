using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class TestHat : MonoBehaviour
{
    [SerializeField] private ShopItem shopItem;
    [SerializeField] private bool update;
    GameObject hatObj;
    void Start()
    {
        hatObj = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if(update)
        {
            update = false;
            if(hatObj != null)
            {
                DestroyImmediate(hatObj);
            }
            if(shopItem.astronautHatElement.hatObj!=null)
            {
                hatObj = GameObject.Instantiate(shopItem.astronautHatElement.hatObj, transform);
            }
        }
    }
}

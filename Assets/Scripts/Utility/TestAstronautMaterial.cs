using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode()]
public class TestAstronautMaterial : MonoBehaviour
{
    [SerializeField] private ShopItem shopItem;
    [SerializeField] private bool update;
    SkinnedMeshRenderer skinnedMeshRenderer;
    MaterialPropertyBlock mpb;
    private void Start()
    {
        update = true;
    }
    void Update()
    {
        NullChecks();

        if(update)
        {
            if(shopItem == null) { return; }

            mpb.SetColor("_Color" ,shopItem.materialCustomizationElement.matColor);
            mpb.SetFloat("_Metallic" ,shopItem.materialCustomizationElement.metallic);
            mpb.SetFloat("_Glossiness", shopItem.materialCustomizationElement.smoothness);

            skinnedMeshRenderer.SetPropertyBlock(mpb, 0);
            skinnedMeshRenderer.SetPropertyBlock(mpb, 1);
            skinnedMeshRenderer.SetPropertyBlock(mpb, 3);

            update = false;
        }
    }
    void NullChecks()
    {
        if(skinnedMeshRenderer == null) { skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>(); }
        if(mpb == null) { mpb = new MaterialPropertyBlock(); }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AstronautHat : MonoBehaviour
{
    [SerializeField] private Material[] defaultMaterials;
    [SerializeField] private Material[] fadeMaterials;
    [SerializeField] private bool[] dynamicThemeColors;
    [SerializeField] private bool[] dynamicPaintColors;
    [SerializeField] private Material syncFadeMat;
    [SerializeField] private string fadePropertyName;
    MeshRenderer hatRenderer;
    MaterialPropertyBlock hatBlock;
    bool isFading = false;
    private void Awake()
    {
        hatRenderer = GetComponent<MeshRenderer>();
        hatBlock = new MaterialPropertyBlock();
        hatRenderer.SetPropertyBlock(hatBlock);
    }
    void Start()
    {
        CalibrateMaterialInstances(false);
    }

    void Update()
    {
        if(isFading)
        {
            UpdateFade();
        }
    }
    public void UpdateFade()
    {
        float fadeValue = syncFadeMat.GetFloat(fadePropertyName);

        hatBlock.SetFloat(fadePropertyName, fadeValue);
        hatRenderer.SetPropertyBlock(hatBlock);
    }
    public void StartFade()
    {
        isFading = true;

        //swap to fade materials
        hatRenderer.materials = fadeMaterials;
        CalibrateMaterialInstances(true);
    }
    public void EndFade()
    {
        isFading = false;

        //swap to default materials
        hatRenderer.materials = defaultMaterials;

        hatBlock.SetFloat(fadePropertyName, 1);
        hatRenderer.SetPropertyBlock(hatBlock);
        CalibrateMaterialInstances(false);
    }
    void CalibrateMaterialInstances(bool fadeMats)
    {
        int i = 0;
        foreach (var m in hatRenderer.materials)
        {
            if(fadeMats)
            {
                m.SetColor("_EdgeColor", syncFadeMat.GetColor("_EdgeColor"));
            }
            if (dynamicThemeColors != null && dynamicThemeColors.Length > i && dynamicThemeColors[i])
            {
                m.SetColor("_Color", GameManager.LevelManager.currentTheme.mainColor);
            }           
            if (dynamicPaintColors != null && dynamicPaintColors.Length > i && dynamicPaintColors[i])
            {
                ShopItem.MaterialCustomizationElement mce = GameManager.CustomizationManager.RetrieveShopItem(SaveLoadManager.currentGlobalSave.astronautMaterialElement).materialCustomizationElement;
                m.SetColor("_Color", mce.matColor);
                m.SetFloat("_Metallic", mce.metallic);
                m.SetFloat("_Glossiness", mce.smoothness);
            }
            i++;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AstronautHat))]
[CanEditMultipleObjects()]
public class AstronautHatEditor : Editor
{
    AstronautHat astronautHat;

    SerializedProperty defaultMaterialsProp;
    SerializedProperty fadeMaterialsProp;
    SerializedProperty dynamicGlowColorsProp;
    SerializedProperty dynamicMatteColorsProp;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.Box("Arrays will update automatically to match meshrenderer materials");

        MeshRenderer mr = astronautHat.gameObject.GetComponent<MeshRenderer>();

        defaultMaterialsProp.arraySize = mr.sharedMaterials.Length;
        fadeMaterialsProp.arraySize = mr.sharedMaterials.Length;
        dynamicGlowColorsProp.arraySize = mr.sharedMaterials.Length;
        dynamicMatteColorsProp.arraySize = mr.sharedMaterials.Length;

        DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        astronautHat = (AstronautHat)target;

        defaultMaterialsProp = serializedObject.FindProperty("defaultMaterials");
        fadeMaterialsProp = serializedObject.FindProperty("fadeMaterials");
        dynamicGlowColorsProp = serializedObject.FindProperty("dynamicThemeColors");
        dynamicMatteColorsProp = serializedObject.FindProperty("dynamicPaintColors");
    }
}

#endif
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
        foreach (var m in hatRenderer.materials)
        {
            m.SetColor("_EdgeColor", syncFadeMat.GetColor("_EdgeColor"));
        }
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
    }
    public void EndFade()
    {
        isFading = false;

        //swap to default materials
        hatRenderer.materials = defaultMaterials;

        hatBlock.SetFloat(fadePropertyName, 1);
        hatRenderer.SetPropertyBlock(hatBlock);
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
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.Box("Arrays will update automatically to match meshrenderer materials");

        MeshRenderer mr = astronautHat.gameObject.GetComponent<MeshRenderer>();

        defaultMaterialsProp.arraySize = mr.sharedMaterials.Length;
        fadeMaterialsProp.arraySize = mr.sharedMaterials.Length;

        DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        astronautHat = (AstronautHat)target;

        defaultMaterialsProp = serializedObject.FindProperty("defaultMaterials");
        fadeMaterialsProp = serializedObject.FindProperty("fadeMaterials");
    }
}

#endif
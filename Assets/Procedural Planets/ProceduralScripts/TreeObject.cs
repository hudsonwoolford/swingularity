using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName ="treeObj", menuName = "treeObject")]
public class TreeObject : ScriptableObject
{
    public GameObject treePrefab;
    [HideInInspector]
    [SerializeField] public bool[] dynamicMaterial;
}
[CustomEditor(typeof(TreeObject))]
public class TreeObjectEditor : Editor
{
    TreeObject treeObject;
    SerializedProperty dynamicMaterial;
    private void OnEnable()
    {
        treeObject = (TreeObject)target;
        dynamicMaterial = serializedObject.FindProperty("dynamicMaterial");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        GameObject prefab = treeObject.treePrefab;
        if(prefab==null) { return; }
        int materialListSize = prefab.GetComponentInChildren<MeshRenderer>().sharedMaterials.Length;
        dynamicMaterial.arraySize = materialListSize;
        EditorGUILayout.PropertyField(dynamicMaterial, new GUIContent("Dynamic Elements: " + prefab.name));
        serializedObject.ApplyModifiedProperties();
    }
}
using System.Numerics;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;

[CustomEditor(typeof(FullPlanetGeneration))]
public class FullPlanetEditor : Editor
{
    #region var
    FullPlanetGeneration planetGeneration; //main script
    Editor terrainEditor; //the normal editor for the terrain scriptable object
    Editor planetEditor;
    MaterialEditor shadingEditor; //the normal editor for the material

    bool oceanFoldout; //if the ocean dropdown is folded out
    bool terrainFoldout; //if the terrain dropdown is folded out
    bool planetFoldout; //if the planet dropdown is folded out
    bool colorSchemes; //if colorschemes are toggled

    SerializedProperty biomeA, biomeB; //serialized properties for defining biome a and biome b color schemes
    SerializedProperty oBiomeA, oBiomeB, aBiomeA, aBiomeB, tBiomeA, tBiomeB; //serialized properties for defining biome a and biome b color schemes
    #endregion

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        EditorGUI.BeginChangeCheck();

        if (GUILayout.Button("Random")) {
            planetGeneration.RandomizeTerrainScheme();
            planetGeneration.RandomizeOffset();
            if(colorSchemes) { planetGeneration.RandomizeColorScheme(); }
            
        }
        EditorWindow inspectorWindow = EditorWindow.focusedWindow;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Terrain", GUILayout.Width(Screen.width / 3f))) 
        {
            planetGeneration.RandomizeTerrainScheme();
        }
        if (GUILayout.Button("Random Offset", GUILayout.MaxWidth(Screen.width / 2)))
        {
            planetGeneration.RandomizeOffset();
        }
        GUILayout.EndHorizontal();

        if (!colorSchemes) //Color schemes is disabled
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Random Color Shading", GUILayout.Width(Screen.width / 2)))
            {
                planetGeneration.RandomizeShading(); //Button for shading
                GUILayout.BeginHorizontal();
            }
            if (GUILayout.Button("Random Smooth & Metallic", GUILayout.MaxWidth(Screen.width / 2))) { planetGeneration.RandomizeSmoothAndMetallic(); } //Button for smoothness and metallic
            GUILayout.EndHorizontal();

            //Color scheme enable if pressed
            if (GUILayout.Button("Toggle Color Scheme"))
            {
                colorSchemes = true; planetGeneration.SaveColorScheme(); planetGeneration.ApplyColorScheme();
                planetGeneration.colorSchemesOut = true;
            }
            if (GUILayout.Button("Toggle Ocean")) { planetGeneration.ToggleOcean(); }
        }
        else //Color schemes are enabled
        {
            if (GUILayout.Button("Toggle Color Scheme")) { colorSchemes = false; planetGeneration.RestoreSavedColorScheme(true, true); planetGeneration.colorSchemesOut = false; } // Disabling color scheme if pressed
            if (GUILayout.Button("Random Color Scheme"))
            {
                planetGeneration.RandomizeColorScheme();
            }
            //CS BiomeA: "Object" | CS BiomeB: "Object"
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("CS BiomeA:");
            EditorGUILayout.PropertyField(biomeA, GUIContent.none);
            GUILayout.Label("CS BiomeB:");
            EditorGUILayout.PropertyField(biomeB, GUIContent.none);
            EditorGUILayout.EndHorizontal();
            //Ocean and Atmosphere toggles
            EditorGUILayout.BeginHorizontal();
            oBiomeA.boolValue = EditorGUILayout.Toggle("O BiomeA: ", oBiomeA.boolValue);
            oBiomeB.boolValue = EditorGUILayout.Toggle("O BiomeB: ", oBiomeB.boolValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            aBiomeA.boolValue = EditorGUILayout.Toggle("A BiomeA: ", aBiomeA.boolValue);
            aBiomeB.boolValue = EditorGUILayout.Toggle("A BiomeB: ", aBiomeB.boolValue);
            EditorGUILayout.EndHorizontal(); 
            EditorGUILayout.BeginHorizontal();
            tBiomeA.boolValue = EditorGUILayout.Toggle("T BiomeA: ", tBiomeA.boolValue);
            tBiomeB.boolValue = EditorGUILayout.Toggle("T BiomeB: ", tBiomeB.boolValue);
            EditorGUILayout.EndHorizontal();

            //Apply button
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
        }
        
        if (GUILayout.Button("Toggle Atmosphere")) { planetGeneration.ToggleAtmosphere(); }

        if (planetGeneration.planet!= null)
        {
            planetFoldout = EditorGUILayout.InspectorTitlebar(planetFoldout, planetGeneration.planet);
            if (planetFoldout)
            {
                
                CreateCachedEditor(planetGeneration.planet, null, ref planetEditor);
                planetEditor.DrawHeader();
                planetEditor.OnInspectorGUI();
            }
        }
        //Display terrain and material
        if (planetGeneration.planet.terrainVar != null)
        {
            SettingsMenu(planetGeneration.planet.terrainVar, ref terrainFoldout, ref terrainEditor);
        }

        if (planetGeneration.planet.planetMaterial != null)
        {   
            DestroyImmediate(shadingEditor); shadingEditor = (MaterialEditor)CreateEditor(planetGeneration.planet.planetMaterial);
            shadingEditor.DrawHeader();
            shadingEditor.OnInspectorGUI();
        }

        GUILayout.Space(1000);
        //Apply properties and save
        serializedObject.ApplyModifiedProperties();
        SaveState();
    }
    void SettingsMenu(Object obj, ref bool foldout, ref Editor currentEditor)
    {
        foldout = EditorGUILayout.InspectorTitlebar(foldout, obj);
        if (foldout)
        {
            CreateCachedEditor(obj, null, ref currentEditor);
            currentEditor.DrawHeader();
            currentEditor.OnInspectorGUI();
        }
    }
    private void OnEnable()
    {
        #region SerializedProperties
        biomeA = serializedObject.FindProperty("biomeA");
        biomeB = serializedObject.FindProperty("biomeB");
        oBiomeA = serializedObject.FindProperty("oBiomeA");
        oBiomeB = serializedObject.FindProperty("oBiomeB");
        aBiomeA = serializedObject.FindProperty("aBiomeA");
        aBiomeB = serializedObject.FindProperty("aBiomeB");
        tBiomeA = serializedObject.FindProperty("tBiomeA");
        tBiomeB = serializedObject.FindProperty("tBiomeB");
        #endregion
        planetGeneration = (FullPlanetGeneration)target;
        shadingEditor = (MaterialEditor)CreateEditor(planetGeneration.planet.planetMaterial);

        oceanFoldout = EditorPrefs.GetBool(nameof(oceanFoldout), false);
        terrainFoldout = EditorPrefs.GetBool(nameof(terrainFoldout), false);
        planetFoldout = EditorPrefs.GetBool(nameof(planetFoldout), false);
        colorSchemes = EditorPrefs.GetBool(nameof(colorSchemes), false);

    }
    private void OnDisable()
    {
        if (shadingEditor != null) { DestroyImmediate(shadingEditor); }
        if (terrainEditor != null) { DestroyImmediate(terrainEditor); }
    }
    void SaveState()
    {
        EditorPrefs.SetBool(nameof(oceanFoldout), oceanFoldout);
        EditorPrefs.SetBool(nameof(terrainFoldout), terrainFoldout);
        EditorPrefs.SetBool(nameof(planetFoldout), planetFoldout);
        EditorPrefs.SetBool(nameof(colorSchemes), colorSchemes);
    }
}

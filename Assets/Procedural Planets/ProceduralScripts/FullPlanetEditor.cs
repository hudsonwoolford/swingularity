using System.Numerics;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FullPlanetGeneration))]
public class FullPlanetEditor : Editor
{
    #region var
    FullPlanetGeneration planetGeneration; //main script
    Editor terrainEditor; //the normal editor for the terrain scriptable object
    MaterialEditor shadingEditor; //the normal editor for the material

    bool oceanFoldout; //if the ocean dropdown is folded out
    bool atmosphereFoldout, innerAtmosphereFoldout, outerAtmosphereFoldout, frontAtmosphereFoldout; //if the atmosphere dropdown is folded out
    bool terrainFoldout; //if the terrain dropdown is folded out
    bool colorSchemes; //if colorschemes are toggled

    SerializedProperty biomeA, biomeB; //serialized properties for defining biome a and biome b color schemes
    SerializedObject planetObj;
    SerializedProperty oceanVariables, atmosphereVariables; //serialized properties for defining ocean and atmosphere variables
    SerializedProperty seaLevelProperty;
    SerializedProperty oceanBlendEndProperty;
    SerializedProperty oceanBlendIntensityProperty;
    SerializedProperty shorelineColorProperty;
    SerializedProperty seabedColorProperty;
    SerializedProperty oceanColorProperty;
    SerializedProperty oceanMetallicProperty;
    SerializedProperty oceanSmoothnessProperty;
    SerializedProperty atmosphereInnerColorProperty;
    SerializedProperty atmosphereInnerRangeProperty;
    SerializedProperty atmosphereInnerIntensityProperty;
    SerializedProperty atmosphereOuterColorProperty;
    SerializedProperty atmosphereOuterRangeProperty;
    SerializedProperty atmosphereOuterIntensityProperty;
    SerializedProperty atmosphereFrontColorProperty;
    SerializedProperty atmosphereFrontRangeProperty;
    SerializedProperty atmosphereFrontIntensityProperty;
    #endregion

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        EditorGUI.BeginChangeCheck();

        if (GUILayout.Button("Random")) { }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Random Terrain", GUILayout.Width(Screen.width / 2))) 
        { 
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
            }
        }
        else //Color schemes are enabled
        {
            if (GUILayout.Button("Toggle Color Scheme")) { colorSchemes = false; planetGeneration.RestoreSavedColorScheme(true, true); } // Disabling color scheme if pressed

            //CS BiomeA: "Object" | CS BiomeB: "Object"
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("CS BiomeA:");
            EditorGUILayout.PropertyField(biomeA, GUIContent.none);
            GUILayout.Label("CS BiomeB:");
            EditorGUILayout.PropertyField(biomeB, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            //Apply button
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply", GUILayout.Width(100)))
            {
                planetGeneration.ApplyColorScheme();

                bool restoreA = biomeA.objectReferenceValue == null ? true : false;
                bool restoreB = biomeB.objectReferenceValue == null ? true : false;
                planetGeneration.RestoreSavedColorScheme(restoreA, restoreB);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            
        }

        oceanFoldout = EditorGUILayout.Foldout(oceanFoldout, "Ocean Variables", true);
        if(oceanFoldout)
        {
            //Display ocean variables
            if(GUILayout.Button("Toggle Ocean")) { planetGeneration.ToggleOcean();  }

            EditorGUILayout.Separator();

            EditorGUILayout.Slider(seaLevelProperty, 0, 5);
            EditorGUILayout.Slider(oceanBlendEndProperty, 0, 1);
            EditorGUILayout.Slider(oceanBlendIntensityProperty, 0, 2);

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(shorelineColorProperty);
            EditorGUILayout.PropertyField(seabedColorProperty);
            EditorGUILayout.PropertyField(oceanColorProperty);

            EditorGUILayout.Separator();

            EditorGUILayout.Slider(oceanMetallicProperty, 0, 1);
            EditorGUILayout.Slider(oceanSmoothnessProperty, 0, 1);
        }
        atmosphereFoldout = EditorGUILayout.Foldout(atmosphereFoldout, "Atmosphere Variables", true);
        if (atmosphereFoldout)
        {
            //Display atmosphere variables
            if (GUILayout.Button("Toggle Atmosphere")) { planetGeneration.ToggleAtmosphere(); }

            innerAtmosphereFoldout = EditorGUILayout.Foldout(innerAtmosphereFoldout, "Inner Atmosphere", true);

            if (innerAtmosphereFoldout)
            {
                EditorGUILayout.PropertyField(atmosphereInnerColorProperty);
                EditorGUILayout.Slider(atmosphereInnerRangeProperty, 0, 10);
                EditorGUILayout.Slider(atmosphereInnerIntensityProperty, 0, 10);
            }

            outerAtmosphereFoldout = EditorGUILayout.Foldout(outerAtmosphereFoldout, "Outer Atmosphere", true);

            if (outerAtmosphereFoldout)
            {
                EditorGUILayout.PropertyField(atmosphereOuterColorProperty);
                EditorGUILayout.Slider(atmosphereOuterRangeProperty, 0, 10);
                EditorGUILayout.Slider(atmosphereOuterIntensityProperty, 0, 10);
            }

            frontAtmosphereFoldout = EditorGUILayout.Foldout(frontAtmosphereFoldout, "Front Atmosphere", true);

            if (frontAtmosphereFoldout)
            {
                EditorGUILayout.PropertyField(atmosphereFrontColorProperty);
                EditorGUILayout.Slider(atmosphereFrontRangeProperty, 0, 10);
                EditorGUILayout.Slider(atmosphereFrontIntensityProperty, 0, 10);
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
        planetObj = new SerializedObject(target);
        oceanVariables = planetObj.FindProperty("oceanVariables");
        Debug.Log(oceanVariables);
        atmosphereVariables = serializedObject.FindProperty("atmosphereVariables");
        seaLevelProperty = oceanVariables.FindPropertyRelative("_SeaLevel");
        oceanBlendEndProperty = oceanVariables.FindPropertyRelative("_OceanBlendEnd");
        oceanBlendIntensityProperty = oceanVariables.FindPropertyRelative("_OceanBlendIntensity");
        shorelineColorProperty = oceanVariables.FindPropertyRelative("_ShorelineColor");
        seabedColorProperty = oceanVariables.FindPropertyRelative("_SeabedColor");
        oceanColorProperty = oceanVariables.FindPropertyRelative("_OceanColor");
        oceanMetallicProperty = oceanVariables.FindPropertyRelative("_OceanMetallic");
        oceanSmoothnessProperty = oceanVariables.FindPropertyRelative("_OceanSmoothness");
        atmosphereOuterColorProperty = atmosphereVariables.FindPropertyRelative("_OuterBandColor");
        atmosphereOuterRangeProperty = atmosphereVariables.FindPropertyRelative("_OuterBandRange");
        atmosphereOuterIntensityProperty = atmosphereVariables.FindPropertyRelative("_OuterBandIntensity");
        atmosphereInnerColorProperty = atmosphereVariables.FindPropertyRelative("_InnerBandColor");
        atmosphereInnerRangeProperty = atmosphereVariables.FindPropertyRelative("_InnerBandRange");
        atmosphereInnerIntensityProperty = atmosphereVariables.FindPropertyRelative("_InnerBandIntensity");
        atmosphereFrontColorProperty = atmosphereVariables.FindPropertyRelative("_FrontColor");
        atmosphereFrontRangeProperty = atmosphereVariables.FindPropertyRelative("_FrontRange");
        atmosphereFrontIntensityProperty = atmosphereVariables.FindPropertyRelative("_FrontIntensity");
        #endregion
        planetGeneration = (FullPlanetGeneration)target;
        shadingEditor = (MaterialEditor)CreateEditor(planetGeneration.planet.planetMaterial);

        oceanFoldout = EditorPrefs.GetBool(nameof(oceanFoldout), false);
        atmosphereFoldout = EditorPrefs.GetBool(nameof(atmosphereFoldout), false);
        innerAtmosphereFoldout = EditorPrefs.GetBool(nameof(innerAtmosphereFoldout), false);
        outerAtmosphereFoldout = EditorPrefs.GetBool(nameof(outerAtmosphereFoldout), false);
        frontAtmosphereFoldout = EditorPrefs.GetBool(nameof(frontAtmosphereFoldout), false);
        terrainFoldout = EditorPrefs.GetBool(nameof(terrainFoldout), false);
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
        EditorPrefs.SetBool(nameof(colorSchemes), colorSchemes);
        EditorPrefs.SetBool(nameof(atmosphereFoldout), atmosphereFoldout);
        EditorPrefs.SetBool(nameof(innerAtmosphereFoldout), innerAtmosphereFoldout);
        EditorPrefs.SetBool(nameof(outerAtmosphereFoldout), outerAtmosphereFoldout);
        EditorPrefs.SetBool(nameof(frontAtmosphereFoldout), frontAtmosphereFoldout);
    }
}
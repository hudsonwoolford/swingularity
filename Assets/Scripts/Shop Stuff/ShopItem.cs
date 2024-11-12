using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CreateAssetMenu(fileName = "shopItem", menuName = "shopItem")]
public class ShopItem : ScriptableObject
{
    public ItemType itemType;
    public uint requiredSupernovae;
    public Sprite itemVisual;

    //different customization elements 
    [ConditionalProp("itemType", (int)ItemType.AstronautMaterial)]
    public MaterialCustomizationElement materialCustomizationElement;
    [ConditionalProp("itemType", (int)ItemType.GolfBallTrail)]
    public GolfBallTrailElement golfBallTrailElement;


    //type definitons
    public enum ItemType
    {
        AstronautMaterial,
        GolfBallTrail,
        AstronautHat
    };
    [System.Serializable]
    public struct MaterialCustomizationElement
    {
        public Color matColor;
        [Range(0f, 1f)]
        public float metallic;
        [Range(0f, 1f)]
        public float smoothness;
    }
    [System.Serializable]
    public struct GolfBallTrailElement
    {
        public GameObject trailRendObj;
    }
}
#if UNITY_EDITOR
#region Custom Inspector Stuff
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
public class ConditionalPropAttribute : UnityEngine.PropertyAttribute
{
    public bool isBoolean;
    public string EnumValue { get; private set; } = null;
    public int TargetEnumValue { get; private set; } = 0;
    public string BooleanName { get; private set; } = null;
    public ConditionalPropAttribute(string booleanName)
    {
        BooleanName = booleanName;
        isBoolean = true;
    }
    public ConditionalPropAttribute(string enumValue, int targetEnumValue)
    {
        EnumValue = enumValue;
        TargetEnumValue = targetEnumValue;

        isBoolean = false;
    }
}

[CustomPropertyDrawer(typeof(ConditionalPropAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get the condition attribute
        ConditionalPropAttribute conditionalField = (ConditionalPropAttribute)attribute;

        // Get the condition field from the serialized object
        SerializedProperty booleanProperty = property.serializedObject.FindProperty(conditionalField.BooleanName);
        SerializedProperty enumProperty = property.serializedObject.FindProperty(conditionalField.EnumValue);

        bool shouldShow = conditionalField.isBoolean
            ? booleanProperty != null && booleanProperty.boolValue
            : enumProperty != null && enumProperty.enumValueIndex == conditionalField.TargetEnumValue;

        // Only draw the field if the condition is met
        if (!shouldShow) { return; }

        property.isExpanded = !property.isExpanded ? true : property.isExpanded;
        EditorGUI.PropertyField(position, property, label, true); 
    }
    // Adjust the height of the property based on its visibility
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Get the condition attribute
        ConditionalPropAttribute conditionalField = (ConditionalPropAttribute)attribute;

        // Get the condition field from the serialized object
        SerializedProperty booleanProperty = property.serializedObject.FindProperty(conditionalField.BooleanName);
        SerializedProperty enumProperty = property.serializedObject.FindProperty(conditionalField.EnumValue);

        bool shouldShow = conditionalField.isBoolean
            ? booleanProperty != null && booleanProperty.boolValue
            : enumProperty != null && enumProperty.enumValueIndex == conditionalField.TargetEnumValue;

        return shouldShow ? EditorGUI.GetPropertyHeight(property) : 0;
    }
}
#endregion
#endif


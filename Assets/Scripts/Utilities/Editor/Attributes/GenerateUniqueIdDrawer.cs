// Assets/Scripts/Utilities/Editor/Attributes/GenerateUniqueIdDrawer.cs
using UnityEditor;
using UnityEngine;

namespace Utilities.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(GenerateUniqueIdAttribute))]
    public class GenerateUniqueIdDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                if (string.IsNullOrEmpty(property.stringValue))
                {
                    property.stringValue = System.Guid.NewGuid().ToString();
                }

                GUI.enabled = false;
                EditorGUI.PropertyField(position, property, label);
                GUI.enabled = true;
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use GenerateUniqueId with string.");
            }
        }
    }
}
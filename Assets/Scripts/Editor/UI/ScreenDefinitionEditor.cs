using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RecipeRage.UI.Core; // Namespace for ScreenDefinition and BaseScreenController

namespace RecipeRage.Editor.UI
{
    [CustomEditor(typeof(ScreenDefinition))]
    public class ScreenDefinitionEditor : UnityEditor.Editor // Explicitly use UnityEditor.Editor
    {
        private SerializedProperty _screenIdProp;
        private SerializedProperty _uxmlAssetProp;
        private SerializedProperty _controllerTypeNameProp; // The string we store
        private SerializedProperty _screenGroupProp;
        private SerializedProperty _persistStateProp;

        private string[] _controllerTypeNames = new string[0];
        private string[] _controllerDisplayNames = new string[0];
        private int _selectedIndex = 0;

        private void OnEnable()
        {
            // Find the serialized properties
            _screenIdProp = serializedObject.FindProperty("_screenId");
            _uxmlAssetProp = serializedObject.FindProperty("_uxmlAsset");
            _controllerTypeNameProp = serializedObject.FindProperty("_controllerTypeName");
            _screenGroupProp = serializedObject.FindProperty("_screenGroup");
            _persistStateProp = serializedObject.FindProperty("_persistState");

            // Find all controller types inheriting from BaseScreenController
            FindAllControllerTypes();

            // Determine the currently selected index based on the stored type name
            UpdateSelectedIndex();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update(); // Always start with this

            // Draw the default fields we want to keep as is
            EditorGUILayout.PropertyField(_screenIdProp);
            EditorGUILayout.PropertyField(_uxmlAssetProp);
            EditorGUILayout.PropertyField(_screenGroupProp);
            EditorGUILayout.PropertyField(_persistStateProp);

            EditorGUI.BeginChangeCheck(); // Start checking for changes in the popup

            // Draw the popup for controller selection
            _selectedIndex = EditorGUILayout.Popup(
                new GUIContent("Controller Type", "Select the C# controller script associated with this screen."),
                _selectedIndex,
                _controllerDisplayNames
            );

            if (EditorGUI.EndChangeCheck()) // If the popup value changed
            {
                // Update the stored string property based on the new selection
                if (_selectedIndex > 0 && _selectedIndex < _controllerTypeNames.Length) // Index 0 is "None"
                {
                    _controllerTypeNameProp.stringValue = _controllerTypeNames[_selectedIndex];
                }
                else
                {
                    _controllerTypeNameProp.stringValue = null; // Set to null if "None" is selected
                }
            }

            // Show the stored type name (optional, for debugging/verification)
            // EditorGUI.BeginDisabledGroup(true);
            // EditorGUILayout.TextField("Stored Type Name", _controllerTypeNameProp.stringValue ?? "None");
            // EditorGUI.EndDisabledGroup();


            serializedObject.ApplyModifiedProperties(); // Always end with this
        }

        private void FindAllControllerTypes()
        {
            List<string> typeNames = new List<string>();
            List<string> displayNames = new List<string>();

            // Add a "None" option first
            typeNames.Add(null);
            displayNames.Add("None");

            try
            {
                // Get all assemblies currently loaded in the AppDomain
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

                // Filter assemblies that might contain user scripts (heuristic)
                var relevantAssemblies = assemblies.Where(asm =>
                   !asm.IsDynamic &&
                   !asm.GetName().Name.StartsWith("Unity") &&
                   !asm.GetName().Name.StartsWith("System") &&
                   !asm.GetName().Name.StartsWith("Mono") &&
                   !asm.GetName().Name.StartsWith("mscorlib") &&
                   !asm.GetName().Name.StartsWith("netstandard")
               );

                // Find types inheriting from BaseScreenController
                var controllerTypes = relevantAssemblies
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(BaseScreenController)))
                    .OrderBy(type => type.Name); // Order alphabetically

                foreach (Type type in controllerTypes)
                {
                    // Store the full name (Namespace.ClassName) required by Type.GetType()
                    typeNames.Add(type.FullName);
                    // Display just the ClassName for readability in the dropdown
                    displayNames.Add(type.Name);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error finding BaseScreenController subclasses: {ex.Message}");
            }

            _controllerTypeNames = typeNames.ToArray();
            _controllerDisplayNames = displayNames.ToArray();
        }

        private void UpdateSelectedIndex()
        {
            // Find the index corresponding to the currently stored string value
            string currentTypeName = _controllerTypeNameProp.stringValue;
            _selectedIndex = Array.IndexOf(_controllerTypeNames, currentTypeName);
            if (_selectedIndex < 0) // If not found (e.g., class renamed or string empty), default to "None"
            {
                _selectedIndex = 0;
                // Optionally clear the invalid stored string if it wasn't null
                // if (!string.IsNullOrEmpty(currentTypeName)) {
                //    _controllerTypeNameProp.stringValue = null;
                // }
            }
        }
    }
}

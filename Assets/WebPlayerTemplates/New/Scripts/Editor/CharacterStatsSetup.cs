using System;
using Gameplay.Character.Stats;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class CharacterStatsSetup : EditorWindow
    {
        private GameObject _characterPrefab;
        private string _selectedClass;
        private StatConfigurationData _statConfig;

        private void OnGUI()
        {
            GUILayout.Label("Character Stats Setup", EditorStyles.boldLabel);

            _characterPrefab = EditorGUILayout.ObjectField("Character Prefab",
                _characterPrefab, typeof(GameObject), false) as GameObject;

            _statConfig = EditorGUILayout.ObjectField("Stat Configuration",
                _statConfig, typeof(StatConfigurationData), false) as StatConfigurationData;

            if (_statConfig != null && _statConfig.characterClasses != null)
            {
                string[] classNames = new string[_statConfig.characterClasses.Length];
                for (int i = 0; i < _statConfig.characterClasses.Length; i++)
                {
                    classNames[i] = _statConfig.characterClasses[i].className;
                }

                int selectedIndex = Array.IndexOf(classNames, _selectedClass);
                selectedIndex = EditorGUILayout.Popup("Character Class", selectedIndex, classNames);
                if (selectedIndex >= 0)
                {
                    _selectedClass = classNames[selectedIndex];
                }
            }

            if (GUILayout.Button("Apply Stats"))
            {
                ApplyStatsToCharacter();
            }
        }

        [MenuItem("Game/Character Stats Setup")]
        public static void ShowWindow()
        {
            GetWindow<CharacterStatsSetup>("Character Stats Setup");
        }

        private void ApplyStatsToCharacter()
        {
            if (_characterPrefab == null || _statConfig == null || string.IsNullOrEmpty(_selectedClass))
            {
                EditorUtility.DisplayDialog("Error", "Please select all required fields", "OK");
                return;
            }

            // Get or add CharacterStats component
            string prefabPath = AssetDatabase.GetAssetPath(_characterPrefab);
            GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            CharacterStats statsComponent = prefab.GetComponent<CharacterStats>();

            if (statsComponent == null)
            {
                statsComponent = prefab.AddComponent<CharacterStats>();
            }

            // Find the stat configuration for the selected class
            StatConfigurationData.CharacterClassStats classStats = Array.Find(_statConfig.characterClasses,
                c => c.className == _selectedClass);

            if (classStats != null)
            {
                // Apply the stats
                SerializedObject serializedObject = new SerializedObject(statsComponent);
                SerializedProperty statConfigProp = serializedObject.FindProperty("statConfiguration");
                statConfigProp.objectReferenceValue = _statConfig;
                serializedObject.ApplyModifiedProperties();

                // Save the prefab
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
                PrefabUtility.UnloadPrefabContents(prefab);

                EditorUtility.DisplayDialog("Success",
                    $"Applied {_selectedClass} stats to {_characterPrefab.name}", "OK");
            }
        }
    }
}
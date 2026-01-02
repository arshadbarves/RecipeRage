using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;

namespace Core.Localization.Editor
{
    public class LocalizationEditorWindow : EditorWindow
    {
        private string _csvPath = "Assets/Resources/Data/Localization.csv";
        private Vector2 _scrollPosition;
        private string _validationResult = "";
        private string _newKey = "";
        private string _newEnglish = "";
        private string _newSpanish = "";

        [MenuItem("Tools/Localization Manager")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationEditorWindow>("Localization Manager");
        }

        private void OnGUI()
        {
            GUILayout.Label("Localization Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // CSV Path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CSV File:", GUILayout.Width(100));
            _csvPath = EditorGUILayout.TextField(_csvPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFilePanel("Select CSV File", "Assets/Resources/Data", "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    _csvPath = "Assets" + path.Replace(Application.dataPath, "");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Validation Section
            GUILayout.Label("Validation", EditorStyles.boldLabel);
            if (GUILayout.Button("Validate All Keys"))
            {
                ValidateCSV();
            }

            if (!string.IsNullOrEmpty(_validationResult))
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));
                EditorGUILayout.HelpBox(_validationResult, MessageType.Info);
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space();

            // Add New Key Section
            GUILayout.Label("Add New Key", EditorStyles.boldLabel);
            _newKey = EditorGUILayout.TextField("Key:", _newKey);
            _newEnglish = EditorGUILayout.TextField("English:", _newEnglish);
            _newSpanish = EditorGUILayout.TextField("Spanish:", _newSpanish);

            if (GUILayout.Button("Add Key to CSV"))
            {
                AddKeyToCSV();
            }

            EditorGUILayout.Space();

            // Quick Actions
            GUILayout.Label("Quick Actions", EditorStyles.boldLabel);
            if (GUILayout.Button("Open CSV in External Editor"))
            {
                System.Diagnostics.Process.Start(_csvPath);
            }

            if (GUILayout.Button("Refresh Asset Database"))
            {
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Success", "Asset database refreshed!", "OK");
            }

            EditorGUILayout.Space();

            // Tips
            EditorGUILayout.HelpBox(
                "Tips:\n" +
                "- Use quotes for values containing commas\n" +
                "- Use {0}, {1}, etc. for string formatting\n" +
                "- Add _zero, _one, _other suffixes for pluralization",
                MessageType.Info);
        }

        private void ValidateCSV()
        {
            if (!File.Exists(_csvPath))
            {
                _validationResult = "CSV file not found!";
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(_csvPath);
                if (lines.Length == 0)
                {
                    _validationResult = "CSV file is empty!";
                    return;
                }

                // Parse header
                string[] headers = lines[0].Split(',');
                int languageCount = headers.Length - 1; // Exclude Key column

                StringBuilder result = new StringBuilder();
                result.AppendLine($"✓ Found {languageCount} languages: {string.Join(", ", headers.Skip(1))}");
                result.AppendLine($"✓ Total keys: {lines.Length - 1}");
                result.AppendLine();

                // Check for missing translations
                int missingCount = 0;
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] parts = ParseCSVLine(lines[i]);
                    if (parts.Length < headers.Length)
                    {
                        result.AppendLine($"⚠ Key '{parts[0]}' has missing translations");
                        missingCount++;
                    }
                    else
                    {
                        for (int j = 1; j < parts.Length; j++)
                        {
                            if (string.IsNullOrWhiteSpace(parts[j]))
                            {
                                result.AppendLine($"⚠ Key '{parts[0]}' missing translation for '{headers[j]}'");
                                missingCount++;
                            }
                        }
                    }
                }

                if (missingCount == 0)
                {
                    result.AppendLine("✓ All keys have complete translations!");
                }
                else
                {
                    result.AppendLine($"\n⚠ Found {missingCount} missing translations");
                }

                _validationResult = result.ToString();
            }
            catch (System.Exception ex)
            {
                _validationResult = $"Error validating CSV: {ex.Message}";
            }
        }

        private void AddKeyToCSV()
        {
            if (string.IsNullOrWhiteSpace(_newKey))
            {
                EditorUtility.DisplayDialog("Error", "Key cannot be empty!", "OK");
                return;
            }

            if (!File.Exists(_csvPath))
            {
                EditorUtility.DisplayDialog("Error", "CSV file not found!", "OK");
                return;
            }

            try
            {
                // Read existing content
                string[] lines = File.ReadAllLines(_csvPath);

                // Check if key already exists
                foreach (var line in lines.Skip(1))
                {
                    string[] parts = ParseCSVLine(line);
                    if (parts.Length > 0 && parts[0].Trim() == _newKey.Trim())
                    {
                        EditorUtility.DisplayDialog("Error", $"Key '{_newKey}' already exists!", "OK");
                        return;
                    }
                }

                // Escape values if they contain commas
                string escapedEnglish = EscapeCSVValue(_newEnglish);
                string escapedSpanish = EscapeCSVValue(_newSpanish);

                // Add new line
                string newLine = $"{_newKey},{escapedEnglish},{escapedSpanish}";
                File.AppendAllText(_csvPath, "\n" + newLine);

                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog("Success", $"Key '{_newKey}' added successfully!", "OK");

                // Clear fields
                _newKey = "";
                _newEnglish = "";
                _newSpanish = "";
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to add key: {ex.Message}", "OK");
            }
        }

        private string[] ParseCSVLine(string line)
        {
            // Simple CSV parser - you can use the same robust parser from LocalizationManager if needed
            return line.Split(',');
        }

        private string EscapeCSVValue(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}


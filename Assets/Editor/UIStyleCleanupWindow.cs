using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RecipeRage.Editor
{
    public class UIStyleCleanupWindow : EditorWindow
    {
        private string _pythonPath = "python3";
        private string _scriptPath = "tools/cleanup_ui_styles.py";
        private string _output = "";
        private Vector2 _scrollPosition;
        private bool _isScanning;

        [System.Serializable]
        public class CleanupIssue
        {
            public string type;
            public string file;
            public string[] classes;
            public string[] styles;
        }

        private List<CleanupIssue> _issues = new List<CleanupIssue>();

        [MenuItem("Tools/UI/Style Cleanup Tool")]
        public static void ShowWindow()
        {
            GetWindow<UIStyleCleanupWindow>("UI Style Cleanup");
        }

        private void OnEnable()
        {
            _scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "tools", "cleanup_ui_styles.py");
        }

        private void OnGUI()
        {
            GUILayout.Label("UI Style Cleanup Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _pythonPath = EditorGUILayout.TextField("Python Path", _pythonPath);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Run Scan", GUILayout.Height(30)))
            {
                RunPythonScript(false);
            }

            GUI.enabled = _issues.Count > 0;
            if (GUILayout.Button("Perform Cleanup", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Confirm Cleanup", 
                    "Are you sure you want to remove unused styles? This will modify UXML and USS files.", "Yes", "No"))
                {
                    RunPythonScript(true);
                }
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILayout.Label($"Found Issues: {_issues.Count}", EditorStyles.boldLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            foreach (var issue in _issues)
            {
                DrawIssue(issue);
            }

            if (!string.IsNullOrEmpty(_output) && _issues.Count == 0)
            {
                EditorGUILayout.HelpBox(_output, MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawIssue(CleanupIssue issue)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            Color iconColor = Color.white;
            string title = "";
            string details = "";

            switch (issue.type)
            {
                case "UXML_BROKEN_STYLE":
                    iconColor = Color.red;
                    title = "Broken Style Reference";
                    details = "Styles: " + string.Join(", ", issue.styles);
                    break;
                case "UXML_MISSING_CLASS":
                    iconColor = Color.yellow;
                    title = "Missing Class Definition";
                    details = "Classes: " + string.Join(", ", issue.classes);
                    break;
                case "USS_UNUSED_CLASS":
                    iconColor = Color.cyan;
                    title = "Unused USS Class";
                    details = "Classes: " + string.Join(", ", issue.classes);
                    break;
            }

            EditorGUILayout.BeginHorizontal();
            var rect = GUILayoutUtility.GetRect(16, 16, GUILayout.Width(16));
            EditorGUI.DrawRect(rect, iconColor);
            GUILayout.Label(title, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            GUILayout.Label(Path.GetFileName(issue.file), EditorStyles.miniLabel);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(details, EditorStyles.wordWrappedLabel);
            
            if (issue.type != "UXML_BROKEN_STYLE")
            {
                if (GUILayout.Button("Clean", GUILayout.Width(50)))
                {
                    if (EditorUtility.DisplayDialog("Clean File", 
                        $"Clean unused classes in {Path.GetFileName(issue.file)}?", "Yes", "No"))
                    {
                        RunPythonScript(true, issue.file);
                    }
                }
            }
            
            if (GUILayout.Button("Open", GUILayout.Width(50)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(GetRelativePath(issue.file));
                if (asset != null) AssetDatabase.OpenAsset(asset);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private string GetRelativePath(string fullPath)
        {
            string projectRoot = Directory.GetCurrentDirectory();
            if (fullPath.StartsWith(projectRoot))
            {
                return fullPath.Substring(projectRoot.Length + 1);
            }
            return fullPath;
        }

        private void RunPythonScript(bool cleanup, string targetFile = null)
        {
            _isScanning = true;
            if (targetFile == null) _issues.Clear(); // Only clear all if scanning everything
            _output = "Running...";

            string arguments = $"\"{_scriptPath}\" --json";
            if (cleanup) arguments += " --cleanup";
            if (!string.IsNullOrEmpty(targetFile)) arguments += $" --file \"{targetFile}\"";

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = _pythonPath;
            start.Arguments = arguments;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.CreateNoWindow = true;
            start.WorkingDirectory = Directory.GetCurrentDirectory();

            try
            {
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();

                        if (!string.IsNullOrEmpty(error))
                        {
                            UnityEngine.Debug.LogError($"Python Error: {error}");
                        }

                        if (!string.IsNullOrEmpty(result))
                        {
                            try
                            {
                                // Simple JSON list parsing
                                string wrappedJson = "{\"items\":" + result + "}";
                                var container = JsonUtility.FromJson<IssueContainer>(wrappedJson);
                                
                                if (string.IsNullOrEmpty(targetFile))
                                {
                                    _issues = container.items;
                                }
                                else
                                {
                                    // Refresh the issues but keep others? Actually easier to just re-scan for simplicity
                                    // or just update the entry. Let's just re-scan for now to ensure consistency.
                                    RunPythonScript(false); 
                                    return;
                                }
                                _output = _issues.Count == 0 ? "No issues found." : "";
                            }
                            catch (System.Exception e)
                            {
                                UnityEngine.Debug.LogError($"Failed to parse JSON: {e.Message}\nResult: {result}");
                                _output = result;
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to run script: {e.Message}");
                _output = $"Error: {e.Message}";
            }

            _isScanning = false;
            Repaint();
        }

        [System.Serializable]
        private class IssueContainer
        {
            public List<CleanupIssue> items;
        }
    }
}

using UnityEngine;
using UnityEditor;
using Gameplay.Spawning;

namespace Editor
{
    /// <summary>
    /// Custom editor for SpawnPoint to make scene setup easier
    /// </summary>
    [CustomEditor(typeof(SpawnPoint))]
    public class SpawnPointEditor : UnityEditor.Editor
    {
        private SerializedProperty _teamCategory;
        private SerializedProperty _isAvailable;
        private SerializedProperty _spawnRadius;
        private SerializedProperty _showGizmos;
        private SerializedProperty _gizmoColor;

        private void OnEnable()
        {
            _teamCategory = serializedObject.FindProperty("_teamCategory");
            _isAvailable = serializedObject.FindProperty("_isAvailable");
            _spawnRadius = serializedObject.FindProperty("_spawnRadius");
            _showGizmos = serializedObject.FindProperty("_showGizmos");
            _gizmoColor = serializedObject.FindProperty("_gizmoColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Spawn Point Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Team Category
            EditorGUILayout.PropertyField(_teamCategory, new GUIContent("Team Category", "Which team can use this spawn point"));

            // Show info box based on team
            TeamCategory team = (TeamCategory)_teamCategory.enumValueIndex;
            switch (team)
            {
                case TeamCategory.Neutral:
                    EditorGUILayout.HelpBox("Neutral spawn points can be used by any team. Good for free-for-all modes.", MessageType.Info);
                    break;
                case TeamCategory.TeamA:
                    EditorGUILayout.HelpBox("Team A spawn points (Blue). Only Team A players will spawn here.", MessageType.Info);
                    break;
                case TeamCategory.TeamB:
                    EditorGUILayout.HelpBox("Team B spawn points (Red). Only Team B players will spawn here.", MessageType.Info);
                    break;
            }

            EditorGUILayout.Space();

            // Availability
            EditorGUILayout.PropertyField(_isAvailable, new GUIContent("Is Available", "Whether this spawn point is currently available for use"));

            // Spawn Radius
            EditorGUILayout.PropertyField(_spawnRadius, new GUIContent("Spawn Radius", "Radius for random position offset"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Visual Settings", EditorStyles.boldLabel);

            // Show Gizmos
            EditorGUILayout.PropertyField(_showGizmos, new GUIContent("Show Gizmos", "Display visual indicators in scene view"));
            EditorGUILayout.PropertyField(_gizmoColor, new GUIContent("Gizmo Color", "Custom gizmo color (overrides team color)"));

            serializedObject.ApplyModifiedProperties();

            // Quick Actions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Team A"))
            {
                _teamCategory.enumValueIndex = (int)TeamCategory.TeamA;
                serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Set Team B"))
            {
                _teamCategory.enumValueIndex = (int)TeamCategory.TeamB;
                serializedObject.ApplyModifiedProperties();
            }
            if (GUILayout.Button("Set Neutral"))
            {
                _teamCategory.enumValueIndex = (int)TeamCategory.Neutral;
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// Menu items for creating spawn points
    /// </summary>
    public static class SpawnPointMenuItems
    {
        [MenuItem("GameObject/RecipeRage/Spawning/Spawn Point - Neutral", false, 10)]
        private static void CreateNeutralSpawnPoint()
        {
            CreateSpawnPoint(TeamCategory.Neutral);
        }

        [MenuItem("GameObject/RecipeRage/Spawning/Spawn Point - Team A", false, 11)]
        private static void CreateTeamASpawnPoint()
        {
            CreateSpawnPoint(TeamCategory.TeamA);
        }

        [MenuItem("GameObject/RecipeRage/Spawning/Spawn Point - Team B", false, 12)]
        private static void CreateTeamBSpawnPoint()
        {
            CreateSpawnPoint(TeamCategory.TeamB);
        }

        [MenuItem("GameObject/RecipeRage/Spawning/Spawn Manager", false, 13)]
        private static void CreateSpawnManager()
        {
            GameObject go = new GameObject("SpawnManager");
            go.AddComponent<SpawnManager>();
            Selection.activeGameObject = go;
            Undo.RegisterCreatedObjectUndo(go, "Create Spawn Manager");
        }

        private static void CreateSpawnPoint(TeamCategory team)
        {
            GameObject go = new GameObject($"SpawnPoint_{team}");
            SpawnPoint spawnPoint = go.AddComponent<SpawnPoint>();
            
            // Use reflection to set private field
            var field = typeof(SpawnPoint).GetField("_teamCategory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(spawnPoint, team);
            }

            // Position at scene view camera if available
            if (SceneView.lastActiveSceneView != null)
            {
                go.transform.position = SceneView.lastActiveSceneView.camera.transform.position + SceneView.lastActiveSceneView.camera.transform.forward * 5f;
            }

            Selection.activeGameObject = go;
            Undo.RegisterCreatedObjectUndo(go, $"Create {team} Spawn Point");
        }
    }
}

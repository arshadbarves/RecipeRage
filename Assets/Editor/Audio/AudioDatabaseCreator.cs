using UnityEngine;
using UnityEditor;
using System.IO;

namespace Core.Audio.Editor
{
    /// <summary>
    /// Editor utility for creating the audio database asset.
    /// </summary>
    public static class AudioDatabaseCreator
    {
        private const string DATABASE_PATH = "Assets/Audio/AudioDatabase.asset";
        private const string DATABASE_DIRECTORY = "Assets/Audio";
        
        [MenuItem("RecipeRage/Audio/Create Audio Database")]
        public static void CreateAudioDatabase()
        {
            // Create directory if it doesn't exist
            if (!Directory.Exists(DATABASE_DIRECTORY))
            {
                Directory.CreateDirectory(DATABASE_DIRECTORY);
                AssetDatabase.Refresh();
            }
            
            // Check if database already exists
            if (File.Exists(DATABASE_PATH))
            {
                Debug.Log($"[AudioDatabaseCreator] Audio database already exists at {DATABASE_PATH}");
                
                // Select the existing database
                AudioDatabase existingDatabase = AssetDatabase.LoadAssetAtPath<AudioDatabase>(DATABASE_PATH);
                Selection.activeObject = existingDatabase;
                
                return;
            }
            
            // Create the audio database
            AudioDatabase database = ScriptableObject.CreateInstance<AudioDatabase>();
            
            // Save the database asset
            AssetDatabase.CreateAsset(database, DATABASE_PATH);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"[AudioDatabaseCreator] Created audio database at {DATABASE_PATH}");
            
            // Select the database in the Project window
            Selection.activeObject = database;
        }
    }
}

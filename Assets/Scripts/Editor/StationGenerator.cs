using System.Collections.Generic;
using RecipeRage.Gameplay.Stations;
using UnityEditor;
using UnityEngine;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Generates cooking station prefabs for the RecipeRage game.
    /// </summary>
    public class StationGenerator
    {
        // List of station definitions
        private readonly List<StationDefinition> _stationDefinitions = new List<StationDefinition>
        {
            // Cutting Station
            new StationDefinition
            {
                Name = "CuttingStation",
                DisplayName = "Cutting Board",
                StationType = typeof(CuttingStation),
                BaseColor = new Color(0.8f, 0.6f, 0.4f), // Brown
                ProcessingTime = 3f,
                PrefabPath = "Assets/Prefabs/Stations/CuttingStation.prefab",
                ModelPath = "Assets/Models/Stations/CuttingBoard.fbx",
                SoundPath = "Assets/Audio/Stations/Cutting.wav"
            },
            
            // Cooking Pot
            new StationDefinition
            {
                Name = "CookingPot",
                DisplayName = "Cooking Pot",
                StationType = typeof(CookingPot),
                BaseColor = new Color(0.6f, 0.6f, 0.6f), // Gray
                ProcessingTime = 5f,
                PrefabPath = "Assets/Prefabs/Stations/CookingPot.prefab",
                ModelPath = "Assets/Models/Stations/Pot.fbx",
                SoundPath = "Assets/Audio/Stations/Boiling.wav"
            },
            
            // Assembly Station
            new StationDefinition
            {
                Name = "AssemblyStation",
                DisplayName = "Assembly Station",
                StationType = typeof(AssemblyStation),
                BaseColor = new Color(0.9f, 0.9f, 0.9f), // White
                ProcessingTime = 0f,
                PrefabPath = "Assets/Prefabs/Stations/AssemblyStation.prefab",
                ModelPath = "Assets/Models/Stations/Counter.fbx",
                SoundPath = "Assets/Audio/Stations/Place.wav"
            }
        };
        
        /// <summary>
        /// Generate cooking station prefabs.
        /// </summary>
        /// <param name="outputPath">The output path for the generated prefabs.</param>
        public void GenerateStations(string outputPath)
        {
            // Create the output directory if it doesn't exist
            System.IO.Directory.CreateDirectory(outputPath);
            
            // Generate each station
            foreach (StationDefinition definition in _stationDefinitions)
            {
                // Create the station game object
                GameObject stationObject = new GameObject(definition.Name);
                
                // Add the station component
                CookingStation stationComponent = (CookingStation)stationObject.AddComponent(definition.StationType);
                
                // Add a box collider
                BoxCollider collider = stationObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(1f, 1f, 1f);
                collider.center = new Vector3(0f, 0.5f, 0f);
                
                // Add a rigidbody
                Rigidbody rigidbody = stationObject.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                
                // Add an audio source
                AudioSource audioSource = stationObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f;
                audioSource.minDistance = 1f;
                audioSource.maxDistance = 10f;
                
                // Create a visual representation
                GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visualObject.name = "Visual";
                visualObject.transform.SetParent(stationObject.transform);
                visualObject.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                visualObject.transform.localScale = new Vector3(1f, 1f, 1f);
                
                // Set the material color
                MeshRenderer renderer = visualObject.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    Material material = new Material(Shader.Find("Standard"));
                    material.color = definition.BaseColor;
                    renderer.material = material;
                }
                
                // Create an ingredient placement point
                GameObject placementPoint = new GameObject("IngredientPlacementPoint");
                placementPoint.transform.SetParent(stationObject.transform);
                placementPoint.transform.localPosition = new Vector3(0f, 1.1f, 0f);
                
                // Create a progress bar
                GameObject progressBar = new GameObject("ProgressBar");
                progressBar.transform.SetParent(stationObject.transform);
                progressBar.transform.localPosition = new Vector3(0f, 1.5f, 0f);
                
                // Add a sprite renderer for the progress bar
                SpriteRenderer progressRenderer = progressBar.AddComponent<SpriteRenderer>();
                progressRenderer.sprite = EditorGUIUtility.whiteTexture;
                progressRenderer.color = Color.green;
                progressRenderer.transform.localScale = new Vector3(1f, 0.1f, 1f);
                
                // Set up the station properties
                SerializedObject serializedObject = new SerializedObject(stationComponent);
                serializedObject.FindProperty("_stationName").stringValue = definition.DisplayName;
                serializedObject.FindProperty("_processingTime").floatValue = definition.ProcessingTime;
                serializedObject.FindProperty("_ingredientPlacementPoint").objectReferenceValue = placementPoint.transform;
                serializedObject.FindProperty("_progressBarPrefab").objectReferenceValue = progressBar;
                
                // Apply the changes
                serializedObject.ApplyModifiedProperties();
                
                // Save the prefab
                string prefabPath = $"{outputPath}/{definition.Name}.prefab";
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(stationObject, prefabPath);
                
                // Clean up
                Object.DestroyImmediate(stationObject);
                
                Debug.Log($"Created station prefab: {definition.Name}");
            }
            
            // Save the changes
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Generated {_stationDefinitions.Count} station prefabs.");
        }
        
        /// <summary>
        /// Definition for a cooking station.
        /// </summary>
        private class StationDefinition
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public System.Type StationType { get; set; }
            public Color BaseColor { get; set; }
            public float ProcessingTime { get; set; }
            public string PrefabPath { get; set; }
            public string ModelPath { get; set; }
            public string SoundPath { get; set; }
        }
    }
}

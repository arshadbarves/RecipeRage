using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace RecipeRage.Editor
{
    public class FallGuysStyleDemo
    {
        [MenuItem("RecipeRage/Demo/Create Fall Guys Style Demo")]
        public static void CreateFallGuysStyleDemo()
        {
            // Create a new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "FallGuysStyleDemo";
            
            // Create a camera
            var cameraObj = new GameObject("Main Camera");
            var camera = cameraObj.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.4f, 0.6f, 0.9f);
            cameraObj.transform.position = new Vector3(0, 1.5f, -5);
            cameraObj.tag = "MainCamera";
            
            // Create a directional light
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            
            // Create a floor
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(10, 1, 10);
            
            // Create some primitive objects
            CreatePrimitiveWithStyle(PrimitiveType.Sphere, "Sphere", new Vector3(-2, 1, 0), Color.red);
            CreatePrimitiveWithStyle(PrimitiveType.Cube, "Cube", new Vector3(0, 1, 0), Color.blue);
            CreatePrimitiveWithStyle(PrimitiveType.Capsule, "Capsule", new Vector3(2, 1, 0), Color.green);
            
            // Save the scene
            string scenePath = "Assets/Scenes/FallGuysStyleDemo.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            
            Debug.Log($"Fall Guys Style Demo scene created at {scenePath}");
        }
        
        private static void CreatePrimitiveWithStyle(PrimitiveType type, string name, Vector3 position, Color color)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.position = position;
            
            // Try to find the shader
            var shader = Shader.Find("Custom/FallGuysStyleShader");
            if (shader == null)
            {
                shader = Shader.Find("Custom/FallGuysStyleShaderSimplified");
            }
            
            if (shader != null)
            {
                // Create a material with the shader
                var material = new Material(shader);
                material.SetColor("_Color", color);
                material.SetColor("_RimColor", Color.white);
                
                // Apply the material
                obj.GetComponent<Renderer>().material = material;
                
                // Add the style applier component
                var styleApplier = obj.AddComponent<RecipeRage.Utilities.FallGuysStyleApplier>();
                
                // Create a new material asset
                string materialPath = $"Assets/Materials/{name}Material.mat";
                AssetDatabase.CreateAsset(material, materialPath);
                AssetDatabase.SaveAssets();
                
                Debug.Log($"Created material at {materialPath}");
            }
            else
            {
                Debug.LogWarning("Fall Guys Style Shader not found. Using default material.");
            }
        }
    }
}

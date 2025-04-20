using UnityEngine;
using UnityEditor;
using RecipeRage.Utilities;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Editor menu for applying stylized materials to objects.
    /// </summary>
    public class StylizedMaterialMenu : MonoBehaviour
    {
        private const string SHADER_PATH_FULL = "Custom/StylizedUnlitShader";
        private const string SHADER_PATH_MOBILE = "Custom/StylizedUnlitShaderMobile";
        private const string MATERIAL_PATH = "Assets/Materials/StylizedMaterial.mat";
        private const string MATERIAL_MOBILE_PATH = "Assets/Materials/StylizedMaterialMobile.mat";
        
        [MenuItem("RecipeRage/Materials/Apply Stylized Material")]
        private static void ApplyStylizedMaterial()
        {
            // Check if any objects are selected
            if (Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Objects Selected", "Please select at least one object to apply the stylized material to.", "OK");
                return;
            }
            
            // Find or create the material
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_PATH);
            if (material == null)
            {
                // Create the material if it doesn't exist
                material = CreateStylizedMaterial(false);
            }
            
            // Apply the material to the selected objects
            ApplyMaterialToSelection(material, false);
        }
        
        [MenuItem("RecipeRage/Materials/Apply Stylized Material (Mobile)")]
        private static void ApplyStylizedMaterialMobile()
        {
            // Check if any objects are selected
            if (Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Objects Selected", "Please select at least one object to apply the stylized material to.", "OK");
                return;
            }
            
            // Find or create the material
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MATERIAL_MOBILE_PATH);
            if (material == null)
            {
                // Create the material if it doesn't exist
                material = CreateStylizedMaterial(true);
            }
            
            // Apply the material to the selected objects
            ApplyMaterialToSelection(material, true);
        }
        
        [MenuItem("RecipeRage/Materials/Create Stylized Material")]
        private static void CreateStylizedMaterialMenu()
        {
            CreateStylizedMaterial(false);
            EditorUtility.DisplayDialog("Material Created", "Stylized material created at " + MATERIAL_PATH, "OK");
        }
        
        [MenuItem("RecipeRage/Materials/Create Stylized Material (Mobile)")]
        private static void CreateStylizedMaterialMobileMenu()
        {
            CreateStylizedMaterial(true);
            EditorUtility.DisplayDialog("Material Created", "Mobile stylized material created at " + MATERIAL_MOBILE_PATH, "OK");
        }
        
        /// <summary>
        /// Creates a stylized material.
        /// </summary>
        /// <param name="isMobile">Whether to create a mobile-optimized material</param>
        /// <returns>The created material</returns>
        private static Material CreateStylizedMaterial(bool isMobile)
        {
            // Find the shader
            string shaderPath = isMobile ? SHADER_PATH_MOBILE : SHADER_PATH_FULL;
            Shader shader = Shader.Find(shaderPath);
            
            if (shader == null)
            {
                Debug.LogError($"Shader '{shaderPath}' not found!");
                return null;
            }
            
            // Create the material
            Material material = new Material(shader);
            
            // Set default properties
            material.SetColor("_Color", Color.white);
            material.SetColor("_AmbientColor", new Color(0.5f, 0.5f, 0.5f, 1f));
            material.SetFloat("_AmbientIntensity", 0.5f);
            material.SetVector("_LightDirection", new Vector4(0.5f, 0.5f, 0f, 0f));
            material.SetColor("_LightColor", Color.white);
            material.SetFloat("_LightIntensity", 1f);
            
            if (!isMobile)
            {
                material.SetFloat("_ShadowSoftness", 0.5f);
                material.SetFloat("_Metallic", 0f);
                material.SetFloat("_Roughness", 0.5f);
                material.SetFloat("_RimIntensity", 0.5f);
            }
            
            material.SetColor("_SpecularColor", Color.white);
            material.SetFloat("_Glossiness", isMobile ? 32f : 64f);
            material.SetFloat("_SpecularIntensity", 1f);
            material.SetColor("_RimColor", Color.white);
            material.SetFloat("_RimPower", 3f);
            
            // Save the material
            string materialPath = isMobile ? MATERIAL_MOBILE_PATH : MATERIAL_PATH;
            
            // Create the directory if it doesn't exist
            string directory = System.IO.Path.GetDirectoryName(materialPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }
            
            AssetDatabase.CreateAsset(material, materialPath);
            AssetDatabase.SaveAssets();
            
            return material;
        }
        
        /// <summary>
        /// Applies a material to the selected objects.
        /// </summary>
        /// <param name="material">The material to apply</param>
        /// <param name="isMobile">Whether this is a mobile-optimized material</param>
        private static void ApplyMaterialToSelection(Material material, bool isMobile)
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                // Check if the object has a renderer
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer == null)
                {
                    Debug.LogWarning($"Object '{obj.name}' does not have a Renderer component!");
                    continue;
                }
                
                // Apply the material
                renderer.material = material;
                
                // Add the StylizedMaterialController component
                StylizedMaterialController controller = obj.GetComponent<StylizedMaterialController>();
                if (controller == null)
                {
                    controller = obj.AddComponent<StylizedMaterialController>();
                }
                
                // Configure the controller
                controller._stylizedMaterial = material;
                controller._createMaterialInstance = true;
                controller.CreateMaterialInstance();
                
                Debug.Log($"Applied stylized material to '{obj.name}'");
            }
        }
    }
}

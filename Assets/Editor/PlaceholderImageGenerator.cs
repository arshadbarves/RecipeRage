using System.IO;
using UnityEditor;
using UnityEngine;

namespace RecipeRage.Editor
{
    public class PlaceholderImageGenerator : EditorWindow
    {
        private static readonly string[] ImageNames = new string[]
        {
            "logo",
            "main-menu-bg",
            "settings-bg",
            "gamemode-bg",
            "character-bg",
            "avatar",
            "trophy",
            "gem",
            "coin",
            "energy",
            "settings",
            "back-arrow",
            "character1",
            "character2",
            "character-icon",
            "event-icon",
            "timer",
            "shop",
            "brawlers",
            "news",
            "friends",
            "club",
            "chat",
            "pass",
            "ability-icon",
            "character-portrait",
            "character-frame",
            "character-full",
            "rarity-icon",
            "lock",
            "classic-mode",
            "time-attack-mode",
            "team-battle-mode",
            "challenge-mode",
            "players-icon",
            "time-icon",
            "difficulty-icon"
        };

        [MenuItem("RecipeRage/Generate Placeholder Images")]
        public static void GeneratePlaceholderImages()
        {
            string resourcesPath = "Assets/Resources/UI/Images";
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(resourcesPath))
            {
                Directory.CreateDirectory(resourcesPath);
            }
            
            foreach (string imageName in ImageNames)
            {
                string assetPath = $"{resourcesPath}/{imageName}.png";
                
                // Skip if the image already exists
                if (File.Exists(assetPath))
                {
                    continue;
                }
                
                // Create a texture
                Texture2D texture = new Texture2D(256, 256);
                
                // Generate a color based on the image name
                Color color = GetColorFromName(imageName);
                
                // Fill the texture with the color
                for (int y = 0; y < texture.height; y++)
                {
                    for (int x = 0; x < texture.width; x++)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
                
                // Add a border
                Color borderColor = Color.white;
                for (int x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, 0, borderColor);
                    texture.SetPixel(x, texture.height - 1, borderColor);
                }
                
                for (int y = 0; y < texture.height; y++)
                {
                    texture.SetPixel(0, y, borderColor);
                    texture.SetPixel(texture.width - 1, y, borderColor);
                }
                
                // Apply changes
                texture.Apply();
                
                // Save the texture as PNG
                byte[] bytes = texture.EncodeToPNG();
                File.WriteAllBytes(assetPath, bytes);
                
                // Import the asset
                AssetDatabase.ImportAsset(assetPath);
                
                // Set texture import settings
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.mipmapEnabled = false;
                    importer.SaveAndReimport();
                }
                
                Debug.Log($"Created placeholder image: {assetPath}");
            }
            
            AssetDatabase.Refresh();
            Debug.Log("Placeholder image generation complete!");
        }
        
        private static Color GetColorFromName(string name)
        {
            // Generate a hash from the name
            int hash = name.GetHashCode();
            
            // Use the hash to generate RGB values
            float r = ((hash & 0xFF0000) >> 16) / 255f;
            float g = ((hash & 0x00FF00) >> 8) / 255f;
            float b = (hash & 0x0000FF) / 255f;
            
            // Ensure the color is not too dark
            r = Mathf.Max(0.2f, r);
            g = Mathf.Max(0.2f, g);
            b = Mathf.Max(0.2f, b);
            
            return new Color(r, g, b);
        }
    }
}

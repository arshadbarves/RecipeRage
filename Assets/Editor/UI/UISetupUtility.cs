using System.IO;
using UnityEditor;
using UnityEngine;

namespace RecipeRage.Editor.UI
{
    /// <summary>
    /// Utility for setting up UI resources
    /// </summary>
    public class UISetupUtility : EditorWindow
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

        [MenuItem("RecipeRage/Setup UI Resources")]
        public static void SetupUIResources()
        {
            // Create directories if they don't exist
            CreateDirectoryIfNotExists("Assets/Resources/UI/Images");

            // Create placeholder images
            CreatePlaceholderImages();

            // Refresh asset database
            AssetDatabase.Refresh();

            Debug.Log("[UISetupUtility] UI resources setup complete");
        }

        /// <summary>
        /// Create directory if it doesn't exist
        /// </summary>
        /// <param name="path">Directory path</param>
        private static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"[UISetupUtility] Created directory: {path}");
            }
        }

        /// <summary>
        /// Create placeholder images for UI
        /// </summary>
        private static void CreatePlaceholderImages()
        {
            foreach (string imageName in ImageNames)
            {
                string path = $"Assets/Resources/UI/Images/{imageName}.png";

                // Skip if the image already exists
                if (File.Exists(path))
                {
                    continue;
                }

                // Create a placeholder texture
                Texture2D texture = CreatePlaceholderTexture(imageName);

                // Save the texture as PNG
                byte[] bytes = texture.EncodeToPNG();
                File.WriteAllBytes(path, bytes);

                Debug.Log($"[UISetupUtility] Created placeholder image: {path}");
            }
        }

        /// <summary>
        /// Create a placeholder texture with the name as text
        /// </summary>
        /// <param name="name">Image name</param>
        /// <returns>Placeholder texture</returns>
        private static Texture2D CreatePlaceholderTexture(string name)
        {
            // Create a new texture
            int width = 256;
            int height = 256;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // Fill with a color based on the name hash
            Color backgroundColor = GetColorFromName(name);
            Color textColor = GetContrastingColor(backgroundColor);

            // Fill the texture with the background color
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, backgroundColor);
                }
            }

            // Draw a border
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, 0, textColor);
                texture.SetPixel(x, height - 1, textColor);
            }

            for (int y = 0; y < height; y++)
            {
                texture.SetPixel(0, y, textColor);
                texture.SetPixel(width - 1, y, textColor);
            }

            // Draw the name in the center
            DrawTextCentered(texture, name, textColor);

            // Apply changes
            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Get a color based on the name hash
        /// </summary>
        /// <param name="name">Name to hash</param>
        /// <returns>Color based on the name</returns>
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

        /// <summary>
        /// Get a contrasting color (black or white) based on the background color
        /// </summary>
        /// <param name="backgroundColor">Background color</param>
        /// <returns>Contrasting color (black or white)</returns>
        private static Color GetContrastingColor(Color backgroundColor)
        {
            // Calculate the perceived brightness
            float brightness = (0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b);

            // Return black for bright backgrounds, white for dark backgrounds
            return brightness > 0.5f ? Color.black : Color.white;
        }

        /// <summary>
        /// Draw text centered in the texture
        /// </summary>
        /// <param name="texture">Texture to draw on</param>
        /// <param name="text">Text to draw</param>
        /// <param name="color">Text color</param>
        private static void DrawTextCentered(Texture2D texture, string text, Color color)
        {
            // This is a very simple text rendering implementation
            // In a real implementation, you would use a font rendering library

            int width = texture.width;
            int height = texture.height;

            // Calculate text position
            int textWidth = text.Length * 10; // Approximate width
            int textHeight = 20; // Approximate height

            int startX = (width - textWidth) / 2;
            int startY = (height - textHeight) / 2;

            // Draw each character
            for (int i = 0; i < text.Length; i++)
            {
                int charX = startX + i * 10;

                // Draw a simple representation of the character
                for (int y = 0; y < 10; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        if (ShouldDrawPixel(text[i], x, y))
                        {
                            texture.SetPixel(charX + x, startY + y, color);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determine if a pixel should be drawn for a character
        /// </summary>
        /// <param name="c">Character</param>
        /// <param name="x">X position within character grid</param>
        /// <param name="y">Y position within character grid</param>
        /// <returns>Whether to draw the pixel</returns>
        private static bool ShouldDrawPixel(char c, int x, int y)
        {
            // This is a very simple character rendering implementation
            // In a real implementation, you would use a font rendering library

            switch (c)
            {
                case '-':
                    return y == 5 && x >= 1 && x <= 6;

                case '_':
                    return y == 9 && x >= 1 && x <= 6;

                case '.':
                    return (y == 8 || y == 9) && (x == 3 || x == 4);

                default:
                    // For simplicity, just draw a box for other characters
                    return (x == 0 || x == 7 || y == 0 || y == 9) ||
                           (x == 3 && y >= 3 && y <= 6) ||
                           (y == 3 && x >= 3 && x <= 6);
            }
        }
    }
}

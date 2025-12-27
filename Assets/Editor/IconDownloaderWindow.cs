using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class IconDownloaderWindow : EditorWindow
{
    // --- SETTINGS ---
    private enum FontAwesomeVersion { v6, v7 }
    private FontAwesomeVersion faVersion = FontAwesomeVersion.v7;

    private bool downloadPng = true;
    private bool downloadSvg = false;
    private int pngSize = 512; // Resolution for PNGs
    private Color pngColor = Color.white; // Default color for PNGs

    // Path inside Unity Project
    private string savePath = "Assets/UI/Icons";

    // --- ICON LIST FROM YOUR HTML ---
    // Mapped strictly to FontAwesome names
    private readonly List<(string name, string style)> iconsToDownload = new List<(string name, string style)>
    {
        // Solid
        ("house", "solid"),
        ("cart-shopping", "solid"),
        ("shirt", "solid"),
        ("crosshairs", "solid"),
        ("coins", "solid"),
        ("gem", "solid"),
        ("chart-simple", "solid"),
        ("user-group", "solid"),
        ("gear", "solid"),
        ("arrow-right-from-bracket", "solid"),
        ("plus", "solid"),
        ("rotate", "solid"),

        // Regular
        ("newspaper", "regular"),
        ("copy", "regular")
    };

    [MenuItem("Tools/Icon Downloader")]
    public static void ShowWindow()
    {
        GetWindow<IconDownloaderWindow>("Icon Downloader");
    }

    private void OnGUI()
    {
        GUILayout.Label("Download Settings", EditorStyles.boldLabel);

        // 0. Version Selection
        faVersion = (FontAwesomeVersion)EditorGUILayout.EnumPopup("FA Version", faVersion);

        // 1. Format Selection
        downloadPng = EditorGUILayout.Toggle("Download PNG", downloadPng);
        downloadSvg = EditorGUILayout.Toggle("Download SVG", downloadSvg);

        // 2. PNG Specific Settings
        if (downloadPng)
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("PNG Settings", EditorStyles.miniBoldLabel);
            pngSize = EditorGUILayout.IntSlider("Size (px)", pngSize, 64, 1024);
            pngColor = EditorGUILayout.ColorField("Icon Color", pngColor);
            EditorGUILayout.HelpBox("White is recommended so you can tint it in the Inspector.", MessageType.Info);
        }

        GUILayout.Space(10);

        // 3. Path
        GUILayout.Label("Save Path:", EditorStyles.label);
        savePath = EditorGUILayout.TextField(savePath);

        GUILayout.Space(20);

        // 4. Download Button
        GUI.enabled = downloadPng || downloadSvg;
        if (GUILayout.Button("Download Icons", GUILayout.Height(40)))
        {
            DownloadAllIcons();
        }
        GUI.enabled = true;
    }

    private async void DownloadAllIcons()
    {
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        string hexColor = ColorUtility.ToHtmlStringRGB(pngColor).ToLower();
        string versionStr = faVersion == FontAwesomeVersion.v7 ? "7" : "6";

        int totalExpected = 0;
        if (downloadPng) totalExpected += iconsToDownload.Count;
        if (downloadSvg) totalExpected += iconsToDownload.Count;

        int count = 0;

        foreach (var (name, style) in iconsToDownload)
        {
            // Collection mapping for Iconify: e.g. fa6-solid, fa7-solid
            string prefix = $"fa{versionStr}";
            string collection = $"{prefix}-{style}";

            if (downloadSvg)
            {
                float progress = (float)count / totalExpected;
                EditorUtility.DisplayProgressBar("Downloading Icons", $"Fetching {name} (SVG)...", progress);

                // Direct from GitHub for raw SVGs (or Iconify if preferred, but GitHub is direct)
                string branch = faVersion == FontAwesomeVersion.v7 ? "7.x" : "6.x";
                string url = $"https://raw.githubusercontent.com/FortAwesome/Font-Awesome/{branch}/svgs/{style}/{name}.svg";

                string fullPath = Path.Combine(savePath, $"{name}.svg");
                await DownloadFile(url, fullPath);
                count++;
            }

            if (downloadPng)
            {
                float progress = (float)count / totalExpected;
                EditorUtility.DisplayProgressBar("Downloading Icons", $"Fetching {name} (PNG)...", progress);

                // Strategy: Use Iconify to generate a colored SVG, then use weserv.nl to convert it to a PNG of the correct size.
                // This bypasses weserv.nl's "tint" limitations which preserves luminance (bad for black icons).
                string iconifyUrl = $"https://api.iconify.design/{collection}/{name}.svg?color=%23{hexColor}";
                string encodedIconifyUrl = UnityWebRequest.EscapeURL(iconifyUrl);
                string weservUrl = $"https://images.weserv.nl/?url={encodedIconifyUrl}&w={pngSize}&h={pngSize}&output=png";

                string fullPath = Path.Combine(savePath, $"{name}.png");
                await DownloadFile(weservUrl, fullPath);
                count++;
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();

        // Automatically set PNGs to Sprite type
        if (downloadPng)
        {
            foreach (var (name, _) in iconsToDownload)
            {
                string relativePath = Path.Combine(savePath, $"{name}.png");
                TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }
            }
        }

        Debug.Log($"<color=green>Done! Downloaded {count} assets to {savePath}</color>");
    }

    private async Task DownloadFile(string url, string filePath)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(filePath, request.downloadHandler.data);
            }
            else
            {
                Debug.LogError($"Failed to download {url}: {request.error}");
            }
        }
    }
}

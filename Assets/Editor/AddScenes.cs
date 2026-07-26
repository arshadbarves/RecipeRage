using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
public static class AddScenes
{
    public static void Run()
    {
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Maps/MapBeachBBQ.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Maps/MapForestCampfire.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Maps/MapPirateShip.unity", true),
        };
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[AddScenes] Build Settings: " + scenes.Count + " scenes");
    }
}

using UnityEngine;
using UnityEditor;
public static class FinalCheck
{
    public static void Run()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var recipes = AssetDatabase.FindAssets("t:RecipeDefinition").Length;
        var chefs = AssetDatabase.FindAssets("t:ChefDefinition").Length;
        var maps = AssetDatabase.FindAssets("t:MapDefinition").Length;
        var prefabs = AssetDatabase.FindAssets("t:Prefab", new[]{"Assets/Game/Network/Prefabs"}).Length;
        var boot = System.IO.File.Exists("Assets/Scenes/Boot.unity");
        Debug.Log($"[FinalCheck] recipes={recipes} chefs={chefs} maps={maps} netPrefabs={prefabs} bootScene={boot}");
    }
}

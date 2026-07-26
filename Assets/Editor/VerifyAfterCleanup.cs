using UnityEngine;
using UnityEditor;
public static class VerifyAfterCleanup
{
    public static void Run()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var recipes = AssetDatabase.FindAssets("t:RecipeDefinition").Length;
        var chefs = AssetDatabase.FindAssets("t:ChefDefinition").Length;
        var maps = AssetDatabase.FindAssets("t:MapDefinition").Length;
        var uxml = AssetDatabase.FindAssets("t:VisualTreeAsset", new[]{"Assets/Game/UI/UXML"}).Length;
        var prefabs = AssetDatabase.FindAssets("t:Prefab", new[]{"Assets/Game/Network/Prefabs"}).Length;
        Debug.Log($"[Verify] recipes={recipes} chefs={chefs} maps={maps} uxml={uxml} netPrefabs={prefabs}");
    }
}

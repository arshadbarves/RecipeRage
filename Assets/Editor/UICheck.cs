using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;
public static class UICheck
{
    public static void Run()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var uxml = AssetDatabase.FindAssets("t:VisualTreeAsset", new[]{"Assets/Game/UI/UXML"});
        var uss = AssetDatabase.FindAssets("t:StyleSheet", new[]{"Assets/Game/UI/Styles"});
        Debug.Log($"[UICheck] UXML={uxml.Length} USS={uss.Length}");
        int ok = 0, fail = 0;
        foreach (var guid in uxml)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            if (tree != null) { ok++; } else { fail++; Debug.LogError("[UICheck] FAILED: " + path); }
        }
        Debug.Log($"[UICheck] Loaded OK={ok} Failed={fail}");
    }
}

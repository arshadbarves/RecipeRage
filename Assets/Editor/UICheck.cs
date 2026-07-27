using UnityEngine; using UnityEditor;
public static class UICheck {
    public static void Run() {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var uss = AssetDatabase.FindAssets("t:StyleSheet", new[]{"Assets/Game/UI/Styles"});
        var uxml = AssetDatabase.FindAssets("t:VisualTreeAsset", new[]{"Assets/Game/UI/UXML"});
        Debug.Log($"[UICheck] USS={uss.Length} UXML={uxml.Length}");
    }
}

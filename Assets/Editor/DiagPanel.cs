using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
public static class DiagPanel
{
    public static void Run()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>("Assets/Game/UI/PanelSettings.asset");
        Debug.Log("[Diag] LoadAssetAtPath<PanelSettings> = " + (ps == null ? "NULL" : ps.name));
        var obj = AssetDatabase.LoadAssetAtPath<Object>("Assets/Game/UI/PanelSettings.asset");
        Debug.Log("[Diag] LoadAssetAtPath<Object> = " + (obj == null ? "NULL" : obj.GetType().FullName));
        var all = AssetDatabase.LoadAllAssetsAtPath("Assets/Game/UI/PanelSettings.asset");
        Debug.Log("[Diag] LoadAllAssetsAtPath count = " + all.Length);
        foreach (var a in all) Debug.Log("[Diag]   - " + a.GetType().FullName);
    }
}

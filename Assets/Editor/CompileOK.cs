using UnityEngine; using UnityEditor;
public static class CompileOK { public static void Run() { AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport); Debug.Log("[CompileOK] done"); } }

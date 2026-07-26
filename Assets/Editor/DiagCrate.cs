using UnityEngine; using UnityEditor; using UnityEditor.SceneManagement; using RecipeRage;
public static class DiagCrate {
    public static void Run() {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Maps/MapBeachBBQ.unity", OpenSceneMode.Single);
        var crates = Object.FindObjectsByType<IngredientCrate>(FindObjectsSortMode.None);
        Debug.Log("[Diag] crates found: " + crates.Length);
        foreach (var c in crates) {
            var so = new SerializedObject(c);
            var p = so.FindProperty("_ingredient");
            Debug.Log($"[Diag] crate={c.name} propNull={(p==null)} curVal={(c.GetComponent<IngredientCrate>() == null)}");
            // try direct reflection set
            var f = typeof(IngredientCrate).GetField("_ingredient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Debug.Log($"[Diag]   reflection field null={(f==null)}");
        }
    }
}

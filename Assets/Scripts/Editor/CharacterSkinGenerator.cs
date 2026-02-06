using System;
using System.IO;
using Gameplay.Characters;
using Gameplay.Skins.Data;
using UnityEditor;
using UnityEngine;

namespace RecipeRage.Editor
{
    /// <summary>
    /// Generates simple visual-only skin prefabs and assigns them to CharacterClass.Skins.
    /// Intended as a bootstrap tool until real character models/skins are available.
    /// </summary>
    public static class CharacterSkinGenerator
    {
        private const string CharactersFolder = "Assets/Resources/ScriptableObjects/CharacterClasses";
        private const string SkinsPrefabsFolder = "Assets/Prefabs/Gameplay/Skins";

        [MenuItem("RecipeRage/Setup/Generate Default Character Skins")]
        public static void GenerateDefaultCharacterSkins()
        {
            if (!EditorUtility.DisplayDialog(
                    "Generate Default Character Skins",
                    "This will overwrite the Skins list on all CharacterClass assets found under:\n" +
                    $"{CharactersFolder}\n\n" +
                    "It will also create simple visual prefabs + materials under:\n" +
                    $"{SkinsPrefabsFolder}\n\n" +
                    "Continue?",
                    "Generate",
                    "Cancel"))
            {
                return;
            }

            EnsureDirectory(CharactersFolder);
            EnsureDirectory(SkinsPrefabsFolder);

            string[] characterGuids = AssetDatabase.FindAssets("t:CharacterClass", new[] { CharactersFolder });
            if (characterGuids == null || characterGuids.Length == 0)
            {
                Debug.LogWarning($"[CharacterSkinGenerator] No CharacterClass assets found under {CharactersFolder}");
                return;
            }

            var definitions = new[]
            {
                new SkinDefinition("default", "Default", SkinRarity.Common, true, new Color(0.8f, 0.8f, 0.8f)),
                new SkinDefinition("rare", "Rare", SkinRarity.Rare, false, new Color(0.3f, 0.6f, 1.0f)),
                new SkinDefinition("epic", "Epic", SkinRarity.Epic, false, new Color(0.8f, 0.3f, 1.0f))
            };

            int totalSkinsGenerated = 0;

            foreach (string guid in characterGuids)
            {
                string characterAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                var character = AssetDatabase.LoadAssetAtPath<CharacterClass>(characterAssetPath);
                if (character == null)
                {
                    continue;
                }

                string characterFolderName = $"Character_{character.Id}_{SanitizeForPath(character.DisplayName)}";
                string characterSkinsFolder = $"{SkinsPrefabsFolder}/{characterFolderName}";
                EnsureDirectory(characterSkinsFolder);

                SerializedObject characterObj = new SerializedObject(character);
                SerializedProperty skinsProp = characterObj.FindProperty("_skins");
                skinsProp.arraySize = 0;

                for (int i = 0; i < definitions.Length; i++)
                {
                    SkinDefinition def = definitions[i];

                    string skinId = $"character_{character.Id}_{def.idSuffix}";
                    string skinDisplayName = $"{character.DisplayName} {def.displayName}";

                    string materialPath = $"{characterSkinsFolder}/{skinId}.mat";
                    Material material = GetOrCreateMaterial(materialPath, def.color);

                    string prefabPath = $"{characterSkinsFolder}/{skinId}.prefab";
                    GameObject prefab = CreateOrUpdatePrimitiveSkinPrefab(prefabPath, skinDisplayName, material);

                    skinsProp.arraySize++;
                    SerializedProperty element = skinsProp.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("id").stringValue = skinId;
                    element.FindPropertyRelative("name").stringValue = skinDisplayName;
                    element.FindPropertyRelative("prefab").objectReferenceValue = prefab;
                    element.FindPropertyRelative("rarity").enumValueIndex = (int)def.rarity;
                    element.FindPropertyRelative("isDefault").boolValue = def.isDefault;
                    element.FindPropertyRelative("priceOverride").intValue = -1;

                    totalSkinsGenerated++;
                }

                characterObj.ApplyModifiedProperties();
                EditorUtility.SetDirty(character);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[CharacterSkinGenerator] Generated/updated {totalSkinsGenerated} skins across {characterGuids.Length} characters.");
        }

        private static Material GetOrCreateMaterial(string assetPath, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ??
                                Shader.Find("Standard") ??
                                Shader.Find("Sprites/Default");

                material = new Material(shader)
                {
                    color = color,
                    name = Path.GetFileNameWithoutExtension(assetPath)
                };

                AssetDatabase.CreateAsset(material, assetPath);
            }
            else
            {
                material.color = color;
                EditorUtility.SetDirty(material);
            }

            return material;
        }

        private static GameObject CreateOrUpdatePrimitiveSkinPrefab(string assetPath, string displayName, Material material)
        {
            var tempObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            tempObject.name = displayName;

            Collider collider = tempObject.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            var renderer = tempObject.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tempObject, assetPath);
            UnityEngine.Object.DestroyImmediate(tempObject);

            return prefab;
        }

        private static void EnsureDirectory(string assetPath)
        {
            string fullPath = assetPath.StartsWith("Assets", StringComparison.Ordinal)
                ? Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length))
                : assetPath;

            if (Directory.Exists(fullPath))
            {
                return;
            }

            Directory.CreateDirectory(fullPath);
            AssetDatabase.Refresh();
        }

        private static string SanitizeForPath(string input)
        {
            if (string.IsNullOrEmpty(input)) return "Unnamed";

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(invalidChar, '_');
            }

            return input.Replace(' ', '_');
        }

        private readonly struct SkinDefinition
        {
            public readonly string idSuffix;
            public readonly string displayName;
            public readonly SkinRarity rarity;
            public readonly bool isDefault;
            public readonly Color color;

            public SkinDefinition(string idSuffix, string displayName, SkinRarity rarity, bool isDefault, Color color)
            {
                this.idSuffix = idSuffix;
                this.displayName = displayName;
                this.rarity = rarity;
                this.isDefault = isDefault;
                this.color = color;
            }
        }
    }
}


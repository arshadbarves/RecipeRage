using UnityEngine;
using UnityEditor;
using System.IO;
using RecipeRage.Gameplay.Interactables;
using Unity.Netcode;

namespace RecipeRage.Editor
{
    public class StationPrefabGenerator : EditorWindow
    {
        private const string PREFAB_PATH = "Assets/Prefabs/Stations";
        private const string MATERIAL_PATH = "Assets/Materials/Stations";
        private const string SHADER_PATH = "Assets/Shaders/Stations";

        private Material[] stationMaterials;
        private GameObject baseStationPrefab;

        [MenuItem("RecipeRage/Generate Station Prefabs")]
        public static void ShowWindow()
        {
            GetWindow<StationPrefabGenerator>("Station Generator");
        }

        private void OnEnable()
        {
            LoadStationMaterials();
            LoadOrCreateBaseStationPrefab();
        }

        private void LoadOrCreateBaseStationPrefab()
        {
            string basePrefabPath = $"{PREFAB_PATH}/BaseStation.prefab";
            baseStationPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);
            
            if (baseStationPrefab == null)
            {
                GenerateBaseStationPrefab();
            }
        }

        private void GenerateBaseStationPrefab()
        {
            // Create base station prefab
            var baseStation = new GameObject("BaseStation");
            baseStation.AddComponent<NetworkObject>();
            baseStation.AddComponent<BaseStation>();

            // Create model container
            var modelContainer = CreateChild(baseStation, "Model");
            var placeholder = CreatePlaceholderMesh(modelContainer, PrimitiveType.Cube);
            placeholder.transform.localScale = new Vector3(1f, 0.5f, 1f);
            placeholder.transform.localPosition = new Vector3(0f, 0.25f, 0f);

            // Create interaction point
            var interactionPoint = CreateChild(baseStation, "InteractionPoint");
            interactionPoint.transform.localPosition = new Vector3(0f, 1f, 0f);

            // Create highlight effect
            var highlight = CreateChild(baseStation, "Highlight");
            var highlightMesh = CreatePlaceholderMesh(highlight, PrimitiveType.Cube);
            highlightMesh.transform.localScale = new Vector3(1.1f, 0.6f, 1.1f);
            highlightMesh.GetComponent<MeshRenderer>().material = CreateHighlightMaterial();

            // Create particle system
            var particleObj = CreateChild(baseStation, "Particles");
            SetupParticleSystem(particleObj);

            // Add audio source
            var audioSource = baseStation.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;

            // Save base station prefab
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
            }
            string basePrefabPath = $"{PREFAB_PATH}/BaseStation.prefab";
            baseStationPrefab = PrefabUtility.SaveAsPrefabAsset(baseStation, basePrefabPath);
            DestroyImmediate(baseStation);
        }

        private void OnGUI()
        {
            GUILayout.Label("Station Prefab Generator", EditorStyles.boldLabel);

            if (GUILayout.Button("Generate All Station Prefabs"))
            {
                GenerateAllStations();
            }
        }

        private void GenerateAllStations()
        {
            GenerateChoppingStation();
            GeneratePlateDispenser();
            GenerateTrashStation();
            GenerateServingStation();
            GenerateContainerStation();
            GenerateItemContainerStation();
        }

        private void GenerateChoppingStation()
        {
            // Create variant from base station
            var prefabPath = $"{PREFAB_PATH}/ChoppingStation.prefab";
            var variant = PrefabUtility.InstantiatePrefab(baseStationPrefab) as GameObject;
            
            // Add chopping station component
            var choppingStation = variant.AddComponent<ChoppingStation>();
            DestroyImmediate(variant.GetComponent<BaseStation>());

            // Customize model
            var modelContainer = variant.transform.Find("Model");
            var renderer = modelContainer.GetComponentInChildren<MeshRenderer>();
            renderer.material = stationMaterials[2]; // Blue

            // Add progress bar
            var progressBar = CreateProgressBar(variant);
            progressBar.transform.localPosition = new Vector3(0f, 1.2f, 0f);

            // Add knife placeholder
            var knife = CreateKnifePlaceholder(variant);
            knife.transform.localPosition = new Vector3(0f, 0.6f, 0f);

            // Save as variant
            PrefabUtility.SaveAsPrefabAsset(variant, prefabPath);
            DestroyImmediate(variant);
        }

        private void GeneratePlateDispenser()
        {
            // Create variant from base station
            var prefabPath = $"{PREFAB_PATH}/PlateDispenser.prefab";
            var variant = PrefabUtility.InstantiatePrefab(baseStationPrefab) as GameObject;
            
            // Add plate dispenser component
            var plateDispenser = variant.AddComponent<PlateDispenser>();
            DestroyImmediate(variant.GetComponent<BaseStation>());

            // Customize model
            var modelContainer = variant.transform.Find("Model");
            var renderer = modelContainer.GetComponentInChildren<MeshRenderer>();
            renderer.material = stationMaterials[4]; // Pink

            // Add spawn point
            var spawnPoint = CreateChild(variant, "SpawnPoint");
            spawnPoint.transform.localPosition = new Vector3(0f, 1.2f, 0.5f);

            // Save as variant
            PrefabUtility.SaveAsPrefabAsset(variant, prefabPath);
            DestroyImmediate(variant);
        }

        private void GenerateTrashStation()
        {
            // Create variant from base station
            var prefabPath = $"{PREFAB_PATH}/TrashStation.prefab";
            var variant = PrefabUtility.InstantiatePrefab(baseStationPrefab) as GameObject;
            
            // Add trash station component
            var trashStation = variant.AddComponent<TrashStation>();
            DestroyImmediate(variant.GetComponent<BaseStation>());

            // Customize model
            var modelContainer = variant.transform.Find("Model");
            var renderer = modelContainer.GetComponentInChildren<MeshRenderer>();
            renderer.material = stationMaterials[0]; // Red

            // Add trash particle system
            var particleSystem = CreateTrashParticleSystem(variant);
            particleSystem.transform.localPosition = new Vector3(0f, 1f, 0f);

            // Save as variant
            PrefabUtility.SaveAsPrefabAsset(variant, prefabPath);
            DestroyImmediate(variant);
        }

        private void GenerateServingStation()
        {
            // Create variant from base station
            var prefabPath = $"{PREFAB_PATH}/ServingStation.prefab";
            var variant = PrefabUtility.InstantiatePrefab(baseStationPrefab) as GameObject;
            
            // Add serving station component
            var servingStation = variant.AddComponent<ServingStation>();
            DestroyImmediate(variant.GetComponent<BaseStation>());

            // Customize model
            var modelContainer = variant.transform.Find("Model");
            var renderer = modelContainer.GetComponentInChildren<MeshRenderer>();
            renderer.material = stationMaterials[1]; // Green

            // Add serving window
            var window = CreateServingWindow(variant);
            window.transform.localPosition = new Vector3(0f, 1f, 0.5f);

            // Save as variant
            PrefabUtility.SaveAsPrefabAsset(variant, prefabPath);
            DestroyImmediate(variant);
        }

        private void GenerateContainerStation()
        {
            // Create variant from base station
            var prefabPath = $"{PREFAB_PATH}/ContainerStation.prefab";
            var variant = PrefabUtility.InstantiatePrefab(baseStationPrefab) as GameObject;
            
            // Add container station component
            var containerStation = variant.AddComponent<ContainerStation>();
            DestroyImmediate(variant.GetComponent<BaseStation>());

            // Customize model
            var modelContainer = variant.transform.Find("Model");
            var renderer = modelContainer.GetComponentInChildren<MeshRenderer>();
            renderer.material = stationMaterials[3]; // Yellow

            // Add spawn point
            var spawnPoint = CreateChild(variant, "SpawnPoint");
            spawnPoint.transform.localPosition = new Vector3(0f, 1.2f, 0.5f);

            // Save as variant
            PrefabUtility.SaveAsPrefabAsset(variant, prefabPath);
            DestroyImmediate(variant);
        }

        private void GenerateItemContainerStation()
        {
            // Create variant from base station
            var prefabPath = $"{PREFAB_PATH}/ItemContainerStation.prefab";
            var variant = PrefabUtility.InstantiatePrefab(baseStationPrefab) as GameObject;
            
            // Add item container station component
            var itemContainer = variant.AddComponent<ItemContainerStation>();
            DestroyImmediate(variant.GetComponent<BaseStation>());

            // Customize model
            var modelContainer = variant.transform.Find("Model");
            var renderer = modelContainer.GetComponentInChildren<MeshRenderer>();
            renderer.material = stationMaterials[5]; // Orange

            // Add storage point
            var storagePoint = CreateChild(variant, "StoragePoint");
            storagePoint.transform.localPosition = new Vector3(0f, 1f, 0f);

            // Save as variant
            PrefabUtility.SaveAsPrefabAsset(variant, prefabPath);
            DestroyImmediate(variant);
        }

        private GameObject CreateChild(GameObject parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent.transform);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child;
        }

        private GameObject CreatePlaceholderMesh(GameObject parent, PrimitiveType type)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.transform.SetParent(parent.transform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            return obj;
        }

        private void SetupCommonComponents(GameObject station)
        {
            // Create interaction point if not exists
            if (!station.transform.Find("InteractionPoint"))
            {
                var interactionPoint = CreateChild(station, "InteractionPoint");
                interactionPoint.transform.localPosition = new Vector3(0f, 1f, 0f);
            }

            // Create highlight effect if not exists
            if (!station.transform.Find("Highlight"))
            {
                var highlight = CreateChild(station, "Highlight");
                var highlightMesh = CreatePlaceholderMesh(highlight, PrimitiveType.Cube);
                highlightMesh.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                highlightMesh.GetComponent<MeshRenderer>().material = CreateHighlightMaterial();
                highlight.SetActive(false);
            }

            // Create particle system if not exists
            if (!station.transform.Find("Particles"))
            {
                var particleObj = CreateChild(station, "Particles");
                SetupParticleSystem(particleObj);
            }

            // Add audio source if not exists
            if (!station.GetComponent<AudioSource>())
            {
                var audioSource = station.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1f;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.minDistance = 1f;
                audioSource.maxDistance = 10f;
            }
        }

        private void SetupParticleSystem(GameObject obj)
        {
            var ps = obj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 1f;
            main.loop = false;
            main.startLifetime = 1f;
            main.startSpeed = 2f;
            main.startSize = 0.1f;
            main.maxParticles = 50;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;

            var renderer = obj.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateParticleMaterial();
        }

        private GameObject CreateProgressBar(GameObject parent)
        {
            var progressBar = CreateChild(parent, "ProgressBar");
            var background = CreatePlaceholderMesh(progressBar, PrimitiveType.Cube);
            background.transform.localScale = new Vector3(1f, 0.1f, 0.1f);
            background.GetComponent<MeshRenderer>().material = CreateProgressBarMaterial();

            var fill = CreatePlaceholderMesh(progressBar, PrimitiveType.Cube);
            fill.name = "Fill";
            fill.transform.localScale = new Vector3(0.98f, 0.08f, 0.08f);
            fill.transform.localPosition = new Vector3(-0.49f, 0f, 0f);
            fill.GetComponent<MeshRenderer>().material = CreateProgressBarFillMaterial();

            return progressBar;
        }

        private GameObject CreateKnifePlaceholder(GameObject parent)
        {
            var knife = CreateChild(parent, "Knife");
            var blade = CreatePlaceholderMesh(knife, PrimitiveType.Cube);
            blade.transform.localScale = new Vector3(0.4f, 0.05f, 0.1f);
            blade.GetComponent<MeshRenderer>().material = CreateKnifeMaterial();

            var handle = CreatePlaceholderMesh(knife, PrimitiveType.Cube);
            handle.transform.localScale = new Vector3(0.2f, 0.05f, 0.05f);
            handle.transform.localPosition = new Vector3(0.3f, 0f, 0f);
            handle.GetComponent<MeshRenderer>().material = CreateKnifeHandleMaterial();

            return knife;
        }

        private GameObject CreateServingWindow(GameObject parent)
        {
            var window = CreateChild(parent, "Window");
            var frame = CreatePlaceholderMesh(window, PrimitiveType.Cube);
            frame.transform.localScale = new Vector3(0.8f, 0.8f, 0.1f);
            frame.GetComponent<MeshRenderer>().material = CreateWindowMaterial();

            return window;
        }

        private ParticleSystem CreateTrashParticleSystem(GameObject parent)
        {
            var trashEffect = CreateChild(parent, "TrashEffect");
            var ps = trashEffect.AddComponent<ParticleSystem>();
            
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.5f;
            main.startSpeed = 3f;
            main.startSize = 0.2f;
            main.maxParticles = 30;
            main.playOnAwake = false;
            main.gravityModifier = 1f;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 30f;
            shape.radius = 0.1f;

            var renderer = trashEffect.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.minParticleSize = 0.1f;
            renderer.maxParticleSize = 0.3f;
            renderer.sortMode = ParticleSystemSortMode.Distance;

            return ps;
        }

        private Material CreateHighlightMaterial()
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", new Color(1f, 1f, 1f, 0.3f));
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetFloat("_Surface", 1);
            material.SetFloat("_Blend", 0);
            material.SetFloat("_ZWrite", 0);
            material.renderQueue = 3000;
            return material;
        }

        private Material CreateParticleMaterial()
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            material.SetColor("_BaseColor", Color.white);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            return material;
        }

        private Material CreateProgressBarMaterial()
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", Color.gray);
            return material;
        }

        private Material CreateProgressBarFillMaterial()
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", Color.green);
            return material;
        }

        private Material CreateKnifeMaterial()
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", Color.gray);
            material.SetFloat("_Metallic", 1f);
            material.SetFloat("_Smoothness", 0.8f);
            return material;
        }

        private Material CreateKnifeHandleMaterial()
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", Color.black);
            return material;
        }

        private Material CreateWindowMaterial()
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", new Color(0.8f, 0.8f, 1f, 0.5f));
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetFloat("_Surface", 1);
            material.SetFloat("_Blend", 0);
            material.renderQueue = 3000;
            return material;
        }

        private void SavePrefab(GameObject obj, string name)
        {
            if (!Directory.Exists(PREFAB_PATH))
            {
                Directory.CreateDirectory(PREFAB_PATH);
            }

            string path = $"{PREFAB_PATH}/{name}.prefab";
            bool success = false;
            PrefabUtility.SaveAsPrefabAsset(obj, path, out success);
            if (success)
                Debug.Log($"Saved prefab: {path}");
            else
                Debug.LogError($"Failed to save prefab: {path}");
        }

        private void LoadStationMaterials()
        {
            stationMaterials = new Material[6];
            stationMaterials[0] = AssetDatabase.LoadAssetAtPath<Material>("Assets/Shaders/Custom_MobileAnimeURP 1.mat"); // Red - Trash
            stationMaterials[1] = AssetDatabase.LoadAssetAtPath<Material>("Assets/Shaders/Custom_MobileAnimeURP 2.mat"); // Green - Serving
            stationMaterials[2] = AssetDatabase.LoadAssetAtPath<Material>("Assets/Shaders/Custom_MobileAnimeURP 3.mat"); // Blue - Chopping
            stationMaterials[3] = AssetDatabase.LoadAssetAtPath<Material>("Assets/Shaders/Custom_MobileAnimeURP 4.mat"); // Yellow - Container
            stationMaterials[4] = AssetDatabase.LoadAssetAtPath<Material>("Assets/Shaders/Custom_MobileAnimeURP 5.mat"); // Pink - Plate
            stationMaterials[5] = AssetDatabase.LoadAssetAtPath<Material>("Assets/Shaders/Custom_MobileAnimeURP 6.mat"); // Orange - ItemContainer
        }
    }
} 
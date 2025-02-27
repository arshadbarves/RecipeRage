using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using Unity.Netcode;
using RecipeRage.Core.Player;
using RecipeRage.Core.Input;
using System.IO;

namespace RecipeRage.Editor
{
    public class PlayerPrefabGenerator : EditorWindow
    {
        private const string PREFAB_PATH = "Assets/Prefabs/Player";
        private Material playerMaterial;
        private Material highlightMaterial;

        [MenuItem("RecipeRage/Generate Player Prefab")]
        public static void ShowWindow()
        {
            GetWindow<PlayerPrefabGenerator>("Player Generator");
        }

        private void OnEnable()
        {
            LoadMaterials();
        }

        private void LoadMaterials()
        {
            // Load the anime shader materials for the player
            playerMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Shaders/Custom_MobileAnimeURP 1.mat");
            highlightMaterial = CreateHighlightMaterial();
        }

        private void OnGUI()
        {
            GUILayout.Label("Player Prefab Generator", EditorStyles.boldLabel);

            if (GUILayout.Button("Generate Player Prefab"))
            {
                GeneratePlayerPrefab();
            }
        }

        private void GeneratePlayerPrefab()
        {
            // Create the player root object
            var player = new GameObject("PlayerPrefab");

            // Add required components
            var networkObject = player.AddComponent<NetworkObject>();
            var playerController = player.AddComponent<PlayerController>();
            var inputManager = player.AddComponent<InputManager>();
            var playerInput = player.AddComponent<PlayerInput>();
            var rigidbody = player.AddComponent<Rigidbody>();

            // Setup rigidbody
            rigidbody.useGravity = true;
            rigidbody.freezeRotation = true;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Add collider
            var capsuleCollider = player.AddComponent<CapsuleCollider>();
            capsuleCollider.height = 2f;
            capsuleCollider.radius = 0.4f;
            capsuleCollider.center = new Vector3(0f, 1f, 0f);

            // Create visual hierarchy
            var modelContainer = CreateChild(player, "Model");
            var body = CreatePlayerModel(modelContainer);
            var holdPoint = CreateChild(player, "HoldPoint");
            holdPoint.transform.localPosition = new Vector3(0.5f, 1.5f, 0.5f);

            // Create interaction point
            var interactionPoint = CreateChild(player, "InteractionPoint");
            interactionPoint.transform.localPosition = new Vector3(0f, 1.5f, 0.8f);

            // Create camera mount
            var cameraMount = CreateChild(player, "CameraMount");
            cameraMount.transform.localPosition = new Vector3(0f, 2.5f, -4f);
            cameraMount.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);

            // Setup player controller references
            playerController.enabled = true;
            var serializedObject = new SerializedObject(playerController);
            var holdPointProp = serializedObject.FindProperty("_holdPoint");
            var interactionPointProp = serializedObject.FindProperty("_interactionPoint");
            var interactionLayerProp = serializedObject.FindProperty("_interactionLayer");
            
            holdPointProp.objectReferenceValue = holdPoint.transform;
            interactionPointProp.objectReferenceValue = interactionPoint.transform;
            interactionLayerProp.intValue = LayerMask.GetMask("Interactable");
            
            serializedObject.ApplyModifiedProperties();

            // Setup input
            SetupPlayerInput(playerInput);

            // Save the prefab
            SavePrefab(player, "PlayerPrefab");
            DestroyImmediate(player);
        }

        private GameObject CreatePlayerModel(GameObject parent)
        {
            // Create basic player model
            var body = CreateChild(parent, "Body");

            // Create capsule for body
            var bodyMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bodyMesh.transform.SetParent(body.transform);
            bodyMesh.transform.localPosition = new Vector3(0f, 1f, 0f);
            bodyMesh.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            bodyMesh.GetComponent<Renderer>().material = playerMaterial;
            DestroyImmediate(bodyMesh.GetComponent<CapsuleCollider>()); // Remove primitive collider

            // Create head
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(body.transform);
            head.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            head.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            head.GetComponent<Renderer>().material = playerMaterial;
            DestroyImmediate(head.GetComponent<SphereCollider>()); // Remove primitive collider

            // Create chef hat
            var hat = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hat.name = "ChefHat";
            hat.transform.SetParent(head.transform);
            hat.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            hat.transform.localScale = new Vector3(0.7f, 0.4f, 0.7f);
            hat.GetComponent<Renderer>().material = playerMaterial;
            DestroyImmediate(hat.GetComponent<CapsuleCollider>()); // Remove primitive collider

            return body;
        }

        private void SetupPlayerInput(PlayerInput playerInput)
        {
            // Create input actions if they don't exist
            string inputActionsPath = "Assets/Settings/PlayerInputActions.inputactions";
            if (!File.Exists(inputActionsPath))
            {
                CreateDefaultInputActions(inputActionsPath);
            }

            // Assign input actions asset
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputActionsPath);
            playerInput.actions = inputActions;
            playerInput.defaultActionMap = "Player";
            playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        }

        private void CreateDefaultInputActions(string path)
        {
            var inputActions = ScriptableObject.CreateInstance<InputActionAsset>();
            var playerMap = new InputActionMap("Player");

            // Movement
            var moveAction = playerMap.AddAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            // Interaction
            var interactAction = playerMap.AddAction("Interact", InputActionType.Button);
            interactAction.AddBinding("<Keyboard>/e");

            // Pause
            var pauseAction = playerMap.AddAction("Pause", InputActionType.Button);
            pauseAction.AddBinding("<Keyboard>/escape");

            inputActions.AddActionMap(playerMap);
            AssetDatabase.CreateAsset(inputActions, path);
            AssetDatabase.SaveAssets();
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
    }
} 
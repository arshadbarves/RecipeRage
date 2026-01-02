using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class VoxelEnvTools : EditorWindow
{
    [MenuItem("Tools/Voxel Env/Setup Materials and Scene")]
    public static void Setup()
    {
        EnsureDirectories();
        CreateMaterials();
        CreateDemoObjects();
    }

    private static void EnsureDirectories()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Art/Materials"))
            AssetDatabase.CreateFolder("Assets/Art", "Materials");
        if (!AssetDatabase.IsValidFolder("Assets/Colors"))
            AssetDatabase.CreateFolder("Assets", "Colors"); // Just in case
    }

    private static void CreateMaterials()
    {
        // Floor Material
        Shader floorShader = Shader.Find("Custom/VoxelFloor");
        if (floorShader == null)
        {
            Debug.LogError("Custom/VoxelFloor shader not found. Did it compile?");
            return;
        }

        Material floorMat = new Material(floorShader);
        floorMat.name = "Mat_VoxelFloor";
        // Colors from reference
        floorMat.SetColor("_BaseColor", new Color(0.85f, 0.56f, 0.45f)); // Sand
        floorMat.SetColor("_GridColor", new Color(0.71f, 0.38f, 0.27f)); // Darker lines
        floorMat.SetFloat("_GridScale", 5.0f);
        floorMat.SetFloat("_GridThickness", 0.05f);
        
        AssetDatabase.CreateAsset(floorMat, "Assets/Art/Materials/Mat_VoxelFloor.mat");

        // Water Material
        Shader waterShader = Shader.Find("Custom/VoxelWater");
        if (waterShader == null)
        {
            Debug.LogError("Custom/VoxelWater shader not found. Did it compile?");
            return;
        }

        Material waterMat = new Material(waterShader);
        waterMat.name = "Mat_VoxelWater";
        waterMat.SetColor("_DeepColor", new Color(0.18f, 0.65f, 0.82f, 0.9f));
        waterMat.SetColor("_ShallowColor", new Color(0.28f, 0.84f, 0.93f, 0.6f));
        waterMat.SetColor("_FoamColor", Color.white);
        waterMat.SetFloat("_WaveScale", 8.0f);
        waterMat.SetFloat("_WaveSpeed", 1.5f);
        waterMat.SetFloat("_FoamThreshold", 0.8f);

        AssetDatabase.CreateAsset(waterMat, "Assets/Art/Materials/Mat_VoxelWater.mat");
        
        AssetDatabase.SaveAssets();
        Debug.Log("Created Voxel Materials in Assets/Art/Materials");
    }

    private static void CreateDemoObjects()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "VoxelFloor";
        floor.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Mat_VoxelFloor.mat");
        
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.name = "VoxelWater";
        water.transform.position = new Vector3(0, -0.1f, 0);
        water.transform.localScale = new Vector3(3, 1, 3);
        water.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/Mat_VoxelWater.mat");

        Selection.activeGameObject = floor;
        SceneView.lastActiveSceneView.FrameSelected();
        Debug.Log("Created Demo Objects in Scene");
    }
}

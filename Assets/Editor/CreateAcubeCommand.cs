using UnityEngine;
using UnityEditor;

// Simple editor-run script that creates a cube named "acube" at the origin.
// The coplay MCP executor will call the public static Execute() method.
public static class CreateAcubeCommand
{
    public static void Execute()
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "acube";
        cube.transform.position = Vector3.zero;
        Debug.Log($"[CreateAcubeCommand] Created GameObject: {cube.name}");
    }
}

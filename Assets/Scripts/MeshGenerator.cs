using System.Collections.Generic;
using UnityEngine;

public class ProceduralMesh : MonoBehaviour
{
    [Header("Mesh Settings")]
    public int meshSize = 10;
    public float scale = 10f;
    public AnimationCurve heightCurve;
    public float heightMultiplier = 2f;

    [Header("Noise Settings")]
    public int seed = 42;
    public float noiseScale = 1f;

    [Header("Mesh Rotation")]
    public bool rotateMesh = false;

    public float[] layerScales = { 10f, 5f, 1f };
    public float[] layerAmplitudes = { 1f, 0.5f, 0.25f };
    public Vector2 offset = new Vector2(0f, 0f);

    [Header("Material Settings")]
    public Material groundMaterial;
    private Mesh mesh;

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        for (int z = 0; z <= meshSize; z++)
        {
            for (int x = 0; x <= meshSize; x++)
            {
                float height = GenerateNoise(x, z);
                vertices.Add(new Vector3(x, height, z));
                uvs.Add(new Vector2((float)x / meshSize, (float)z / meshSize));
            }
        }

        for (int z = 0; z < meshSize; z++)
        {
            for (int x = 0; x < meshSize; x++)
            {
                int topLeft = z * (meshSize + 1) + x;
                int topRight = topLeft + 1;
                int bottomLeft = (z + 1) * (meshSize + 1) + x;
                int bottomRight = bottomLeft + 1;

                triangles.Add(topLeft);
                triangles.Add(bottomLeft);
                triangles.Add(topRight);
                triangles.Add(topRight);
                triangles.Add(bottomLeft);
                triangles.Add(bottomRight);
            }
        }

        mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        if (groundMaterial != null)
        {
            meshRenderer.material = groundMaterial;
        }
        else
        {
            Debug.LogWarning("Ground material not assigned in the Inspector.");
        }
    }

    float GenerateNoise(int x, int z)
    {
        float height = 0f;

        for (int i = 0; i < layerScales.Length; i++)
        {
            float xCoord = (x + transform.position.x + offset.x) / layerScales[i];
            float zCoord = (z + transform.position.z + offset.y) / layerScales[i];
            float perlinValue = Mathf.PerlinNoise(xCoord + seed, zCoord + seed);
            height += perlinValue * layerAmplitudes[i];
        }

        float finalHeight = heightCurve.Evaluate(height) * heightMultiplier;
        if (rotateMesh)
        {
            finalHeight = -finalHeight;
        }

        Debug.Log($"Noise at ({x}, {z}): {height}, Height: {finalHeight}");
        return finalHeight;
    }
}


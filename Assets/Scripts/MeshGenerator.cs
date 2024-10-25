using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    public Renderer objectRenderer;
    Vector3[] vertices;
    int[] triangles;
    Color[] colors;
    private GameObject waterObject;
    private Mesh waterMesh;
    MeshRenderer waterMeshRenderer;

    public int baseXSize = 20;
    public int baseZSize = 20;
    private int xSegments;
    private int zSegments;

    public float heightMultiplier = 2f;
    public float noiseScale = 0.3f;

    public Gradient gradient;
    private float minTerrainHeight;
    private float maxTerrainHeight;

    public Slider heightSlider;
    public Slider detailSlider;
    public Slider noiseScaleSlider;

    public Material frameMaterial;
    public Material gradientMaterial;
    public Material waterMaterial;
    private bool frameMat = true;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateWaterObject();
        UpdateTerrain();
    }

    public void UpdateTerrain()
    {
        xSegments = Mathf.Max(1, Mathf.RoundToInt(baseXSize * detailSlider.value));
        zSegments = Mathf.Max(1, Mathf.RoundToInt(baseZSize * detailSlider.value));
        heightMultiplier = heightSlider.value;
        noiseScale = noiseScaleSlider.value;

        CreateShape();
        UpdateMesh();
        UpdateWaterMesh();
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSegments + 1) * (zSegments + 1)];

        minTerrainHeight = float.MaxValue;
        maxTerrainHeight = float.MinValue;

        for (int i = 0, z = 0; z <= zSegments; z++)
        {
            for (int x = 0; x <= xSegments; x++)
            {
                float xCoord = (float)x / xSegments * baseXSize;
                float zCoord = (float)z / zSegments * baseZSize;

                float y = Mathf.PerlinNoise(xCoord * noiseScale, zCoord * noiseScale) * heightMultiplier;
                vertices[i] = new Vector3(xCoord, y, zCoord);

                if (y > maxTerrainHeight) maxTerrainHeight = y;
                if (y < minTerrainHeight) minTerrainHeight = y;

                i++;
            }
        }

        int vert = 0;
        int tris = 0;
        triangles = new int[xSegments * zSegments * 6];

        for (int z = 0; z < zSegments; z++)
        {
            for (int x = 0; x < xSegments; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSegments + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSegments + 1;
                triangles[tris + 5] = vert + xSegments + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        GenerateColors();
    }

    void GenerateColors()
    {
        colors = new Color[vertices.Length];
        for (int z = 0, i = 0; z < zSegments; z++)
        {
            for (int x = 0; x < xSegments; x++)
            {
                float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[i].y);
                colors[i] = gradient.Evaluate(height);
                i++;
            }
        }
    }
    public void ChangeObjectMaterial()
    {
        if (frameMat)
        {
            objectRenderer.material = gradientMaterial;
            frameMat = false;
        }
        else
        {
            objectRenderer.material = frameMaterial;
            frameMat = true;
        }

        UpdateTerrain();
    }
    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();
    }

    void CreateWaterObject()
    {
        waterObject = new GameObject("WaterMesh");
        waterObject.transform.position = Vector3.zero;
        MeshFilter waterMeshFilter = waterObject.AddComponent<MeshFilter>();
        waterMeshRenderer = waterObject.AddComponent<MeshRenderer>();

        waterMesh = new Mesh();
        waterMeshFilter.mesh = waterMesh;
        waterMeshRenderer.material = waterMaterial;
    }
    void UpdateWaterMesh()
    {
        Vector3[] waterVertices = new Vector3[vertices.Length];
        int[] waterTriangles = new int[triangles.Length];
        int waterTriangleCount = 0;

        float averageTerrainHeight = 0f;
        foreach (var vertex in vertices)
        {
            averageTerrainHeight += vertex.y;
        }
        averageTerrainHeight /= vertices.Length;
        float waterHeight = averageTerrainHeight * 0.7f;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 terrainVertex = vertices[i];
            if (terrainVertex.y < waterHeight)
            {
                waterVertices[i] = new Vector3(terrainVertex.x, waterHeight, terrainVertex.z);
            }
            else
            {
                waterVertices[i] = terrainVertex;
            }
        }

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int v1 = triangles[i];
            int v2 = triangles[i + 1];
            int v3 = triangles[i + 2];

            if (waterVertices[v1].y == waterHeight && waterVertices[v2].y == waterHeight && waterVertices[v3].y == waterHeight)
            {
                waterTriangles[waterTriangleCount] = v1;
                waterTriangles[waterTriangleCount + 1] = v2;
                waterTriangles[waterTriangleCount + 2] = v3;
                waterTriangleCount += 3;
            }
        }

        waterMesh.Clear();
        waterMesh.vertices = waterVertices;
        waterMesh.triangles = waterTriangles;
        waterMesh.RecalculateNormals();
    }
}
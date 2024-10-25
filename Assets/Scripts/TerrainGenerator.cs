using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour
{
    Mesh mesh;
    public Renderer objectRenderer;
    public Gradient gradient;
    public Slider heightSlider;
    public Slider detailSlider;
    public Slider noiseScaleSlider;

    public Material frameMaterial;
    public Material gradientMaterial;

    public int baseXSize = 20;
    public int baseZSize = 20;
    private readonly int maxSegments = 1000;

    private int xSegments;
    private int zSegments;

    private float heightMultiplier = 2f;
    private float noiseScale = 0.3f;

    public Vector3[] vertices;
    public int[] triangles;
    private Color[] colors;

    public float minTerrainHeight;
    public float maxTerrainHeight;

    private bool frameMat = true;

    public event Action OnTerrainGenerated;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        UpdateTerrain();
    }

    public void UpdateTerrain()
    {
        //xSegments = Mathf.Max(1, Mathf.RoundToInt(baseXSize * detailSlider.value));
        xSegments = Mathf.Clamp(Mathf.RoundToInt(baseXSize * detailSlider.value), 1, maxSegments);
        zSegments = Mathf.Clamp(Mathf.RoundToInt(baseZSize * detailSlider.value), 1, maxSegments);
        heightMultiplier = heightSlider.value;
        noiseScale = noiseScaleSlider.value;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSegments + 1) * (zSegments + 1)];
        minTerrainHeight = float.MaxValue;
        maxTerrainHeight = float.MinValue;

        object lockObj = new(); // Блокування для синхронізації доступу до min/max висот

        Parallel.For(0, zSegments + 1, z =>
        {
            for (int x = 0; x <= xSegments; x++)
            {
                int i = z * (xSegments + 1) + x;
                float xCoord = (float)x / xSegments * baseXSize;
                float zCoord = (float)z / zSegments * baseZSize;
                float y = Mathf.PerlinNoise(xCoord * noiseScale, zCoord * noiseScale) * heightMultiplier;
                vertices[i] = new Vector3(xCoord, y, zCoord);

                // Оновлюємо мінімальну та максимальну висоту у безпечний спосіб
                lock (lockObj)
                {
                    if (y > maxTerrainHeight) maxTerrainHeight = y;
                    if (y < minTerrainHeight) minTerrainHeight = y;
                }
            }
        });
        /*for (int i = 0, z = 0; z <= zSegments; z++)
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
        }*/

        // int vert = 0;
        // int tris = 0;
        triangles = new int[xSegments * zSegments * 6];

        Parallel.For(0, zSegments, z =>
        {
            int localVert = z * (xSegments + 1);
            int localTris = z * (xSegments * 6);

            for (int x = 0; x < xSegments; x++)
            {
                triangles[localTris + 0] = localVert + 0;
                triangles[localTris + 1] = localVert + xSegments + 1;
                triangles[localTris + 2] = localVert + 1;
                triangles[localTris + 3] = localVert + 1;
                triangles[localTris + 4] = localVert + xSegments + 1;
                triangles[localTris + 5] = localVert + xSegments + 2;

                localVert++;
                localTris += 6;
            }
            localVert++;
        });
/*        for (int z = 0; z < zSegments; z++)
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
        }*/

        GenerateColors();

        OnTerrainGenerated?.Invoke();
    }

    void GenerateColors()
    {
        colors = new Color[vertices.Length];
        for (int z = 0, i = 0; z <= zSegments; z++)
        {
            for (int x = 0; x <= xSegments; x++)
            {
                float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[i].y);
                colors[i] = gradient.Evaluate(height);
                i++;
            }
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();
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
}

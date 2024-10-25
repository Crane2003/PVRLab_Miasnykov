using UnityEngine;
using UnityEngine.UI;

public class WaterGenerator : MonoBehaviour
{
    private GameObject waterObject;
    public Mesh waterMesh;
    private MeshRenderer waterMeshRenderer;

    public Material staticMaterial;
    public Material dynamicMaterial;
    private bool staticWater = true;

    private TerrainGenerator terrainGenerator;

    public float waveHeight = 0.5f;
    public float waveSpeed = 1f;
    public float waveFrequency = 0.2f;
    public int waveCount = 3;

    private Vector3[] originalVertices;
    private Vector3[] displacedVertices;

    public float waterDepth = 0.5f;
    public float transparency = 0.5f;
    public Slider depthSlider;
    public Slider transparencySlider;

    void Start()
    {
        terrainGenerator = GetComponent<TerrainGenerator>();
        terrainGenerator.OnTerrainGenerated += GenerateWater;

        depthSlider.onValueChanged.AddListener(OnDepthChanged);
        transparencySlider.onValueChanged.AddListener(OnTransparencyChanged);
    }

    void GenerateWater()
    {
        if (waterObject == null)
        {
            CreateWaterObject();
        }
        UpdateWaterMesh();
    }

    void CreateWaterObject()
    {
        waterObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        waterObject.transform.localScale = new Vector3(terrainGenerator.baseXSize / 10f, 1, terrainGenerator.baseZSize / 10f);

        waterMeshRenderer = waterObject.GetComponent<MeshRenderer>();
        waterMeshRenderer.material = staticMaterial;

        MeshFilter meshFilter = waterObject.GetComponent<MeshFilter>();
        waterMesh = meshFilter.mesh;
        waterMesh.RecalculateNormals();
        meshFilter.mesh = waterMesh;

        originalVertices = waterMesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];

        UpdateTransparency();
    }

    void Update()
    {
        if (!staticWater)
        {
            SimulateWaves();
        }
    }

    public void UpdateWaterMesh()
    {
        float waterHeight = Mathf.Lerp(terrainGenerator.minTerrainHeight, terrainGenerator.maxTerrainHeight, 0.2f) + waterDepth;
        if (waterObject != null)
        {
            waterObject.transform.position = new Vector3(terrainGenerator.baseXSize / 2f, waterHeight, terrainGenerator.baseZSize / 2f);
        }
    }

    public void ChangeObjectMaterial()
    {
        if (staticWater)
        {
            waterMeshRenderer.material = dynamicMaterial;
        }
        else
        {
            waterMeshRenderer.material = staticMaterial;
        }
        staticWater = !staticWater;

        UpdateWaterMesh();
    }

    void SimulateWaves()
    {
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];

            float waveY = 0f;

            for (int j = 0; j < waveCount; j++)
            {
                float frequency = waveFrequency + j * 0.2f;
                float amplitude = waveHeight / (j + 1);

                waveY += Mathf.Sin((vertex.x + Time.time * waveSpeed) * frequency) * amplitude;
                waveY += Mathf.Cos((vertex.z + Time.time * waveSpeed) * frequency) * amplitude;
            }

            vertex.y += waveY;
            displacedVertices[i] = vertex;
        }

        waterMesh.vertices = displacedVertices;
        waterMesh.RecalculateNormals();
    }

    void OnDepthChanged(float newDepth)
    {
        waterDepth = newDepth;
        UpdateWaterMesh();
    }

    void OnTransparencyChanged(float newTransparency)
    {
        transparency = newTransparency;
        UpdateTransparency();
    }

    void UpdateTransparency()
    {
        Color color = waterMeshRenderer.material.color;
        color.a = transparency;
        waterMeshRenderer.material.color = color;
    }
}

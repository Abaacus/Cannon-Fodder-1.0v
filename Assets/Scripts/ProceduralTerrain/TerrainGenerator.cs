using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public static TerrainGenerator instance;
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Multiple instances of " + this + " found");
        }
        instance = this;
    }

    public bool autoUpdate;
    public ShapeSettings shapeSettings;
    ShapeGenerator shapeGenerator = new ShapeGenerator();
    [HideInInspector]
    public bool shapeSettingsFoldOut;

    public int resolution;

    public int height;
    public int length;
    public int width;

    public float offset;
    public float baseStrength;
    [Range(0,1)]
    public float craterStrength;
    [Range(0.01f, 5)]
    public float scale;
    public float minHeight;
    Terrain terrain;

    public enum HeightType { perlin, multiFreqPerlin, multiFreqNoise};
    public HeightType heightType;

    public void GenerateNewTerrain()
    {
        offset = Random.Range(-10000f, 10000f);
        LoadTerrain();
    }

    public void LoadTerrain()
    {
        Initialize();
        GenerateTerrainData();
        GenerateColors();
    }

    void GenerateColors()
    {
        // notImplemented
    }

    void Initialize()
    {
        shapeGenerator.UpdateSettings(shapeSettings);
        terrain = GetComponent<Terrain>();
    }

    void GenerateTerrainData()
    {
        terrain.terrainData.heightmapResolution = resolution + 1;
        terrain.terrainData.baseMapResolution = resolution;

        terrain.terrainData.size = new Vector3(width, height, length);
        terrain.terrainData.SetHeights(0, 0, GenerateHeights());
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[resolution, resolution];

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                switch (heightType)
                {
                    default:
                    case HeightType.perlin:
                        heights[x, y] = GeneratePerlinHeight(x);
                        break;

                    case HeightType.multiFreqPerlin:
                        heights[x, y] = GenerateMultiFreqPerlinHeight(x, y);
                        break;

                    case HeightType.multiFreqNoise:
                        heights[x, y] = GenerateMulitFreqNoiseHeight(x);
                        break;
                }
              
            }
        }

        return heights;
    }

    float GenerateMultiFreqPerlinHeight(int x, int y)
    {
        float xCoord = offset + (float)x / length;
        float yCoord = offset + (float)y / length;

        float craterHeight = craterStrength * Mathf.PerlinNoise(xCoord / scale, 0);
        float baseHeight = baseStrength * Mathf.PerlinNoise(xCoord, 0);

        return baseHeight * craterHeight + minHeight;
    }

    float GeneratePerlinHeight(int x)
    {
        float xCoord = offset + (float)x / length;

        return Mathf.PerlinNoise(xCoord, 0);
    }

    float GenerateMulitFreqNoiseHeight(int x)
    {
        float unscaledElevation = shapeGenerator.CalculateUnscaledElevation(Vector3.forward * (offset + x));
        return unscaledElevation;   //shapeGenerator.GetScaledElevation(unscaledElevation);
    }
}

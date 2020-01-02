using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    TerrainGenerator terrain;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                terrain.LoadTerrain();
            }
        }

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Load Terrain"))
        {
            terrain.LoadTerrain();
        }

        if (GUILayout.Button("Generate New Terrain"))
        {
            terrain.GenerateNewTerrain();
        }

        GUILayout.EndHorizontal();
    }

    private void OnEnable()
    {
        terrain = (TerrainGenerator)target;
    }
}
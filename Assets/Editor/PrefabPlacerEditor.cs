using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrefabPlacer))]
public class PrefabPlacerEditor : Editor
{
    PrefabPlacer prefabPlacer;
    bool quadsEnabled;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Init Quads"))
            {
                prefabPlacer.cannonBoundary.InitializeQuad();
                prefabPlacer.targetBoundary.InitializeQuad();
            }

            quadsEnabled = EditorGUILayout.Toggle("Enable Quads", quadsEnabled);

            if (prefabPlacer.cannonBoundary != null)
            {
                DisplayBoundary(prefabPlacer.cannonBoundary, "Cannon");
            }

            GUILayout.Space(5);

            if (prefabPlacer.targetBoundary != null)
            {
                DisplayBoundary(prefabPlacer.targetBoundary, "Target");
            }

            if (check.changed)
            {
                prefabPlacer.UpdateBoundaries();
            }
        }
    }

    void DisplayBoundary(Boundary boundary, string name = "boundary ")
    {
        EditorGUIUtility.labelWidth = 115f;
        GUILayout.BeginHorizontal();
        boundary.quadMaterial = (Material)EditorGUILayout.ObjectField(name + "  |  Material", boundary.quadMaterial, typeof(Material), false);
        EditorGUIUtility.labelWidth = 80f;
        boundary.heightBuffer = EditorGUILayout.FloatField("Height Buffer", boundary.heightBuffer);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 100f;
        boundary.height = EditorGUILayout.FloatField("Transform | Y:", boundary.height, GUILayout.ExpandWidth(false));

        EditorGUIUtility.labelWidth = 25f;
        boundary.z1 = EditorGUILayout.FloatField("Z1:", boundary.z1, GUILayout.ExpandWidth(false));
        boundary.z2 = EditorGUILayout.FloatField("Z2:", boundary.z2, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        boundary.EnableQuad(quadsEnabled);
    }

    private void OnEnable()
    {
        prefabPlacer = (PrefabPlacer)target;
    }
}
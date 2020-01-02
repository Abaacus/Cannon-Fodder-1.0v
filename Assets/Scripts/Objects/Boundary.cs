using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
[System.Serializable]
public class Boundary : ScriptableObject
{
    public float z1;    // min Z value to place the object at
    public float z2;    // max Z value to place the object at
    public float height;    // visual boundary height

    public float heightBuffer;  // when placing the object, we need to raise it slightly so it isn't underground, that's this value

    public Transform objTransform;  // objects corresponding the boundary that get moved
    public Material quadMaterial;   // the visual boundaries material
    GameObject quad;    // the gameobject of the visual boundary

    public void GenerateNewPos()
    {
        float z = Random.Range(z1, z2);   // picks a random z value
        float y = CalculateHeight(z);  // generates the appropiate height for that z value
        objTransform.position = new Vector3(objTransform.position.x, y, z);   // moves the obj to the new pos
    }

    public float CalculateHeight(float z)   // calculates the y that makes the object be placed at to be above
    {
        int layerMask = 1 << 8;
        if (Physics.Raycast(new Vector3(0, height, z), Vector3.down, out RaycastHit hit, Mathf.Infinity, layerMask))
        {   // creates a ray from the boundary pad at the inputted y value, aiming downwards
            return hit.point.y + heightBuffer;  // returns the height the ray hit the terrain
        }

        Debug.LogError("No collider hit");  // if the ray doesn't hit anything, return an error and 0
        return 0;
    }

    public void UpdateBoundary()
    {
        if (quad == null)
        {   // if the quad doesn't exist, create it
            InitializeQuad();
        }

        UpdateTransform();  // update the boundary's position
        UpdateColours();    // update the boundary's colour

    }

    void UpdateTransform()
    {
        quad.GetComponent<MeshFilter>().sharedMesh.vertices = new Vector3[4]
        {
            new Vector3(1f, height, z1),
            new Vector3(-1f, height, z1),
            new Vector3(1f, height, z2),    // moves the meshs vertices to the boundary's parameters
            new Vector3(-1f, height, z2)
        };
    }

    void UpdateColours()
    {
        quad.GetComponent<MeshRenderer>().material = quadMaterial;  // sets the mesh to the quad's material
    }

    public void InitializeQuad()
    {
        if (quad != null)
        {
            DestroyImmediate(quad);  // if the quad already exists, then destroy it, a new one is being made
        }

        quad = new GameObject("boundaryQuad", typeof(MeshFilter), typeof(MeshRenderer));    // create a gameobject with the proper components
        quad.GetComponent<MeshFilter>().mesh = CreateQuad();
        quad.transform.parent = UnityEngine.Object.FindObjectOfType<PrefabPlacer>().transform;  // parent it to the prefab placer
    }

    Mesh CreateQuad()
    {
        Mesh mesh = new Mesh
        {
            vertices = new Vector3[4]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 0, 1),   // make a flat square
                new Vector3(1, 0, 1)
            },

            triangles = new int[12]
            {
                0, 2, 1,
                2, 3, 1,
                1, 2, 0,    // connect the triangles that will be rendered
                1, 3, 2
            },

            normals = new Vector3[4]
            {
                Vector3.forward,
                Vector3.forward,
                -Vector3.forward,   // create the normals for the boundarys
                -Vector3.forward
            },

            uv = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),  // initialize the uv
                new Vector2(0, 1),
                new Vector2(1, 1)
            }
        };

        return mesh;
    }

    public void EnableQuad(bool enable)
    {   // enable or disable the quads based on the parameter entered
        quad.GetComponent<MeshRenderer>().enabled = enable;
    }
}
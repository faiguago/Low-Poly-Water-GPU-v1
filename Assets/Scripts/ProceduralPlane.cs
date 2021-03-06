﻿using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralPlane : MonoBehaviour
{
    public enum WaterType
    {
        Wire,
        Normal
    }

    public WaterType waterType = 
        WaterType.Normal;

    [Range(2, 200)]
    public int size;
    [Range(0, 1)]
    public float noise;
    private Mesh mesh;

    private Vector4 tangent = 
        new Vector4(1, 0, 0, -1);
    
    private void Start() { }

    public void Generate()
    {
        if (waterType == WaterType.Wire)
        {
            size /= 2;
        }

        GetComponent<MeshFilter>().mesh = 
            mesh = new Mesh();
        
        List<Vector3> vertices = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<int> triangles = new List<int>();

        GenerateVertices(tangents, vertices);
        AddTriangles(triangles);
        CreateMesh(tangents, vertices, triangles);
    }

    private void GenerateVertices(List<Vector4> tangents, 
        List<Vector3> vertices)
    {
        for (int z = 0; z <= size; z++)
        {
            bool isPair = z % 2 == 0;
            int xSize = isPair ? size : size + 1;

            for (int x = 0; x <= xSize; x++)
            {
                if (isPair)
                {
                    AddVertex(vertices, new Vector3(x - size / 2f, 0, z - size / 2f));
                    tangents.Add(tangent);
                }
                else
                {
                    AddVertex(vertices, new Vector3(x - 0.5f - size / 2f, 0, z - size / 2f));
                    tangents.Add(tangent);
                }
            }
        }
    }

    private void AddVertex(List<Vector3> vertices, Vector3 pos)
    {
        if (noise > 0f)
        {
            Vector2 offset = Random.insideUnitCircle * noise * 0.5f;
            pos = new Vector3(pos.x + offset.x, 0, pos.z + offset.y);
        }

        vertices.Add(pos);
    }

    private void AddTriangles(List<int> triangles)
    {
        for (int z = 0, i = 0; z < size; z++, i++)
        {
            bool isPair = z % 2 == 0;
            int xSize = isPair ? size : size + 1;

            for (int x = 0; x < xSize; x++, i++)
            {
                if (isPair)
                {
                    triangles.Add(i);
                    triangles.Add(i + size + 1);
                    triangles.Add(i + size + 2);
                    triangles.Add(i);
                    triangles.Add(i + size + 2);
                    triangles.Add(i + 1);

                    if (x == xSize - 1)
                    {
                        triangles.Add(i + 1);
                        triangles.Add(i + size + 2);
                        triangles.Add(i + size + 3);
                    }
                }
                else
                {
                    if (x != 0)
                    {
                        triangles.Add(i);
                        triangles.Add(i - 1);
                        triangles.Add(i + size + 1);
                        triangles.Add(i);
                        triangles.Add(i + size + 1);
                        triangles.Add(i + size + 2);

                        if (x == xSize - 1)
                        {
                            triangles.Add(i);
                            triangles.Add(i + size + 2);
                            triangles.Add(i + 1);
                        }
                    }
                }
            }
        }
    }

    private void CreateMesh(List<Vector4> tangents, List<Vector3> vertices,
        List<int> triangles)
    {
        if (waterType == WaterType.Wire)
        {
            tangents.Clear();
            Vector3[] newVertices = new Vector3[triangles.Count];
            for (int i = 0; i < triangles.Count; i++)
            {
                newVertices[i] = vertices[triangles[i]];
                tangents.Add(tangent);
                triangles[i] = i;
            }

            Color[] barCoords = new Color[triangles.Count];
            for (int i = 0; i < triangles.Count; i += 3)
            {
                barCoords[i] = new Color(1, 0, 0);
                barCoords[i + 1] = new Color(0, 1, 0);
                barCoords[i + 2] = new Color(0, 0, 1);
            }

            mesh.vertices = newVertices;
            mesh.triangles = triangles.ToArray();
            mesh.tangents = tangents.ToArray();
            mesh.colors = barCoords;
        }
        else
        {
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.tangents = tangents.ToArray();
        }
    }
    
}

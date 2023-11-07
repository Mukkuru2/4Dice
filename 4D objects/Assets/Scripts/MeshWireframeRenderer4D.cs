using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MeshWireframeRenderer4D : MonoBehaviour
{
    public GameObject CylinderPrefab;
    public GameObject VertexPrefab;
    public MeshFilter Mesh3D;
    public Transform4D transform4D;
    
    private GameObject[] EdgeGameobjects;
    private GameObject[] VertexGameobjects;

    private OuterPoints outerPoints4D;
    public OuterPoints OuterPoints4D { get => outerPoints4D; private set => outerPoints4D = value; }

    public struct OuterPoints
    {
        public int xMin;
        public int xMax;
        public int yMin;
        public int yMax;
        public int zMin;
        public int zMax;
    }
    
    public void Render()
    {
        GetWireframeMesh(transform4D.Vertices, transform4D.Mesh.Edges);
        SetOuterPoints();
        // Mesh3D.mesh = mesh;
    }

    public void GetWireframeMesh(Vector4[] vertices, Mesh4D.Edge[] edges)
    {
        float factor = 2f;

        // Initialise the edge gameobjects array if it is null or not the same size as the edges array
        if (EdgeGameobjects == null || VertexGameobjects == null || EdgeGameobjects.Length != edges.Length ||
            VertexGameobjects.Length != vertices.Length)
        {
            EdgeGameobjects = new GameObject[edges.Length];
            VertexGameobjects = new GameObject[vertices.Length];
        }

        // Loop through edges and create a cylinder if it doesn't already exist
        // Each cylinder will have 2 vertices for each end of the cylinder

        for (var edgeIndex = 0; edgeIndex < edges.Length; edgeIndex++)
        {
            var edge = edges[edgeIndex];
            Vector3 start = new Vector3(vertices[edge.Index0].x, vertices[edge.Index0].y, vertices[edge.Index0].z);
            start *= Mathf.Pow(factor, vertices[edge.Index0].w);
            start += transform.position;

            Vector3 end = new Vector3(vertices[edge.Index1].x, vertices[edge.Index1].y, vertices[edge.Index1].z);
            end *= Mathf.Pow(factor, vertices[edge.Index1].w);
            end += transform.position;

            // Check if a cylinder exists in the EdgeGameobjects array. If not, create one
            if (!EdgeGameobjects[edgeIndex])
            {
                EdgeGameobjects[edgeIndex] = Instantiate(CylinderPrefab, transform);
            }

            EdgeGameobjects[edgeIndex].transform.position = start;
            EdgeGameobjects[edgeIndex].transform.LookAt(end, Vector3.up);

            // Scale the cylinder between the two points
            float distance = Vector3.Distance(start, end);
            EdgeGameobjects[edgeIndex].transform.localScale = new Vector3(1, 1, distance);
        }

        // Loop over the vertices
        for (var vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
        {
            var vertex = vertices[vertexIndex];
            Vector3 position = new Vector3(vertex.x, vertex.y, vertex.z);
            position *= Mathf.Pow(factor, vertex.w);
            position += transform.position;

            if (!VertexGameobjects[vertexIndex])
            {
                VertexGameobjects[vertexIndex] = Instantiate(VertexPrefab, position, Quaternion.identity, transform);
            }
            else
            {
                VertexGameobjects[vertexIndex].transform.position = position;
            }
        }
    }

    public void SetOuterPoints()
    {
        // Loop through all the vertices and find the min and max x, y and z
        OuterPoints4D = new OuterPoints();
        
        for (int i = 0; i < transform4D.Vertices.Length; i++)
        {
            Vector4 vertex = transform4D.Vertices[i];

            if (vertex.x < transform4D.Vertices[OuterPoints4D.xMin].x)
            {
                outerPoints4D.xMin = i;
            }
            else if (vertex.x > transform4D.Vertices[OuterPoints4D.xMax].x)
            {
                outerPoints4D.xMax = i;
            }

            if (vertex.y < transform4D.Vertices[OuterPoints4D.yMin].y)
            {
                outerPoints4D.yMin = i;
            }
            else if (vertex.y > transform4D.Vertices[OuterPoints4D.yMax].y)
            {
                outerPoints4D.yMax = i;
            }

            if (vertex.z < transform4D.Vertices[OuterPoints4D.zMin].z)
            {
                outerPoints4D.zMin = i;
            }
            else if (vertex.z > transform4D.Vertices[OuterPoints4D.zMax].z)
            {
                outerPoints4D.zMax = i;
            }
        }
        
        // Loop through all vertices
        // Make it red if it is an outer point
        // Make it white if it is not an outer point
        for (int i = 0; i < VertexGameobjects.Length; i++)
        {
            if (i == OuterPoints4D.xMin || i == OuterPoints4D.xMax || i == OuterPoints4D.yMin || i == OuterPoints4D.yMax ||
                i == OuterPoints4D.zMin || i == OuterPoints4D.zMax)
            {
                VertexGameobjects[i].GetComponent<MeshRenderer>().material.color = Color.red;
            }
            else
            {
                VertexGameobjects[i].GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }
    }

    // When disabled, disable all children
    public void Disable()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    // When enabled, enable all children
    public void Enable()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }
}
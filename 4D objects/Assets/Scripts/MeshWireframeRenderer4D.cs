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

    public void Render()
    {
        Mesh mesh = GetWireframeMesh(transform4D.Vertices, transform4D.Mesh.Edges);
        // Mesh3D.mesh = mesh;
    }
    
    public Mesh GetWireframeMesh(Vector4[] vertices, Mesh4D.Edge[] edges)
    {
        Mesh mesh = new Mesh();
        float factor = 2f;
        
        // Initialise the edge gameobjects array if it is null or not the same size as the edges array
        if (EdgeGameobjects == null || VertexGameobjects == null || EdgeGameobjects.Length != edges.Length || VertexGameobjects.Length != vertices.Length)
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
            start *=  Mathf.Pow(factor, vertices[edge.Index0].w);
            start += transform.position;

            Vector3 end = new Vector3(vertices[edge.Index1].x, vertices[edge.Index1].y, vertices[edge.Index1].z);
            end *=  Mathf.Pow(factor, vertices[edge.Index1].w);
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

            var debug = EdgeGameobjects[edgeIndex].GetComponent<EdgeDebug>();
            debug.index0 = edge.Index0;
            debug.index1 = edge.Index1;
            
        }
        
        // Loop over the vertices
        for (var vertexIndex = 0; vertexIndex < vertices.Length; vertexIndex++)
        {
            
            var vertex = vertices[vertexIndex];
            Vector3 position = new Vector3(vertex.x, vertex.y, vertex.z);
            position *=  Mathf.Pow(factor, vertex.w);
            position += transform.position;

            if (!VertexGameobjects[vertexIndex])
            {
                VertexGameobjects[vertexIndex] = Instantiate(VertexPrefab, position, Quaternion.identity, transform);
            }
            else
            {
                VertexGameobjects[vertexIndex].transform.position = position;
            }

            VertexDebug debug = VertexGameobjects[vertexIndex].GetComponent<VertexDebug>();
            debug.index = vertexIndex;
        }

        return mesh;
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

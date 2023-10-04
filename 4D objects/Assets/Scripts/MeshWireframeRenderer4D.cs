using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MeshWireframeRenderer4D : MonoBehaviour
{
    
    public GameObject CylinderPrefab;
    public MeshFilter Mesh3D;
    public Transform4D transform4D;

    public GameObject[] EdgeGameobjects;
    
    private void Start()
    {
        Mesh3D = GetComponent<MeshFilter>();
        
        transform4D = GetComponent<Transform4D>();
    }

    public void Render()
    {
        Mesh mesh = GetWireframeMesh(transform4D.Vertices, transform4D.Mesh.Edges);
        // Mesh3D.mesh = mesh;
    }
    
    public Mesh GetWireframeMesh(Vector4[] vertices, Mesh4D.Edge[] edges)
    {
        Mesh mesh = new Mesh();
        float factor = 1.5f;
        
        // Initialise the edge gameobjects array if it is null or not the same size as the edges array
        if (EdgeGameobjects == null || EdgeGameobjects.Length != edges.Length)
        {
            EdgeGameobjects = new GameObject[edges.Length];
        }
        
        // Loop through edges and create a cylinder if it doesn't already exist
        // Each cylinder will have 2 vertices for each end of the cylinder
        for (var edgeIndex = 0; edgeIndex < edges.Length; edgeIndex++)
        {
            var edge = edges[edgeIndex];
            Vector3 pos = transform4D.Position;

            Vector3 start = new Vector3(vertices[edge.Index0].x, vertices[edge.Index0].y, vertices[edge.Index0].z);
            start -= pos;
            start *=  Mathf.Pow(factor, vertices[edge.Index0].w);
            start += pos;

            Vector3 end = new Vector3(vertices[edge.Index1].x, vertices[edge.Index1].y, vertices[edge.Index1].z);
            end -= pos;
            end *=  Mathf.Pow(factor, vertices[edge.Index1].w);
            end += pos;
            
            
            // Check if a cylinder exists in the EdgeGameobjects array. If not, create one
            if (EdgeGameobjects[edgeIndex] == null)
            {
                EdgeGameobjects[edgeIndex] = Instantiate(CylinderPrefab, transform);
            }

            EdgeGameobjects[edgeIndex].transform.position = start;
            EdgeGameobjects[edgeIndex].transform.LookAt(end, Vector3.up);
            
            // Scale the cylinder between the two points
            float distance = Vector3.Distance(start, end);
            EdgeGameobjects[edgeIndex].transform.localScale = new Vector3(1, 1, distance);
        }

        return mesh;
    }
    
    // When disabled, disable all children
    private void OnDisable()
    {
        foreach (Transform child in transform)
        {
            if (child != transform) child.gameObject.SetActive(false);
        }
    }
    
    // When enabled, enable all children
    private void OnEnable()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
    }
}

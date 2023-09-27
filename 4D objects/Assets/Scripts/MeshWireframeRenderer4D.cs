using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshWireframeRenderer4D : MonoBehaviour
{
    
    public GameObject CylinderPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Mesh Render(Vector4[] vertices, Mesh4D.Edge[] meshEdges)
    {
        // Kill all children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        
        // Create a cylinder for each edge. Loop through edges
        // and create a cylinder for each one.
        // Each cylinder will have 2 vertices for each end of the cylinder

        Mesh mesh = new Mesh();
        CombineInstance[] combine = new CombineInstance[meshEdges.Length];

        float factor = 1.5f;
        
        foreach (Mesh4D.Edge edge in meshEdges)
        {
            Vector3 start = new Vector3(vertices[edge.Index0].x, vertices[edge.Index0].y, vertices[edge.Index0].z) * Mathf.Pow(factor, -vertices[edge.Index0].w);
            Vector3 end = new Vector3(vertices[edge.Index1].x, vertices[edge.Index1].y, vertices[edge.Index1].z) * Mathf.Pow(factor, -vertices[edge.Index1].w);
            
            // Instantiate a cylinder 
            GameObject cylinder = Instantiate(CylinderPrefab, start, Quaternion.identity, transform);
            cylinder.transform.LookAt(end, Vector3.up);
            
            // Scale the cylinder between the two points
            float distance = Vector3.Distance(start, end);
            cylinder.transform.localScale = new Vector3(1, 1, distance);
            
        }

        return mesh;
    }
}

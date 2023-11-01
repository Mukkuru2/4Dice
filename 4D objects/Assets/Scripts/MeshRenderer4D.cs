using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MIConvexHull;


public class MeshRenderer4D : MonoBehaviour
{
    public MeshFilter Mesh3D;
    public Transform4D transform4D;
    
    private Vector3[] vertices3 = Array.Empty<Vector3>();
    private DefaultVertex[] calculationVertices = Array.Empty<DefaultVertex>();
    private int[] triangles = Array.Empty<int>();
    
    public void Render()
    {
        Intersect(transform4D.Vertices, transform4D.Mesh.Edges);
    }
    public void Intersect(Vector4[] vertices, Mesh4D.Edge[] edges)
    {
        // Calculates the intersections
        List<Vector3> intersectionVertices = new List<Vector3>();
        foreach (Mesh4D.Edge edge in edges)
            Intersection
            (
                intersectionVertices,
                vertices[edge.Index0],
                vertices[edge.Index1]
            );

        // Not enough intersection points!
        if (intersectionVertices.Count < 3)
        {
            // Set all vertices in the vertices3 array to 0
            for (int i = 0; i < vertices3.Length; i++)
            {
                vertices3[i] = Vector3.zero;
            }
            
            return;
        }

        // Creates and returns the mesh
        CreateMesh(intersectionVertices.ToArray());
        intersectionVertices.Clear();
    }

    private void CreateMesh(Vector3[] v3)
    {
        if (calculationVertices.Length != v3.Length)
        {
            calculationVertices = new DefaultVertex[v3.Length];
        }

        for (int i = 0; i < calculationVertices.Length; i++)
        {
            calculationVertices[i] = new DefaultVertex();
            calculationVertices[i].Position = new double[] { v3[i].x, v3[i].y, v3[i].z };
        }

        // Creates the convex null
        var result = ConvexHull.Create(calculationVertices).Result;

        // No convex hull found
        if (result == null)
        {
            return;
        }

        // Check if more vertices and triangles are needed
        if (vertices3.Length < result.Faces.Count() * 3)
        {
            vertices3 = new Vector3[result.Faces.Count() * 3];
            triangles = new int[result.Faces.Count() * 3];
        }

        int v = 0;
        foreach (var face in result.Faces)
        {
            vertices3[v] = new Vector3((float)face.Vertices[0].Position[0], (float)face.Vertices[0].Position[1],
                (float)face.Vertices[0].Position[2]);
            triangles[v] = v++;

            vertices3[v] = new Vector3((float)face.Vertices[1].Position[0], (float)face.Vertices[1].Position[1],
                (float)face.Vertices[1].Position[2]);
            triangles[v] = v++;

            vertices3[v] = new Vector3((float)face.Vertices[2].Position[0], (float)face.Vertices[2].Position[1],
                (float)face.Vertices[2].Position[2]);
            triangles[v] = v++;
        }
        
        // Fill rest of vector3 array with 0s
        for (int i = v; i < vertices3.Length; i++)
        {
            vertices3[i] = Vector3.zero;
            triangles[i] = 0;
        }

        Mesh mesh = Mesh3D.mesh;
        mesh.Clear();
        mesh.vertices = vertices3;
        mesh.triangles = triangles;
        
        mesh.RecalculateNormals();
    }

    private int Intersection(List<Vector3> list, Vector4 v0, Vector4 v1)
    {
        // Both points are 3D ==> the entire segment lies in the 3D space
        if (v1.w == 0 && v0.w == 0)
        {
            list.Add(v0);
            list.Add(v1);
            return 2;
        }

        // Both w coordinates are equal
        // If they are both 0 ==> the entire line is in the 3D space (already tested)
        // If they are not 0 ==> the entire line is outside the 3D space
        if (v1.w - v0.w == 0)
            return 0;

        // Time of intersection
        float t = -v0.w / (v1.w - v0.w);

        // No intersection
        if (t < 0 || t > 1)
            return 0;

        // One intersection
        Vector4 x = v0 + (v1 - v0) * t;
        list.Add(x);
        return 1;
    }

    // On disable, disable the Mesh3D
    public void Disable()
    {
        Mesh3D.mesh = new Mesh();
    }
}
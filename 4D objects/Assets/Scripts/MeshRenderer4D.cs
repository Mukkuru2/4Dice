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

    private void Start()
    {
        Mesh3D = GetComponent<MeshFilter>();
        
        transform4D = GetComponent<Transform4D>();
    }

    public void Render()
    {
        Mesh mesh = Intersect(transform4D.Vertices, transform4D.Mesh.Edges);
        Mesh3D.mesh = mesh;
    }
    public Mesh Intersect(Vector4[] vertices, Mesh4D.Edge[] edges)
    {
        // Calculates the intersections
        List<Vector3> vert3d = new List<Vector3>();
        foreach (Mesh4D.Edge edge in edges)
            Intersection
            (
                vert3d,
                vertices[edge.Index0],
                vertices[edge.Index1]
            );

        // Not enough intersection points!
        if (vert3d.Count < 3)
            return null;

        // Creates and returns the mesh
        return CreateMesh(vert3d);
    }
    
    Mesh CreateMesh(List<Vector3> unsortedVector3)
    {
        // Vertex <- Vector3
        DefaultVertex[] vertices = new DefaultVertex[unsortedVector3.Count];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new DefaultVertex();
            vertices[i].Position = new double[] { unsortedVector3[i].x, unsortedVector3[i].y, unsortedVector3[i].z };
        }

        // Creates the convex null
        var result = ConvexHull.Create(vertices).Result;

        // No convex hull found
        if (result == null)
        {
            return Mesh3D.mesh;
        }
        
        // Create Mesh3D
        Vector3[] vertices3 = new Vector3[result.Faces.Count() * 3];
        int[] triangles = new int[result.Faces.Count() * 3];

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

        Mesh mesh = new Mesh();
        mesh.vertices = vertices3;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        return mesh;
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
    private void OnDisable()
    {
        Mesh3D.mesh = new Mesh();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MIConvexHull;
using UnityEngine;
using Vector4 = UnityEngine.Vector4;

public class Mesh4D : MonoBehaviour
{
    public Vector4[] Vertices;
    public Edge[] Edges;


    [Serializable]
    public struct Edge
    {
        public int Index0;
        public int Index1;

        public Edge(int index0, int index1)
        {
            Index0 = index0;
            Index1 = index1;
        }
    }

    public void Initialise()
    {
        Vertices = GetHypercubeVertices();
        Edges = GetHypercubeEdges(Vertices);
    }

    // Get the vertices of a hypercube
    private Vector4[] GetHypercubeVertices()
    {
        Vector4[] vertices = new Vector4[16];
        // Vertices with the normal 3d part of a cube and the w set to 1
        vertices[0] = new Vector4(-1, -1, -1, 1);
        vertices[1] = new Vector4(-1, -1, 1, 1);
        vertices[2] = new Vector4(-1, 1, -1, 1);
        vertices[3] = new Vector4(-1, 1, 1, 1);
        vertices[4] = new Vector4(1, -1, -1, 1);
        vertices[5] = new Vector4(1, -1, 1, 1);
        vertices[6] = new Vector4(1, 1, -1, 1);
        vertices[7] = new Vector4(1, 1, 1, 1);
        // Vertices with the normal 3d part of a cube and the w set to -1
        vertices[8] = new Vector4(-1, -1, -1, -1);
        vertices[9] = new Vector4(-1, -1, 1, -1);
        vertices[10] = new Vector4(-1, 1, -1, -1);
        vertices[11] = new Vector4(-1, 1, 1, -1);
        vertices[12] = new Vector4(1, -1, -1, -1);
        vertices[13] = new Vector4(1, -1, 1, -1);
        vertices[14] = new Vector4(1, 1, -1, -1);
        vertices[15] = new Vector4(1, 1, 1, -1);

        return vertices;
    }

    // Get the edges of a hypercube
    private Edge[] GetHypercubeEdges(Vector4[] vertices)
    {
        // Vertex <- Vector4
        DefaultVertex[] verts = new DefaultVertex[vertices.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = new DefaultVertex();
            verts[i].Position = new double[] { vertices[i].x, vertices[i].y, vertices[i].z, vertices[i].w };
        }

        // Creates the convex null
        var result = ConvexHull.Create(verts).Result;


        // Set the edges to the vertices
        List<Edge> edges = new List<Edge>();
        List<Vector4> verts4 = new List<Vector4>();
        
        foreach (var face in result.Faces)
        {
            Vector4[] faceVerts = new Vector4[4];
            int[] faceIndices = new int[4];
            // Get index of each vertex in the face
            for (int i = 0; i < 4; i++)
            {
                faceVerts[i] = new Vector4((float)face.Vertices[i].Position[0], (float)face.Vertices[i].Position[1],
                    (float)face.Vertices[i].Position[2], (float)face.Vertices[i].Position[3]);
                // Get the index of vert if it is already in the list, else add it to the list
                faceIndices[i] = verts4.IndexOf(faceVerts[i]);
                if (faceIndices[i] == -1)
                {
                    verts4.Add(faceVerts[i]);
                    faceIndices[i] = verts4.Count - 1;
                }
            }
        }
        
        float smallest = GetSmallestDistance(verts4.ToArray());

        // Loop through all the vertices
        foreach (Vector4 vertex in verts4)
        {
            foreach (Vector4 vertex2 in verts4)
            {
                // If the vertices are the same, skip
                if (vertex == vertex2)
                {
                    continue;
                }
                
                // If the distance is 2, add the edge
                if (Vector4.Distance(vertex, vertex2) == smallest)
                {
                    edges.Add(new Edge(verts4.IndexOf(vertex), verts4.IndexOf(vertex2)));
                }
            }
        }

        print(edges.Count);
        

        Vertices = verts4.ToArray();
        return edges.ToArray();
    }

    private float GetSmallestDistance(Vector4[] verts)
    {
        // Get the smallest distance between two vertices
        float smallest = float.MaxValue;
        foreach (Vector4 vertex in verts)
        {
            foreach (Vector4 vertex2 in verts)
            {
                // If the vertices are the same, skip
                if (vertex == vertex2)
                {
                    continue;
                }
                
                // If the distance is smaller than the smallest, set the smallest to the distance
                float distance = Vector4.Distance(vertex, vertex2);
                if (distance < smallest)
                {
                    smallest = distance;
                }
            }
        }

        return smallest;
    }
}
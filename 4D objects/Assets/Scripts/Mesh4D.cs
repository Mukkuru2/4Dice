using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Edges = GetHypercubeEdges();
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
    private Edge[] GetHypercubeEdges()
    {
        Edge[] edges = new Edge[32];

        // Edges of the first 3d cube, with the w set to 1
        edges[0] = new Edge(0, 1);
        edges[1] = new Edge(0, 2);
        edges[2] = new Edge(0, 4);
        edges[3] = new Edge(1, 3);
        edges[4] = new Edge(1, 5);
        edges[5] = new Edge(2, 3);
        edges[6] = new Edge(2, 6);
        edges[7] = new Edge(3, 7);
        edges[8] = new Edge(4, 5);
        edges[9] = new Edge(4, 6);
        edges[10] = new Edge(5, 7);
        edges[11] = new Edge(6, 7);
        // Edges of the second 3d cube, with the w set to -1
        edges[12] = new Edge(8, 9);
        edges[13] = new Edge(8, 10);
        edges[14] = new Edge(8, 12);
        edges[15] = new Edge(9, 11);
        edges[16] = new Edge(9, 13);
        edges[17] = new Edge(10, 11);
        edges[18] = new Edge(10, 14);
        edges[19] = new Edge(11, 15);
        edges[20] = new Edge(12, 13);
        edges[21] = new Edge(12, 14);
        edges[22] = new Edge(13, 15);
        edges[23] = new Edge(14, 15);

        // Connecting edges
        edges[24] = new Edge(0, 8);
        edges[25] = new Edge(1, 9);
        edges[26] = new Edge(2, 10);
        edges[27] = new Edge(3, 11);
        edges[28] = new Edge(4, 12);
        edges[29] = new Edge(5, 13);
        edges[30] = new Edge(6, 14);
        edges[31] = new Edge(7, 15);



        return edges;
    }

}
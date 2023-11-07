using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using static Mesh4D;

public static class Terrain4D
{

    public static float period = 30f;
    
    public static Vector4[] GetTerrainVertices(Vector3 pos)
    {
        
        List<Vector4> vertices = new();

        for (int i = -1; i <= 1; i+=2)
        {
            for (int j = -1; j <= 1; j+=2)
            {
                for (int k = -1; k <= 1; k+=2)
                {
                    float y = 8 + 2 * noise.snoise(new float3((i + pos.x) / period + 1000, (j + pos.z) / period + 1000, k));
                    vertices.Add(new Vector4(i, 0, j, k));
                    vertices.Add(new Vector4(i, y, j, k));
                }
            }
        }
        
        return vertices.ToArray();
    }

    public static Edge[] GetTerrainEdges(Vector4[] vertices, float gridDistance)
    {
        List<Edge> edges = new List<Edge>();

        // Loop through all the vertices
        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = vertices[i];
            for (var j = i + 1; j < vertices.Length; j++)
            {
                var vertex2 = vertices[j];
                
                // We want to check if v1.x - v2.x == gridDistance, but also that v1.z - v2.z == 0, v1.w - v2.w == 0.
                // We can do this by casting the x, z and w to a vector3 and looking at the distance.
                // We don't look at the distance between the vector4s, since the y can be different.
                // When this is true, either the y are both 0 or both not 0 to have an edge between them.
                //
                // We also check for two vertices on the same x,z,w since that will be an edge too. 
                
                Vector3 dist1 = new(vertex.x, vertex.z, vertex.w);
                Vector3 dist2 = new(vertex2.x, vertex2.z, vertex2.w);
                
                bool bothYZero = vertex.y == 0 && vertex2.y == 0;
                bool bothYNotZero = vertex.y != 0 && vertex2.y != 0;
                bool sameXZW = vertex.x == vertex2.x && vertex.z == vertex2.z && vertex.w == vertex2.w;
                bool oneGridUnitAwayFromEachother = math.abs(math.distance(dist1, dist2) - gridDistance) < 0.0001f;   
                
                if ((oneGridUnitAwayFromEachother && (bothYZero || bothYNotZero)) || sameXZW)
                {
                    edges.Add(new Edge(i, j));
                }
            }
        }

        return edges.ToArray();
    }
}
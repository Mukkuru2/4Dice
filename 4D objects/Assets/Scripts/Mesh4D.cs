using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector4 = UnityEngine.Vector4;

public class Mesh4D : MonoBehaviour
{
    public Vector4[] Vertices;
    public Edge[] Edges;
    public Shapes Shape;

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
    
    
    public enum Shapes
    {
        None,
        Cell5,
        Hypercube,
        Cell16,
        Cell24,
        Cell120,
        Cell600,
        Terrain
    }

  public void Initialise()
    {
        if (Polychora.IsPolychoron(Shape))
        {
            Vertices = Polychora.GetPolychoronVertices(Shape);
            Edges = Polychora.GetPolychoronEdges(Vertices, Shape);
            // Normalise all vertices
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = Vertices[i].normalized;
            }
        }
        
        if (Shape == Shapes.Terrain)
        {
            Vertices = Terrain4D.GetTerrainVertices(transform.position);
            Edges = Terrain4D.GetTerrainEdges(Vertices, 2);
        }
        
    }


}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector4 = UnityEngine.Vector4;

public class Mesh4D : MonoBehaviour
{
    public Vector4[] Vertices;
    public Edge[] Edges;
    public Shapes Shape = Shapes.None;
    
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
    
    public struct ShapeSeed
    {
        public Dictionary<Vector4, Parity> Seeds;
        public float EdgeLength;

        public ShapeSeed(Dictionary<Vector4, Parity> seeds, float edgeLength)
        {
            Seeds = seeds;
            EdgeLength = edgeLength;
        }
    }

    public enum Shapes
    {
        None,
        Hypercube,
        Cell16,
        Cell24,
        Cell120,
    }
    
    public enum Parity
    {
        All,
        Even,
        Odd
    }
    
    private static readonly float PHI = (1 + Mathf.Sqrt(5)) / 2;
    private static readonly float PHIn2 = Mathf.Pow(PHI, -2);
    private static readonly float PHIn1 = Mathf.Pow(PHI, -1);
    private static readonly float PHI2 = Mathf.Pow(PHI, 2);
    private static readonly float PHI3 = Mathf.Pow(PHI, 3);
    private static readonly float PHI4 = Mathf.Pow(PHI, 4);
    private static readonly float PHI5 = Mathf.Pow(PHI, 5);
    private static readonly float PHI6 = Mathf.Pow(PHI, 6);
    


    private readonly Dictionary<Shapes, ShapeSeed> _shapes = new Dictionary<Shapes, ShapeSeed>()
    {
        {
            Shapes.Hypercube, new(new Dictionary<Vector4, Parity>()
            {
                {new Vector4(1, 1, 1, 1), Parity.All},
            }, 2)
        },
        {
            Shapes.Cell16, new(new Dictionary<Vector4, Parity>()
            {
                {new Vector4(1, 0, 0, 0), Parity.All},
                
            }, Mathf.Sqrt(2))
        },
        {
            Shapes.Cell24, new(new Dictionary<Vector4, Parity>()
            {
                {new Vector4(1, 1, 0, 0), Parity.All},
            }, Mathf.Sqrt(2))
        },
        {
            Shapes.Cell120, new(new Dictionary<Vector4, Parity>()
            {
                // All
                {new Vector4(2, 2, 0, 0), Parity.All},
                {new Vector4(Mathf.Sqrt(5), 1, 1, 1), Parity.All},
                {new Vector4(PHI, PHI, PHI, PHIn2), Parity.All},
                {new Vector4(PHI2, PHIn1, PHIn1, PHIn1), Parity.All},
                
                // Even
                {new Vector4(PHI2, PHIn2, 1, 0), Parity.Even},
                {new Vector4(Mathf.Sqrt(5), PHIn1, PHI, 0), Parity.Even},
                {new Vector4(2, 1, PHI, PHIn1), Parity.Even},
            }, 3 - Mathf.Sqrt(5))
        },
    };

    public void Initialise()
    {
        Vertices = GetPolychoronVertices(Shape);
        Edges = GetPolychoronEdges();
        
        // Normalise all vertices
        for (int i = 0; i < Vertices.Length; i++)
        {
            Vertices[i] = Vertices[i].normalized;
        }
    }

    // Get the vertices of a hypercube
    private Vector4[] GetPolychoronVertices(Shapes shape)
    {
        List<Vector4> vertices = new List<Vector4>();
        
        // Loop through the dictionary and add the vertices
        foreach (KeyValuePair<Vector4, Parity> seed in _shapes[shape].Seeds)
        {
            vertices.AddRange(PlusMinusPermutations(seed.Key, seed.Value));
        }
        
        return vertices.ToArray();
    }

    // Get the edges of a hypercube
    private Edge[] GetPolychoronEdges()
    {
        List<Edge> edges = new List<Edge>();
        
        // Loop through all the vertices
        for (var i = 0; i < Vertices.Length; i++)
        {
            var vertex = Vertices[i];
            for (var j = i + 1; j < Vertices.Length; j++)
            {
                var vertex2 = Vertices[j];
                
                // If the distance is equal to size, add the edge
                if (Math.Abs(Vector4.Distance(vertex, vertex2) - _shapes[Shape].EdgeLength) < 0.001f)
                {
                    edges.Add(new Edge(i, j));
                }
            }
        }
        
        return edges.ToArray();
    }

    private void AddVerticesOfSize(List<Edge> edges, float dist)
    {
        int log = 0;
        // Loop through all the vertices
        for (var i = 0; i < Vertices.Length; i++)
        {
            var vertex = Vertices[i];
            for (var j = i + 1; j < Vertices.Length; j++)
            {
                var vertex2 = Vertices[j];
                
                // If the distance is equal to size, add the edge
                if (Math.Abs(Vector4.Distance(vertex, vertex2) - dist) < 0.001f)
                {
                    log++;
                    edges.Add(new Edge(i, j));
                }
            }
            // log += "\n";
        }
        Debug.Log(log);
    }

    private List<Vector4> PlusMinusPermutations(Vector4 vertex, Parity parity = Parity.All)
    {
        // Get all the permutations of + and - for the 4 dimensions
        List<Vector4> vertices = new List<Vector4>();
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    for (int w = 0; w < 2; w++)
                    {
                        Vector4 vert = new Vector4(
                            vertex.x * (x == 0 ? 1 : -1),
                            vertex.y * (y == 0 ? 1 : -1),
                            vertex.z * (z == 0 ? 1 : -1),
                            vertex.w * (w == 0 ? 1 : -1)
                        ); 
                        vertices.AddRange(Permute(vert, parity));
                    }
                }
            }
        }
        
        // Remove duplicate vertices
        for (int i = 0; i < vertices.Count; i++)
        {
            for (int j = i + 1; j < vertices.Count; j++)
            {
                if (vertices[i] == vertices[j])
                {
                    vertices.RemoveAt(j);
                    j--;
                }
            }
        }

        return vertices;
    }

    private Vector4[] Permute(Vector4 vertex, Parity parity = Parity.All)
    {
        var list = new List<Vector4>();
        float[] nums = {vertex.x, vertex.y, vertex.z, vertex.w};
        return DoPermute(nums, list, parity).ToArray();
    }

    
    // Pieced together this answer from
    // https://stackoverflow.com/questions/756055/listing-all-permutations-of-a-string-integer
    List<Vector4> DoPermute(float[] nums, List<Vector4> list, Parity parity = Parity.All, int swaps = 0, int start = 0)
    {
        if (start == nums.Length - 1)
        {
            switch (parity)
            {
                case Parity.All:
                    // We have one of our possible n! solutions,
                    // add it to the list.
                    list.Add(new Vector4(nums[0], nums[1], nums[2], nums[3]));
                    break;
                case Parity.Even:
                    // Only add if the number of swaps is even
                    if (swaps % 2 == 0)
                    {
                        list.Add(new Vector4(nums[0], nums[1], nums[2], nums[3]));
                    }   
                    break;
            }
        }
        else
        {
            for (var i = start; i < nums.Length; i++)
            {
                Swap(ref nums[start], ref nums[i]);
                DoPermute(nums, list, parity, swaps, start + 1);
                swaps++;
                Swap(ref nums[start], ref nums[i]);
            }
        }
        return list;
    }

    void Swap(ref float a, ref float b)
    {
        (a, b) = (b, a);
    }
}
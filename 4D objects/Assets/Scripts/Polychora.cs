using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Mesh4D;

public static class Polychora
{
    public struct ShapeSeed
    {
        public Dictionary<Vector4, Parity> Seeds;
        public float EdgeLength;
        public bool UsePermutationSeed;

        public ShapeSeed(Dictionary<Vector4, Parity> seeds, float edgeLength, bool usePermutationSeed = true)
        {
            Seeds = seeds;
            EdgeLength = edgeLength;
            UsePermutationSeed = usePermutationSeed;
        }
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
    
    // All other shapes can be constructed from a permutation seed
    private static readonly Dictionary<Shapes, ShapeSeed> _shapes = new()
    {
        {
            Shapes.None, new(new()
            {
                { new(0, 0, 0, 0), Parity.All },
            }, 0)
        },
        {
            // This shape actually doesn't use permutations, therefore the parity is obsolete
            Shapes.Cell5, new(new()
            {
                { new(1, 1, 1, -1 / MathF.Sqrt(5)), Parity.All },
                { new(1, -1, -1, -1 / MathF.Sqrt(5)), Parity.All },
                { new(-1, 1, -1, -1 / MathF.Sqrt(5)), Parity.All },
                { new(-1, -1, 1, -1 / MathF.Sqrt(5)), Parity.All },
                { new(0, 0, 0, 4 / MathF.Sqrt(5)), Parity.All },
            }, 2 * Mathf.Sqrt(2), false)
        },
        {
            Shapes.Hypercube, new(new()
            {
                { new(1, 1, 1, 1), Parity.All },
            }, 2)
        },
        {
            Shapes.Cell16, new(new()
            {
                { new(1, 0, 0, 0), Parity.All },
            }, Mathf.Sqrt(2))
        },
        {
            Shapes.Cell24, new(new()
            {
                { new(1, 1, 0, 0), Parity.All },
            }, Mathf.Sqrt(2))
        },
        {
            Shapes.Cell120, new(new()
            {
                // All
                { new(2, 2, 0, 0), Parity.All },
                { new(Mathf.Sqrt(5), 1, 1, 1), Parity.All },
                { new(PHI, PHI, PHI, PHIn2), Parity.All },
                { new(PHI2, PHIn1, PHIn1, PHIn1), Parity.All },

                // Even
                { new(PHI2, PHIn2, 1, 0), Parity.Even },
                { new(Mathf.Sqrt(5), PHIn1, PHI, 0), Parity.Even },
                { new(2, 1, PHI, PHIn1), Parity.Even },
            }, 3 - Mathf.Sqrt(5))
        },
        {
        Shapes.Cell600, new(new Dictionary<Vector4, Parity>()
        {
            // All
            { new Vector4(2, 0, 0, 0), Parity.All },
            { new Vector4(1, 1, 1, 1), Parity.All },
        
            // Even
            { new Vector4(PHI, 1, PHIn1, 0), Parity.Even },
        }, 2/PHI)
        },
    };


    public static bool IsPolychoron(Shapes shape)
    {
        return _shapes.ContainsKey(shape);
    }

    // Get the vertices of a hypercube
    public static Vector4[] GetPolychoronVertices(Shapes shape)
    {
        List<Vector4> vertices = new();

        if (!_shapes[shape].UsePermutationSeed)
        {
            // Return the seeds themself
            foreach (KeyValuePair<Vector4, Parity> seed in _shapes[shape].Seeds)
            {
                vertices.Add(seed.Key);
            }

            return vertices.ToArray();
        }

        // Loop through the dictionary and add the vertices
        foreach (KeyValuePair<Vector4, Parity> seed in _shapes[shape].Seeds)
        {
            vertices.AddRange(PlusMinusPermutations(seed.Key, seed.Value));
        }

        return vertices.ToArray();
    }

    // Get the edges of a hypercube
    public static Edge[] GetPolychoronEdges(Vector4[] vertices, Shapes shape)
    {
        List<Edge> edges = new();

        // Loop through all the vertices
        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = vertices[i];
            for (var j = i + 1; j < vertices.Length; j++)
            {
                var vertex2 = vertices[j];

                // If the distance is equal to size, add the edge
                if (Mathf.Abs(Vector4.Distance(vertex, vertex2) - _shapes[shape].EdgeLength) < 0.0001f)
                {
                    edges.Add(new(i, j));
                }
            }
        }

        return edges.ToArray();
    }

    private static List<Vector4> PlusMinusPermutations(Vector4 vertex, Parity parity = Parity.All)
    { 
        // Get all the permutations of + and - for the 4 dimensions
        List<Vector4> vertices = new();
        
        // Get all permutations of a seed
        Vector4[] permutation = Permute(vertex, parity);

        // Then add all sign variations for each permutation
        foreach (Vector4 perm in permutation)
        {
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        for (int w = 0; w < 2; w++)
                        {
                            Vector4 signPermutation = new Vector4(
                                perm.x * (x == 0 ? 1 : -1), 
                                perm.y * (y == 0 ? 1 : -1), 
                                perm.z * (z == 0 ? 1 : -1), 
                                perm.w * (w == 0 ? 1 : -1));
                            vertices.Add(signPermutation);
                        }
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

    private static Vector4[] Permute(Vector4 vertex, Parity parity = Parity.All)
    {
        var list = new List<Vector4>();
        float[] nums = { vertex.x, vertex.y, vertex.z, vertex.w };
        // Sort the nums array
        Array.Sort(nums);
        return DoPermute(nums, list, parity).ToArray();
    }


    // Pieced together this answer from
    // https://stackoverflow.com/questions/756055/listing-all-permutations-of-a-string-integer
    private static List<Vector4> DoPermute(float[] nums, List<Vector4> list, Parity parity = Parity.All, int start = 0)
    {
        if (start == nums.Length - 1)
        {
            switch (parity)
            {
                case Parity.All:
                    // We have one of our possible n! solutions,
                    // add it to the list.
                    list.Add(new(nums[0], nums[1], nums[2], nums[3]));
                    break;
                case Parity.Even:
                    // Only add if the number of swaps is even
                    if (IsEvenPermutation(nums))
                        list.Add(new(nums[0], nums[1], nums[2], nums[3]));
                    break;
            }
        }
        else
        {
            for (var i = start; i < nums.Length; i++)
            {
                Swap(ref nums[start], ref nums[i]);
                DoPermute(nums, list, parity, start + 1);
                Swap(ref nums[start], ref nums[i]);
            }
        }

        return list;
    }

    private static void Swap(ref float a, ref float b)
    {
        (a, b) = (b, a);
    }
    
    public static bool IsEvenPermutation(float[] arr)
    {
        int count = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            for (int j = i + 1; j < arr.Length; j++)
            {
                if (arr[i] > arr[j])
                {
                    count++;
                }
            }
        }
        return count % 2 == 0;
    }
}
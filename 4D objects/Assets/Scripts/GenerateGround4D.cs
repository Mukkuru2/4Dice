using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateGround4D : MonoBehaviour
{
    public GameObject groundChunk;
    
    public GameObject[] chunks;
    private Transform4D[] chunkTransforms;
    
    [SerializeField] private float w = 0;

    public int size = 1;

    private bool reverse = false;

    // Start is called before the first frame update
    void Start()
    {
        groundChunk.GetComponent<Mesh4D>().Shape = Mesh4D.Shapes.Terrain;

        chunks = new GameObject[size * 4 - 4];
        chunkTransforms = new Transform4D[size * 4 - 4];

        // Create a size by size grid of ground chunks, every 2 x and z

        int n = 0;
        for (int x = -size/2; x <= size/2; x++)
        {
            for (int z = -size/2; z <= size/2; z++)
            {
                // Return if not outer edge
                if (x > -size/2 && x < size/2 && z > -size/2 && z < size/2) continue;
                
                chunks[n] = Instantiate(groundChunk, transform);
                chunkTransforms[n] = chunks[n].GetComponent<Transform4D>();
                chunks[n].transform.position = new Vector3(x * 2, 0, z * 2);
                chunks[n].GetComponent<Mesh4D>().Initialise();
                SetBoxCollider(chunks[n].GetComponent<BoxCollider>());
                n++;
            }
        }
    }

    public void Update()
    {
        SetChildrenW(w);
    }

    public void FixedUpdate()
    {
        w += reverse ? -0.01f : 0.01f;

        reverse = w switch
        {
            // When it reached 1, reverse. Same with -1
            >= 1 => true,
            <= -1 => false,
            _ => reverse
        };
    }

    private void SetBoxCollider(BoxCollider collider)
    {
        // Set box collider to average of the eight vertices where the y isnt 0
        Vector4[] vertices = collider.GetComponent<Mesh4D>().Vertices;
        Vector3 center = Vector3.zero;
        int n = 0;
        foreach (Vector4 vertex in vertices)
        {
            if (vertex.y != 0)
            {
                center += new Vector3(vertex.x, vertex.y, vertex.z);
                n++;
            }
        }
        
        center /= n;
        center.y /= 2;
        collider.center = center;
        collider.size = new Vector3(2, center.y * 2, 2);
    }

    private void SetChildrenW(float w)
    {
        foreach (Transform4D chunk in chunkTransforms)
        {
            chunk.Position = new Vector4(chunk.Position.x, chunk.Position.y, chunk.Position.z, w);
        }
    }
    
}
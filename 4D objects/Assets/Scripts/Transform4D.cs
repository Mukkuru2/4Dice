using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MIConvexHull;
using UnityEngine;

public class Transform4D : MonoBehaviour
{

    [Header("Mesh4D")]
    public Mesh4D Mesh;
    public Mesh Mesh3D;
    private Vector4[] Vertices;
    [Header("Transform")]
    public Vector4 Position;
    public Euler4 Rotation;
    public Vector4 Scale = new Vector4(1,1,1,1);
    private Matrix4x4 RotationMatrix;
    private Matrix4x4 RotationMatrixInverse;
    
    [Serializable]
    public struct Euler4
    {
        [Range(-180, +180)]
        public float XY; // Z (W)
        [Range(-180, +180)]
        public float YZ; // X (w)
        [Range(-180, +180)]
        public float XZ; // Y (W)
        [Range(-180, +180)]
        public float XW; // Y Z
        [Range(-180, +180)]
        public float YW; // X Z
        [Range(-180, +180)]
        public float ZW; // X Y
    }

    private void Start()
    {
        Mesh.Initialise();
        Vertices = new Vector4[Mesh.Vertices.Length];
        print(Mesh.Vertices.Length + " Should be equal to " + Vertices.Length);
        UpdateRotationMatrix();
        UpdateVertices();
        
        Mesh3D = GetComponent<MeshFilter>().mesh = Intersect();
    }

    private void Update()
    {
        UpdateRotationMatrix();
        UpdateVertices();
        
        Mesh3D = Intersect();
    }
    private void UpdateVertices ()
    {
        for (int i = 0; i < Mesh.Vertices.Length; i++)
        {
            Vertices[i] = Transform(Mesh.Vertices[i]);
        }
    }
    
    // Takes a 4D point and translate, rotate and scale it
    // according to this transform
    public Vector4 Transform (Vector4 v)
    {
        // Rotates around zero
        // v = RotationMatrix.Multiply(v);
    
        // Scales around zero
        v.x *= Scale.x;
        v.y *= Scale.y;
        v.z *= Scale.z;
        v.w *= Scale.w;
        // Translates
        v += Position;
        return v;
    }
    
    private Matrix4x4 UpdateRotationMatrix()
    {
        RotationMatrix = Matrix4x4.identity;

        RotateXY(RotationMatrix, Rotation.XY * Mathf.Deg2Rad);
        RotateYZ(RotationMatrix, Rotation.YZ * Mathf.Deg2Rad);
        RotateXZ(RotationMatrix, Rotation.XZ * Mathf.Deg2Rad);
        RotateXW(RotationMatrix, Rotation.XW * Mathf.Deg2Rad);
        RotateYW(RotationMatrix, Rotation.YW * Mathf.Deg2Rad);
        RotateZW(RotationMatrix, Rotation.ZW * Mathf.Deg2Rad);
        
        RotationMatrixInverse = RotationMatrix.inverse;
        return RotationMatrix;
    }
    
    public static Matrix4x4 RotateXY(float a)
    {
        float c = Mathf.Cos(a);
        float s = Mathf.Sin(a);
        Matrix4x4 m = new Matrix4x4();
        m.SetColumn(0, new Vector4(c, -s, 0, 0));
        m.SetColumn(1, new Vector4(s, c, 0, 0));
        m.SetColumn(2, new Vector4(0, 0, 1, 0));
        m.SetColumn(3, new Vector4(0, 0, 0, 1));
        return m;
    }
    public static Matrix4x4 RotateXY(Matrix4x4 m, float a)
    {
        return m * RotateXY(a);
    }
    public static Matrix4x4 RotateYZ(float a)
    {
        float c = Mathf.Cos(a);
        float s = Mathf.Sin(a);
        Matrix4x4 m = new Matrix4x4();
        m.SetColumn(0, new Vector4(1, 0, 0, 0));
        m.SetColumn(1, new Vector4(0, c, -s, 0));
        m.SetColumn(2, new Vector4(0, s, c, 0));
        m.SetColumn(3, new Vector4(0, 0, 0, 1));
        return m;
    }
    public static Matrix4x4 RotateYZ(Matrix4x4 m, float a)
    {
        return m * RotateYZ(a);
    }
    
    public static Matrix4x4 RotateXZ(float a)
    {
        float c = Mathf.Cos(a);
        float s = Mathf.Sin(a);
        Matrix4x4 m = new Matrix4x4();
        m.SetColumn(0, new Vector4(c, 0, s, 0));
        m.SetColumn(1, new Vector4(0, 1, 0, 0));
        m.SetColumn(2, new Vector4(-s, 0, c, 0));
        m.SetColumn(3, new Vector4(0, 0, 0, 1));
        return m;
    }
    public static Matrix4x4 RotateXZ(Matrix4x4 m, float a)
    {
        return m * RotateXZ(a);
    }
    
    public static Matrix4x4 RotateXW(float a)
    {
        float c = Mathf.Cos(a);
        float s = Mathf.Sin(a);
        Matrix4x4 m = new Matrix4x4();
        m.SetColumn(0, new Vector4(c, 0, 0, -s));
        m.SetColumn(1, new Vector4(0, 1, 0, 0));
        m.SetColumn(2, new Vector4(0, 0, 1, 0));
        m.SetColumn(3, new Vector4(s, 0, 0, c));
        return m;
    }
    
    public static Matrix4x4 RotateXW(Matrix4x4 m, float a)
    {
        return m * RotateXW(a);
    }
    
    public static Matrix4x4 RotateYW(float a)
    {
        float c = Mathf.Cos(a);
        float s = Mathf.Sin(a);
        Matrix4x4 m = new Matrix4x4();
        m.SetColumn(0, new Vector4(1, 0, 0, 0));
        m.SetColumn(1, new Vector4(0, c, 0, s));
        m.SetColumn(2, new Vector4(0, 0, 1, 0));
        m.SetColumn(3, new Vector4(0, -s, 0, c));
        return m;
    }
    public static Matrix4x4 RotateYW(Matrix4x4 m, float a)
    {
        return m * RotateYW(a);
    }
    
    public static Matrix4x4 RotateZW(float a)
    {
        float c = Mathf.Cos(a);
        float s = Mathf.Sin(a);
        Matrix4x4 m = new Matrix4x4();
        m.SetColumn(0, new Vector4(1, 0, 0, 0));
        m.SetColumn(1, new Vector4(0, 1, 0, 0));
        m.SetColumn(2, new Vector4(0, 0, c, s));
        m.SetColumn(3, new Vector4(0, 0, -s, c));
        return m;
    }
    public static Matrix4x4 RotateZW(Matrix4x4 m, float a)
    {
        return m * RotateZW(a);
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
    
    public Mesh Intersect ()
    {
        // Calculates the intersections
        List<Vector3> vertices = new List<Vector3>();
        foreach (Mesh4D.Edge edge in Mesh.Edges)
            Intersection
            (
                vertices,
                Vertices[edge.Index0],
                Vertices[edge.Index1]
            );

        // Not enough intersection points!
        if (vertices.Count < 3)
            return null;

        // Creates and returns the mesh
        return CreateMesh(vertices);
    }

    Mesh CreateMesh(List<Vector3> vertex4)
    {
        // Vertex <- Vector4
        DefaultVertex[] vertices = new DefaultVertex[vertex4.Count];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new DefaultVertex();
            vertices[i].Position = new double[] { vertex4[i].x, vertex4[i].y, vertex4[i].z };
        }

        // Creates the convex null
        var result = ConvexHull.Create(vertices).Result;

        // Mesh 3D
        
        Vector3[] vertices3 = new Vector3[result.Faces.Count() * 3];
        int[] triangles = new int[result.Faces.Count() * 3];
        
        int v = 0;
        foreach (var face in result.Faces)
        {
            vertices3[v] = new Vector3((float)face.Vertices[0].Position[0], (float)face.Vertices[0].Position[1], (float)face.Vertices[0].Position[2]);
            triangles[v] = v ++;
        
            vertices3[v] = new Vector3((float)face.Vertices[1].Position[0], (float)face.Vertices[1].Position[1], (float)face.Vertices[1].Position[2]);
            triangles[v] = v++;
        
            vertices3[v] = new Vector3((float)face.Vertices[2].Position[0], (float)face.Vertices[2].Position[1], (float)face.Vertices[2].Position[2]);
            triangles[v] = v++;
        }
        
        Mesh mesh = new Mesh();
        mesh.vertices = vertices3;
        mesh.triangles = triangles;
        
        mesh.RecalculateNormals();
    
        return mesh;
    }
}

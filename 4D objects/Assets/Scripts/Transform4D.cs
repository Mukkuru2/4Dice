using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MeshWireframeRenderer4D;

public class Transform4D : MonoBehaviour
{
    [Header("Mesh4D")] 
    public Mesh4D Mesh;

    public ProjectionModes projectionMode;
    public ProjectionModes ProjectionMode {
        get { return projectionMode; }
        set {
            projectionMode = value;
            // Disable the other renderer
            switch (value)
            {
                case ProjectionModes.CrossSection:
                    if (WFRenderer != null)
                        WFRenderer.enabled = false;
                    if (Renderer != null)
                        Renderer.enabled = true;
                    break;
                case ProjectionModes.Orthographic:
                    if (WFRenderer != null)
                        WFRenderer.enabled = true;
                    if (Renderer != null)
                        Renderer.enabled = false;
                    break;
            }
        }
    }
    
    private MeshRenderer4D Renderer;
    private MeshWireframeRenderer4D WFRenderer;
    private Vector4[] vertices;
    public Vector4[] Vertices => vertices;

    [Header("Transform")] 
    private Vector4 position;

    public float w;
    
    public Vector4 Position {
        get { return position; }
        set {
            position = value;
            transform.position = new Vector3(value.x, value.y, value.z);
        }
    }
    public Euler4 Rotation;
    public Vector4 Scale = new Vector4(1, 1, 1, 1);
    private Matrix4x4 RotationMatrix;
    private Matrix4x4 RotationMatrixInverse;
    
    
    private float lastSwitchTime;


    [Serializable]
    public struct Euler4
    {
        [Range(-180, +180)] public float XY; // Z (W)
        [Range(-180, +180)] public float YZ; // X (w)
        [Range(-180, +180)] public float XZ; // Y (W)
        [Range(-180, +180)] public float XW; // Y Z
        [Range(-180, +180)] public float YW; // X Z
        [Range(-180, +180)] public float ZW; // X Y
    }

    public enum ProjectionModes
    {
        CrossSection,
        Orthographic,
    }

    private void Start()
    {
        // Get the renderers
        Renderer = GetComponent<MeshRenderer4D>();
        WFRenderer = GetComponent<MeshWireframeRenderer4D>();
        
        Mesh.Initialise();
        vertices = new Vector4[Mesh.Vertices.Length];

        ProjectionMode = projectionMode;

        UpdateRotationMatrix();
        UpdateVertices();

        Rotation.YZ = 45;
        Rotation.ZW = 45;
    }

    private void FixedUpdate()
    {
        Rotation.YW += 0.5f;
        if (Rotation.YW > 180)
            Rotation.YW = -180;
        
        Rotation.XW += 0.8f;
        if (Rotation.XW > 180)
            Rotation.XW = -180;
        

        UpdateRotationMatrix();
        UpdateVertices();

        switch (ProjectionMode)
        {
            case ProjectionModes.CrossSection:
                // Raise error if no renderer
                if (Renderer == null)
                {
                    Debug.LogError("No MeshRenderer4D component found on this object");
                    return;
                }
                Renderer.Render();
                break;
            case ProjectionModes.Orthographic:
                // Raise error if no renderer
                if (WFRenderer == null)
                {
                    Debug.LogError("No MeshWireframeRenderer4D component found on this object");
                    return;
                }
                WFRenderer.Render();
                break;
        }
    }

    private void UpdateVertices()
    {
        for (int i = 0; i < Mesh.Vertices.Length; i++)
        {
            vertices[i] = Transform(Mesh.Vertices[i]);
        }
    }

    // Takes a 4D point and translate, rotate and scale it
    // according to this transform
    public Vector4 Transform(Vector4 v)
    {
        // Rotates around zero
        v = RotationMatrix * v;

        // Scales around zero
        v.x *= Scale.x;
        v.y *= Scale.y;
        v.z *= Scale.z;
        v.w *= Scale.w;
        // Translates

        var position = transform.position;
        this.position = new Vector4(position.x, position.y, position.z, w);
        
        v += this.position;
        return v;
    }

    private Matrix4x4 UpdateRotationMatrix()
    {
        RotationMatrix = Matrix4x4.identity;

        RotationMatrix *= RotateXY(Rotation.XY * Mathf.Deg2Rad);
        RotationMatrix *= RotateYZ(Rotation.YZ * Mathf.Deg2Rad);
        RotationMatrix *= RotateXZ(Rotation.XZ * Mathf.Deg2Rad);
        RotationMatrix *= RotateXW(Rotation.XW * Mathf.Deg2Rad);
        RotationMatrix *= RotateYW(Rotation.YW * Mathf.Deg2Rad);
        RotationMatrix *= RotateZW(Rotation.ZW * Mathf.Deg2Rad);

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
}
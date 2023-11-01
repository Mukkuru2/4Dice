using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MeshWireframeRenderer4D;

public class Transform4D : MonoBehaviour
{
    [Header("Mesh4D")] public Mesh4D Mesh;

    public ProjectionModes projectionMode;

    public ProjectionModes ProjectionMode
    {
        get => projectionMode;
        set
        {
            projectionMode = value;
            SwitchProjectionMode();
        }
    }

    public MeshRenderer4D Renderer;
    public MeshWireframeRenderer4D WFRenderer;
    private Vector4[] vertices;
    public Vector4[] Vertices => vertices;

    [Header("Transform")] 
    private Vector4 position;

    public Vector4 Position
    {
        get => position;
        set
        {
            position = value;
            transform.position = new Vector3(value.x, value.y, value.z);
        }
    }

    public Euler4 Rotation;
    public Vector4 Scale = new Vector4(1, 1, 1, 1);
    private Matrix4x4 RotationMatrix;
    private Matrix4x4 RotationMatrixInverse;


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

    // private void OnValidate()
    // {
    //     Initialise();
    //     UpdateRotationMatrix();
    //     UpdateVertices();
    //     RenderCrossSection();
    // }

    private void Start()
    {
        Initialise();
        UpdateRotationMatrix();
        UpdateVertices();
    }

    private void Initialise()
    {
        Mesh.Initialise();
        vertices = new Vector4[Mesh.Vertices.Length];

        Position = transform.position;

        ProjectionMode = projectionMode;
    }

    private void FixedUpdate()
    {
        // Rotation.YW += 0.5f;
        // if (Rotation.YW > 180)
        //     Rotation.YW = -180;
        //
        // Rotation.XW += 0.8f;
        // if (Rotation.XW > 180)
        //     Rotation.XW = -180;

        // w += 0.01f;
        // if (w > 1)
        //     w = -1;


        UpdateRotationMatrix();
        UpdateVertices();
    }

    private void Update()
    {
        RenderAll();
    }

    private void RenderAll()
    {
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

    // Only show the cross section mesh in the editor
    private void RenderCrossSection()
    {
        // Raise error if no renderer
        if (Renderer == null)
        {
            Debug.LogError("No MeshRenderer4D component found on this object");
            return;
        }
        
        if (ProjectionMode == ProjectionModes.CrossSection) Renderer.Render();
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

        // 3D handling is done by the base transform
        Vector4 pos = new Vector4(0, 0, 0, position.w);

        v += pos;
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

    private void SwitchProjectionMode()
    {
        switch (ProjectionMode)
        {
            case ProjectionModes.CrossSection:
                // Disable wfrenderer if it exists
                if (WFRenderer != null) WFRenderer.Disable();
                break;
            case ProjectionModes.Orthographic:
                // Enable wfrenderer if it exists
                if (WFRenderer != null) WFRenderer.Enable();
                if (GetComponent<Renderer>() != null) Renderer.Disable();
                break;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Physics4D : MonoBehaviour
{
    public Transform4D transform4D;
    public Vector4 velocity;
    public Vector4 acceleration;
    
    private Vector3 startPosition;

    [Header("Rotation")]
    public Transform4D.Euler4 angularVelocity;
    
    [Header("Params")]
    public float airResistance = 0.99f;
    public float angularAirResistance = 1f;
    
    public float bounceResistance = 0.95f;
    public float angularBounceResistance = 0.9f;
    
    public float gravity = 9.81f;

    public float angularVelocityModifier = 10f; 
    
    public float flickVelocity = 20f;
    public float flickAngularVelocity = 5f;

    private void Start()
    {
        transform4D = GetComponent<Transform4D>();
        
        startPosition = transform4D.Position;
        
        FlickRandom();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateVelocity();
        UpdatePosition();
        UpdateAngularVelocity();

        // Hardcoded floor
        if (IsOnFloor())
        {
            // Set velocity and angular velocity to 0 if it reaches a certain bound of velocity
            if (IsVelocityBelowThreshold())
            {
                ResetVelocityAndAngularVelocity();
                return;
            }
            
            BounceOffFloor();
            ApproximateDicePath();
        }
        
        BounceWalls();
        AirResistance();
        Gravity();
    }

    private void Gravity()
    {
        // Apply gravity
        velocity -= new Vector4(0, gravity * Time.fixedDeltaTime, 0, 0);
    }

    private void BounceWalls()
    {
        float wallX = 8;
        float wallZ = 8;

        if (transform4D.Position.x < -wallX || transform4D.Position.x > wallX)
        {
            float newVelocityX = -velocity.x;
            transform4D.Position = new Vector4(
                Mathf.Clamp(transform4D.Position.x, -wallX, wallX), 
                transform4D.Position.y, 
                transform4D.Position.z, 
                transform4D.Position.w);
            velocity.x = newVelocityX;
            BounceResistance();
        }

        if (transform4D.Position.z < -wallZ || transform4D.Position.z > wallZ)
        {
            float newVelocityZ = -velocity.z;
            transform4D.Position = new Vector4(
                transform4D.Position.x, 
                transform4D.Position.y, 
                Mathf.Clamp(transform4D.Position.z, -wallZ, wallZ), 
                transform4D.Position.w); 
            velocity.z = newVelocityZ;
            BounceResistance();
        }
    }

    private void BounceOffFloor()
    {
        transform4D.Position = new Vector4(transform4D.Position.x, 1, transform4D.Position.z, transform4D.Position.w);
        velocity = new Vector4(velocity.x, -velocity.y, velocity.z, velocity.w);
        BounceResistance();
    }

    private void ResetVelocityAndAngularVelocity()
    {
        velocity = Vector4.zero;
        acceleration = Vector4.zero;

        angularVelocity.XY = 0;
        angularVelocity.YZ = 0;
        angularVelocity.XZ = 0;
        angularVelocity.XW = 0;
        angularVelocity.YW = 0;
        angularVelocity.ZW = 0;
    }

    private bool IsVelocityBelowThreshold()
    {
        return Mathf.Abs(velocity.y) < 0.5f;
    }

    private bool IsOnFloor()
    {
        return transform4D.Position.y < 1;
    }

    private void UpdateAngularVelocity()
    {
        transform4D.Rotation.XY += angularVelocity.XY * Time.fixedDeltaTime * angularVelocityModifier;
        transform4D.Rotation.YZ += angularVelocity.YZ * Time.fixedDeltaTime * angularVelocityModifier;
        transform4D.Rotation.XZ += angularVelocity.XZ * Time.fixedDeltaTime * angularVelocityModifier;
        transform4D.Rotation.XW += angularVelocity.XW * Time.fixedDeltaTime * angularVelocityModifier;
        transform4D.Rotation.YW += angularVelocity.YW * Time.fixedDeltaTime * angularVelocityModifier;
        transform4D.Rotation.ZW += angularVelocity.ZW * Time.fixedDeltaTime * angularVelocityModifier;
    }

    private void UpdatePosition()
    {
        // Update position
        transform4D.Position += velocity * Time.fixedDeltaTime;
    }

    private void UpdateVelocity()
    {
        // Update velocity
        velocity += acceleration * Time.fixedDeltaTime;
    }

    private void ApproximateDicePath()
    {
        // Approximate a possible path a die could take when it hits the floor
        
        Vector3 v = new Vector3(velocity.x, velocity.y, velocity.z);
        
        float angle = Mathf.Atan2(v.z, v.x);
        angle += Random.Range(-Mathf.PI / 4, Mathf.PI / 4);

        float totalEnergy = -1;
        float kineticEnergyChange = 0;

        float energyRatio = (velocity.x * velocity.x + velocity.y * velocity.y + velocity.z * velocity.z) /
                            (angularVelocity.XY * angularVelocity.XY + angularVelocity.YZ * angularVelocity.YZ +
                             angularVelocity.XZ * angularVelocity.XZ + angularVelocity.XW * angularVelocity.XW +
                             angularVelocity.YW * angularVelocity.YW + angularVelocity.ZW * angularVelocity.ZW);
        
        print(energyRatio);
        
        while (totalEnergy < 0)
        {
            float magnitudeXZ = Random.Range(0.5f, 1.5f);
            float magnitureY = Random.Range(0.5f, 1.5f);

            velocity = new Vector4(Mathf.Cos(angle) * magnitudeXZ, velocity.y * magnitureY,
                Mathf.Sin(angle) * magnitudeXZ, velocity.w);

            // Find the total kinetic energy gained or lost with this change
            float kineticEnergy = velocity.x * velocity.x + velocity.y * velocity.y + velocity.z * velocity.z;
            float kineticEnergyOld = v.x * v.x + v.y * v.y + v.z * v.z;
            kineticEnergyChange = kineticEnergy - kineticEnergyOld;
            
            totalEnergy = Mathf.Abs(velocity.x) + Mathf.Abs(velocity.y) + Mathf.Abs(velocity.z) + Mathf.Abs(angularVelocity.XY) + Mathf.Abs(angularVelocity.YZ) + Mathf.Abs(angularVelocity.XZ) + Mathf.Abs(angularVelocity.XW) + Mathf.Abs(angularVelocity.YW) + Mathf.Abs(angularVelocity.ZW);
        }

        // Get 6 numbers that add up to 1
        float[] numbers = new float[6];
        
        float sum = 0;
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] = Random.Range(0, 1f);
            sum += numbers[i];
        }
        
        // Divide each number by the sum to get 6 numbers that add up to 1
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] /= sum;
        }
        
        // Multiply each number by the kinetic energy change to get the change in kinetic energy for each number
        for (int i = 0; i < numbers.Length; i++)
        {
            numbers[i] *= kineticEnergyChange;
        }
        
        // Set the new velocities
        angularVelocity.XY += numbers[0];
        angularVelocity.YZ += numbers[1];
        angularVelocity.XZ += numbers[2];
        angularVelocity.XW += numbers[3];
        angularVelocity.YW += numbers[4];
        angularVelocity.ZW += numbers[5];
    }
    

    public void FlickRandom()
    {
        // Set initial velocity
        
        float x = Random.Range(-flickVelocity, flickVelocity);
        float z = Random.Range(-flickVelocity, flickVelocity);
        // float w = Random.Range(-range, range);
        
        Flick(new Vector4(x, 0, z, 0));
        
        // Set random rotation
        angularVelocity.XY = Random.Range(-flickAngularVelocity, flickAngularVelocity);
        angularVelocity.YZ = Random.Range(-flickAngularVelocity, flickAngularVelocity);
        angularVelocity.XZ = Random.Range(-flickAngularVelocity, flickAngularVelocity);
        angularVelocity.XW = Random.Range(-flickAngularVelocity, flickAngularVelocity);
        angularVelocity.YW = Random.Range(-flickAngularVelocity, flickAngularVelocity);
        angularVelocity.ZW = Random.Range(-flickAngularVelocity, flickAngularVelocity);

    }

    private void Flick(Vector4 force)
    {
        velocity += force;
    }

    private void BounceResistance()
    {
        velocity *= bounceResistance;
        angularVelocity.XY *= angularBounceResistance;
        angularVelocity.YZ *= angularBounceResistance;
        angularVelocity.XZ *= angularBounceResistance;
        angularVelocity.XW *= angularBounceResistance;
        angularVelocity.YW *= angularBounceResistance;
        angularVelocity.ZW *= angularBounceResistance;
    }

    private void AirResistance()
    {
        velocity *= airResistance;
        angularVelocity.XY *= angularAirResistance;
        angularVelocity.YZ *= angularAirResistance;
        angularVelocity.XZ *= angularAirResistance;
        angularVelocity.XW *= angularAirResistance;
        angularVelocity.YW *= angularAirResistance;
        angularVelocity.ZW *= angularAirResistance;
    }

    public void ResetPosition()
    {
        transform4D.Position = startPosition;
        velocity = Vector4.zero;

        FlickRandom();
    }
}
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

    [Header("Rotation")]
    public Transform4D.Euler4 angularVelocity;
    
    [Header("Params")]
    public float airResistance = 0.999f;
    public float angularAirResistance = 1f;
    
    public float bounceResistance = 0.95f;
    public float angularBounceResistance = 0.9f;
    
    public float gravity = 9.81f;

    public float angularVelocityModifier = 20f; 
    
    public float flickVelocity = 5f;
    public float flickAngularVelocity = 5f;

    private void Start()
    {
        transform4D = GetComponent<Transform4D>();
        
        FlickRandom();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Update velocity
        velocity += acceleration * Time.fixedDeltaTime;
            
        // Update position
        transform4D.Position += velocity * Time.fixedDeltaTime;
            
        transform4D.Rotation.XY += angularVelocity.XY * Time.fixedDeltaTime * angularVelocityModifier;
        transform4D.Rotation.YZ += angularVelocity.YZ * Time.fixedDeltaTime * angularVelocityModifier;
        transform4D.Rotation.XZ += angularVelocity.XZ * Time.fixedDeltaTime * angularVelocityModifier;
        transform4D.Rotation.XW += angularVelocity.XW * Time.fixedDeltaTime * angularVelocityModifier;
        transform4D.Rotation.YW += angularVelocity.YW * Time.fixedDeltaTime * angularVelocityModifier;
        transform4D.Rotation.ZW += angularVelocity.ZW * Time.fixedDeltaTime * angularVelocityModifier;
            
        // Hardcoded floor
        if (transform4D.Position.y < 1)
        {
            
            // Set velocity and angular velocity to 0 if it reaches a certain bound of velocity
            if (Mathf.Abs(velocity.y) < 0.5f)
            {
                velocity = Vector4.zero;
                acceleration = Vector4.zero;
                
                angularVelocity.XY = 0;
                angularVelocity.YZ = 0;
                angularVelocity.XZ = 0;
                angularVelocity.XW = 0;
                angularVelocity.YW = 0;
                angularVelocity.ZW = 0;

                return;
            }
            
            transform4D.Position = new Vector4(transform4D.Position.x, 1, transform4D.Position.z, transform4D.Position.w);
            velocity = new Vector4(velocity.x, -velocity.y, velocity.z, velocity.w);
            
            velocity *= bounceResistance;
            angularVelocity.XY *= angularBounceResistance;
            angularVelocity.YZ *= angularBounceResistance;
            angularVelocity.XZ *= angularBounceResistance;
            angularVelocity.XW *= angularBounceResistance;
            angularVelocity.YW *= angularBounceResistance;
            angularVelocity.ZW *= angularBounceResistance;
            
            ShuffleVelocityAngularVelocity();
        }
            
        // multiply by air resistance every second
        velocity *= airResistance;
        
        // Same for angular velocity
        angularVelocity.XY *= angularAirResistance;
        angularVelocity.YZ *= angularAirResistance;
        angularVelocity.XZ *= angularAirResistance;
        angularVelocity.XW *= angularAirResistance;
        angularVelocity.YW *= angularAirResistance;
        angularVelocity.ZW *= angularAirResistance;
            
        // Apply gravity
        velocity -= new Vector4(0, gravity * Time.fixedDeltaTime, 0, 0);
    }

    private void ShuffleVelocityAngularVelocity()
    {
        float[] randomValues = new float[9];
        float sum = 0;
        
        for (int i = 0; i < 9; i++)
        {
            randomValues[i] = Random.Range(0f, 1f);
            sum += randomValues[i];
        }

        for (int i = 0; i < 9; i++)
        {
            randomValues[i] /= sum;
            randomValues[i] /= 2f;
        }
        
        float velocitySum = Mathf.Abs(velocity.x) + Mathf.Abs(velocity.y) + Mathf.Abs(velocity.z);
        float angularVelocitySum = Mathf.Abs(angularVelocity.XY) + Mathf.Abs(angularVelocity.XZ) + Mathf.Abs(angularVelocity.XW) + Mathf.Abs(angularVelocity.YZ) + Mathf.Abs(angularVelocity.YW) + Mathf.Abs(angularVelocity.ZW);
        float total = velocitySum + angularVelocitySum;
        
        velocity.x = Mathf.Sign(velocity.x) * total * randomValues[0] + velocity.x / 2;
        velocity.y = Mathf.Sign(velocity.y) * total * randomValues[1] + velocity.y / 2;
        velocity.z = Mathf.Sign(velocity.z) * total * randomValues[2] + velocity.z / 2;
        
        angularVelocity.XY = Mathf.Sign(angularVelocity.XY) * total * randomValues[3] + angularVelocity.XY / 2;
        angularVelocity.XZ = Mathf.Sign(angularVelocity.XZ) * total * randomValues[4] + angularVelocity.XZ / 2;
        angularVelocity.XW = Mathf.Sign(angularVelocity.XW) * total * randomValues[5] + angularVelocity.XW / 2;
        angularVelocity.YZ = Mathf.Sign(angularVelocity.YZ) * total * randomValues[6] + angularVelocity.YZ / 2;
        angularVelocity.YW = Mathf.Sign(angularVelocity.YW) * total * randomValues[7] + angularVelocity.YW / 2;
        angularVelocity.ZW = Mathf.Sign(angularVelocity.ZW) * total * randomValues[8] + angularVelocity.ZW / 2;

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
}
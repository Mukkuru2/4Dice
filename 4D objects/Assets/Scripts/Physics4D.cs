using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Physics4D : MonoBehaviour
{
    public Transform4D transform4D;
    public Rigidbody rigidbody;
    
    public Vector4 velocity;
    
    private Vector3 startPosition;

    [Header("Rotation")]
    public Transform4D.Euler4 angularVelocity;
    
    [Header("Params")]
    public float bounceResistance;
    public float angularVelocityModifier; 
    
    public float gravity = 9.81f;
    
    public float flickVelocity;
    public float flickAngularForce;

    private int nCollisions = 0;

    private void Start()
    {
        transform4D = GetComponent<Transform4D>();
        
        startPosition = transform.position;
        
        FlickRandom();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (rigidbody.isKinematic) return;
        UpdatePosition();
        UpdateAngularVelocity();
        
        Gravity();
    }
    
    private void Gravity()
    {
        // Apply gravity
        velocity -= new Vector4(0, gravity * Time.fixedDeltaTime, 0, 0);
    }
    
    private void UpdatePosition() {
        transform4D.Position += velocity * Time.fixedDeltaTime;
    }

    private float GetAngularVelocitySum()
    {
        return Mathf.Abs(angularVelocity.XY) + Mathf.Abs(angularVelocity.YZ) + Mathf.Abs(angularVelocity.XZ) +
               Mathf.Abs(angularVelocity.XW) + Mathf.Abs(angularVelocity.YW) + Mathf.Abs(angularVelocity.ZW);
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

    private void HitFloor(Vector3 pos)
    {
        // If the die has already bounced and y velocity is more than 0, return
        if (velocity.y > 0) return;
        
        BounceResistance();
        
        bool shouldStop = Mathf.Abs(velocity.y) < 0.4f;
        if (shouldStop)
        {
            rigidbody.isKinematic = true;
            return;
        }

        velocity = new Vector4(velocity.x, -velocity.y, velocity.z, velocity.w);
        SetMovementAngleBasedOnSpeed();

        BounceEmulation(pos);

    }

    private void BounceEmulation(Vector3 pos)
    {
        Vector3 diff = pos - transform.position;

        angularVelocity.XY = diff.x * -angularVelocityModifier * Mathf.Abs(velocity.y);
        angularVelocity.YZ = diff.z * angularVelocityModifier * Mathf.Abs(velocity.y);

        angularVelocity.XZ = diff.x * angularVelocityModifier * Mathf.Abs(velocity.y);
        angularVelocity.XW = diff.y * angularVelocityModifier * Mathf.Abs(velocity.y);
        angularVelocity.YW = diff.y * angularVelocityModifier * Mathf.Abs(velocity.y);
        angularVelocity.ZW = diff.z * -angularVelocityModifier * Mathf.Abs(velocity.y);
        
        
    }

    private void SetMovementAngleBasedOnSpeed()
    {
        Vector2 XZ = new Vector2(velocity.x, velocity.z);

        // When a collision happens the angle is adjusted to the speed. Lower speed, more random angle change.
        float angle = Mathf.Atan2(velocity.x, velocity.z);

        // The angle is adjusted by a random amount, depending on the velocity. Higher velocity, lower random angle.
        float randomAngle = angle + Random.Range(-2 / Mathf.Abs(velocity.magnitude), 2 / Mathf.Abs(velocity.magnitude));

        // The velocity is adjusted by the random angle
        velocity = new Vector4(
            Mathf.Sin(randomAngle) * XZ.magnitude,
            velocity.y,
            Mathf.Cos(randomAngle) * XZ.magnitude,
            velocity.w);
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
    
    private void BounceResistance()
    {
        float n = 1000;
        float mod = 40;
        velocity *= 1 - Mathf.Pow(n, -Mathf.Abs(velocity.y / mod));
    }
    
    
    public void FlickRandom()
    {
        // Set initial velocity
        
        float x = Random.Range(-flickVelocity, flickVelocity);
        float z = Random.Range(-flickVelocity, flickVelocity);
        
        velocity = new Vector4(x, 0, z, 0);
        
        // Set random rotation
        angularVelocity.XY = Random.Range(-flickAngularForce, flickAngularForce);
        angularVelocity.YZ = Random.Range(-flickAngularForce, flickAngularForce);
        angularVelocity.XZ = Random.Range(-flickAngularForce, flickAngularForce);
        angularVelocity.XW = Random.Range(-flickAngularForce, flickAngularForce);
        angularVelocity.YW = Random.Range(-flickAngularForce, flickAngularForce);
        angularVelocity.ZW = Random.Range(-flickAngularForce, flickAngularForce);
    }
    
    private void ResetVelocityAndAngularVelocity()
    {
        velocity = Vector4.zero;

        angularVelocity.XY = 0;
        angularVelocity.YZ = 0;
        angularVelocity.XZ = 0;
        angularVelocity.XW = 0;
        angularVelocity.YW = 0;
        angularVelocity.ZW = 0;
    }
    
    public void ResetPosition()
    {
        ResetVelocityAndAngularVelocity();
        
        transform4D.Position = startPosition;
        rigidbody.isKinematic = false;
        
        transform4D.Rotation.XY = 0;
        transform4D.Rotation.YZ = 0;
        transform4D.Rotation.XZ = 0;
        transform4D.Rotation.XW = 0;
        transform4D.Rotation.YW = 0;
        transform4D.Rotation.ZW = 0;

        FlickRandom();
    }

    private void OnCollisionEnter(Collision other)
    {
        nCollisions++;
    }

    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Floor") && !rigidbody.isKinematic)
        {
            HitFloor(other.GetContact(0).point);
            return;
        }
        BounceWalls();
    }
    
    void OnCollisionExit(Collision other)
    {
        nCollisions--;
    }
    
}
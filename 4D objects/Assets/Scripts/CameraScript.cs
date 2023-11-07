using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform4D transform4D;
    public new Camera camera;

    private readonly Queue _heightQueue = new();

    // Update is called once per frame
    void Update()
    {
        _heightQueue.Enqueue(transform4D.Position.y);
        if (_heightQueue.Count > 100) _heightQueue.Dequeue();
        
        // Get average of last 10 heights
        float avg = 0;
        foreach (float height in _heightQueue)
        {
            avg += height;
        }
        
        avg /= _heightQueue.Count;
        
        
        transform.position = new Vector3(transform4D.Position.x / 2, avg * 5, transform4D.Position.z / 2);
        transform.LookAt(transform4D.Position);
    }

    public void Reset()
    {
        _heightQueue.Clear();
    }
}

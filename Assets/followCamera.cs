using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class followCamera : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Vector3 offset;

    void FixedUpdate()
    {
        Vector3 playerLocation = rb.position;
        Vector3 newLocation = new Vector3(playerLocation.x + offset.x, playerLocation.y + offset.y, playerLocation.z + offset.z);
        this.transform.position = newLocation;
    }

    public float GetZ()
    {
        return offset.z;
    }
}

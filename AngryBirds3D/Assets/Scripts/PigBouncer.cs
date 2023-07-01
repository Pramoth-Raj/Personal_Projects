using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PigBouncer : MonoBehaviour
{
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.up * 20;
    }
}

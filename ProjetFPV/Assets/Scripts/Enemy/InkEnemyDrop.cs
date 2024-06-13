using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkEnemyDrop : MonoBehaviour
{
    private Rigidbody rb;

    public float rotationAmplitude;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.angularVelocity = new Vector3(0,rotationAmplitude,0);
    }
}

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
        rb.angularVelocity = new Vector3(Mathf.Cos(Time.time),Mathf.Cos(Time.time),Mathf.Sin(Time.time) * rotationAmplitude);
    }
}

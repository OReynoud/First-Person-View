using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ControllableProp : MonoBehaviour
{
    public bool isGrabbed;

    public Rigidbody body;

    // Start is called before the first frame update
    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
        //body.velocity = -Vector3.forward * speed;
    }

    public virtual void ApplyTelekinesis()
    {
        body.useGravity = !body.useGravity;
    }
}

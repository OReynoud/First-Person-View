using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ControllableProp : MonoBehaviour
{
    public bool isGrabbed;

    public Rigidbody body;

    public float spamBuffer = 0.6f;

    public bool canBeGrabbed = true;

    // Start is called before the first frame update
    public virtual void Awake()
    {
        body = GetComponent<Rigidbody>();
        canBeGrabbed = true;
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

    public IEnumerator BufferGrabbing()
    {
        canBeGrabbed = false;
        yield return new WaitForSeconds(spamBuffer);
        canBeGrabbed = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ControllableProp : MonoBehaviour
{
    [BoxGroup("Controllable Prop")] public bool isGrabbed;

    [BoxGroup("Controllable Prop")] public Rigidbody body;

    [BoxGroup("Controllable Prop")] public float spamBuffer = 0.6f;

    [BoxGroup("Controllable Prop")] public bool canBeGrabbed = true;

    // Start is called before the first frame update
    public virtual void Awake()
    {
        body = GetComponent<Rigidbody>();
        canBeGrabbed = true;
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

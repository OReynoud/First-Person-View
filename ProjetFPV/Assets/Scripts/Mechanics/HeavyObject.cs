using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyObject : ControllableProp
{
    public float restingDistanceToPlayer;
    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
    }
    public override void ApplyTelekinesis() 
    {
        body.useGravity = !body.useGravity;

        if (gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            gameObject.layer = LayerMask.NameToLayer("Telekinesis");
            body.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }
}

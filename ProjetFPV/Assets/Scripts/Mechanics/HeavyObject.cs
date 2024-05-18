using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeavyObject : ControllableProp
{
    public float restingDistanceToPlayer = 5;
    // Start is called before the first frame update

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position + transform.forward * restingDistanceToPlayer, 0.1f);
    }

    public override void Awake()
    {
        base.Awake();
        body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
    }
    public override void ApplyTelekinesis() 
    {
        body.useGravity = !body.useGravity;

        if (!body.useGravity)
        {
            body.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using AmplifyShaderEditor;
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

    private void OnCollisionEnter(Collision other)
    {
        if (body.velocity.y > 1 && !isGrabbed)
        {
            AudioManager.instance.PlaySound(1, 1, gameObject, 0.1f, false);
            //SON
        }
    }
}

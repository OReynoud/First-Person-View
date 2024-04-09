using System;
using System.Collections;
using System.Collections.Generic;
using Mechanics;
using UnityEngine;

public class TelekinesisObject : ControllableProp
{
    public bool thrown;
    [SerializeField]private Collider col;

    public override void Awake()
    {
        base.Awake();
        col = GetComponent<Collider>();
    }

    // Update is called once per frame
    public override void ApplyTelekinesis() 
    {
        body.useGravity = !body.useGravity;

        if (gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            gameObject.layer = LayerMask.NameToLayer("Telekinesis");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            thrown = true;
        }
    }

    private void FixedUpdate()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!thrown)return;
        
        if (body.velocity.magnitude < 10)
        {
            thrown = false;
            return;
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (other.gameObject.TryGetComponent(out Enemy enemy))
            {
                Debug.Log("Stunned an enemy");
                enemy.ApplyStun();
                thrown = false;
            }
        }
    }
}

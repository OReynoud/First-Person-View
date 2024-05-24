using System;
using System.Collections;
using System.Collections.Generic;
using Mechanics;
using Unity.VisualScripting;
using UnityEngine;

public class TelekinesisObject : ControllableProp
{
    public float velocityLimit = 10;
    public bool thrown;
    public float ignoreGravityTime = 3;
    [SerializeField]private Collider col;

    public override void Awake()
    {
        base.Awake();
        col = GetComponent<Collider>();
    }

    private float timer;

    // Update is called once per frame
    public override void ApplyTelekinesis() 
    {
        body.useGravity = !body.useGravity;

        if (gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            timer = 0;
            thrown = false;
            gameObject.layer = LayerMask.NameToLayer("Telekinesis");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            if (PlayerController.instance.recentlyDepletedStamina)
            {
                thrown = false;
                return;
            }
            thrown = PlayerController.instance.playerCam.forward.y > -PlayerController.instance.tkManager.holdObjectYTolerance;
            
        }
    }

    private void FixedUpdate()
    {
        if (thrown)
        {
            timer += Time.deltaTime;
            body.useGravity = timer > ignoreGravityTime;
        }
        else
        {
            body.useGravity = true;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!thrown)return;
        
        if (body.velocity.magnitude < velocityLimit)
        {
            StartCoroutine(NotThrown());
            return;
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            var enemy = other.collider.GetComponentInParent<Enemy>();
                Debug.Log("Stunned an enemy");
                enemy.ApplyStun();
                
                StartCoroutine(NotThrown());
            
        }
        
        //SON
    }

    IEnumerator NotThrown()
    {
        yield return new WaitForSeconds(0.2f);
        thrown = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelekinesisObject : ControllableProp
{
    // Start is called before the first frame update
    void Start()
    {
        
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
        }
    }
}

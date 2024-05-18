using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEnnemi : MonoBehaviour
{
    public Transform actor;

    public Transform destination;

    public float speed;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggered = true;
        }
    }

    private void Update()
    {
        if (triggered)
        {
            actor.position = Vector3.MoveTowards(actor.position, destination.position, speed);
        }

        if (actor.position == destination.position)
        {
            Destroy(transform.parent.gameObject);
        }
    }
}

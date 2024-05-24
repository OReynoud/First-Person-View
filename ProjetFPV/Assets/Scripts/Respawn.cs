using System;
using System.Collections;
using System.Collections.Generic;
using Mechanics;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    public Transform respawnpoint;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController.instance.transform.position = respawnpoint.position;
        }
    }
}

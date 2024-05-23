using System;
using System.Collections;
using System.Collections.Generic;
using Mechanics;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform respawnPoint;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //GameManager.instance.actualRespawnPoint = respawnPoint;
        }
    }
}

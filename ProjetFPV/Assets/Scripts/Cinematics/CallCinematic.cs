using System;
using Unity.VisualScripting;
using UnityEngine;

public class CallCinematic : MonoBehaviour
{
    [SerializeField] private int cinematicToCall;
    
    private void OnTriggerEnter(Collider other)
    {
        switch (cinematicToCall)
        {
            case 1:
                CinematicManager.instance.StartCinematic();
                Destroy(gameObject);
                break;
        }
    }
}

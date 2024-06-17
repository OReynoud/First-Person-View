using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEnnemi : MonoBehaviour
{
    public Transform actor;

    public Transform destination;
    public GameObject meshParent;

    public float speed;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.instance.PlaySound(5, 1, actor.gameObject, 0.5f, false);
            triggered = true;
        }
    }

    private void Update()
    {
        if (triggered)
        {
            actor.position = Vector3.MoveTowards(actor.position, destination.position, speed);
        }

        if (Vector3.Distance(actor.position, destination.position) <= 0.3f)
        {
            StartCoroutine(DisableAndDestroy());
        }
    }

    private IEnumerator DisableAndDestroy()
    {
        meshParent.SetActive(false);
        Debug.Log("ZERAEZAEAZEAZEAZEAZEAZEAZEAZE");
        yield return new WaitForSeconds(5f);
        Destroy(transform.parent.gameObject);
    }
}

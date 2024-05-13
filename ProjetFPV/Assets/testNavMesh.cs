using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class testNavMesh : MonoBehaviour
{
    public NavMeshAgent agent;

    public GameObject destination;
    // Start is called before the first frame update
    void Start()
    {
        agent.SetDestination(destination.transform.position);

        Debug.Log(agent.destination);
        foreach (var corner in agent.path.corners)
        {
            Debug.Log(corner);
            Instantiate(destination,corner, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mechanics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ChargerBehavior : MonoBehaviour
{
    private Enemy brain;
    private NavMeshAgent agent;
    [SerializeField] private float pathUpdateFrequency;

    
    
    [SerializeField] private float walkAreaRange;
    [SerializeField] private Transform[] spawnPositions;
    [SerializeField] private float rushRange;
    [SerializeField] private float rushSpeed;
    [SerializeField] private float atkRange;
    [SerializeField] private float atkDamage;
    [SerializeField] private int maskCount;
    [SerializeField] private float stunDurationTK;
    public enum States
    {
        Neutral,
        Repositioning,
        Rush,
        Attack,
        Stunned,
        Paralysed
    }

    public States currentState;

    public void ApplyTK()
    {
        if (currentState != States.Paralysed)
        {
            currentState = States.Paralysed;
        }
        else
        {
            currentState = States.Repositioning;
        }
    }
    // Start is called before the first frame update
    private void Awake()
    {
        playerLayer = LayerMask.GetMask("Player");
    }

    void Start()
    {
        agent.stoppingDistance = atkDamage;
        
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case States.Neutral:
                if (Vector3.Distance(PlayerController.instance.transform.position, transform.position) < rushRange)
                {
                    TryDetectPlayer();
                }
                break;
            case States.Repositioning:
                break;
            case States.Rush:
                if (Vector3.Distance(PlayerController.instance.transform.position, transform.position) < atkRange)
                {
                    StartCoroutine(AttackPlayer());
                }
                break;
            case States.Attack:
                break;
            case States.Stunned:
                break;
            case States.Paralysed:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }




    }

    private LayerMask playerLayer;
    void TryDetectPlayer()
    {
        if (!Physics.Linecast(transform.position, PlayerController.instance.transform.position)) return;
        
        currentState = States.Repositioning;
        SetAgentDestination();
    }
    async void SetAgentDestination()
    {
        
        if (!Application.isPlaying)return;
        
        await Task.Delay(Mathf.RoundToInt(pathUpdateFrequency * 1000));
        while (currentState != States.Rush)
        {
            await Task.Delay(100);
        }
            
        
        agent.SetDestination(PlayerController.instance.transform.position);
        
        SetAgentDestination();
    }

    
    
    IEnumerator AttackPlayer()
    {
        currentState = States.Attack;
        Debug.Log("j'attaque le joueur");
        yield return new WaitForSeconds(0.5f);
        var colliders = Physics.OverlapSphere(transform.position + transform.forward, 1f, playerLayer);
        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out PlayerController player))
            {
                Debug.Log("J'ai touché le joueur");
                player.TakeDamage(atkDamage);
            }
        }
        yield return new WaitForSeconds(1.5f);
        currentState = States.Repositioning;

    }

    Vector3 GetRandomSpawnPoint()
    {
        var random = Random.Range(0, spawnPositions.Length);
        return spawnPositions[random].position;
    }
}

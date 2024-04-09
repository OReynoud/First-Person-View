using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Mechanics;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ChargerBehavior : MonoBehaviour
{
    private Enemy brain;
    private NavMeshAgent agent;
    [InfoBox("Cercle vert = Zone de patrouille;  Cercle rouge = Zone d'aggro;  Cercle violet = portée de l'attaque")]
    [SerializeField] private float pathUpdateFrequency;

    [SerializeField] private float stunDuration = 2f;
    private float stunTimer;

    [Foldout("Reposition state")] [SerializeField]
    private float disappearDuration = 0.5f;

    [Foldout("Reposition state")] [SerializeField]
    private float idleDuration = 1f;

    [Foldout("Reposition state")] [SerializeField]
    private float appearDuration = 0.5f;

    [Foldout("Reposition state")] [SerializeField]
    private Transform[] spawnPositions;

    [Foldout("Neutral state")] [SerializeField]
    private float walkAreaRange;

    [Foldout("Neutral state")] [SerializeField]
    private float wanderFrequency;

    [Foldout("Neutral state")] [SerializeField]
    private float wanderSpeed;

    [Foldout("Rush state")] [SerializeField]
    private float rushRange;

    [Foldout("Rush state")] [SerializeField]
    private float rushSpeed;

    [Foldout("Attack state")] [SerializeField]
    private float atkRange;

    [Foldout("Attack state")] [SerializeField]
    private float atkDamage;

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
    private bool repositioning;



    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.green;
        if (Application.isPlaying)
        {
            Handles.DrawWireDisc(origin, Vector3.up, walkAreaRange,2);
        }
        else
        {
            Handles.DrawWireDisc(transform.position, Vector3.up, walkAreaRange,2);
        }

        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.up, rushRange,2);
        
        Handles.color = Color.magenta;
        Handles.DrawWireDisc(transform.position, Vector3.up, atkRange,2);
    }

    public void ApplyTK()
    {
        if (currentState != States.Paralysed)
        {
            currentState = States.Paralysed;
        }
        else
        {
            currentState = States.Rush;

        }

        agent.enabled = currentState != States.Paralysed;


        if (agent.enabled)
            agent.SetDestination(PlayerController.instance.transform.position);
    }

    public void ApplyStun()
    {
        currentState = States.Stunned;
    }

    IEnumerator Stun()
    {
        
    }

    // Start is called before the first frame update
    private Vector3 origin;

    private void Awake()
    {
        origin = transform.position;
        agent = GetComponent<NavMeshAgent>();
        brain = GetComponent<Enemy>();
    }

    IEnumerator Wander()
    {
        var oui = Random.insideUnitCircle * walkAreaRange;
        agent.SetDestination(origin + new Vector3(oui.x, 0, oui.y));
        yield return new WaitForSeconds(wanderFrequency);
        if (currentState == States.Neutral)
        {
            StartCoroutine(Wander());
        }
    }

    void Start()
    {
        agent.stoppingDistance = atkRange;
        agent.speed = wanderSpeed;
        currentState = States.Neutral;
        StartCoroutine(Wander());
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case States.Neutral:

                TryDetectPlayer();
                break;
            case States.Repositioning:
                if (repositioning) break;
                StartCoroutine(Reposition());

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

    private IEnumerator Reposition()
    {
        agent.enabled = false;
        repositioning = true;
        foreach (var part in brain.bodyParts)
        {
            part.bodyPartCollider.enabled = false;
        }

        brain.body.isKinematic = true;
        transform.DOMove(transform.position + Vector3.down * 5, disappearDuration);

        yield return new WaitForSeconds(disappearDuration + idleDuration);
        transform.position = GetRandomSpawnPoint() + Vector3.down * 5;
        transform.DOMove(transform.position + Vector3.up * 5, appearDuration).OnComplete(() =>
        {
            repositioning = false;
            currentState = States.Rush;
            foreach (var part in brain.bodyParts)
            {
                part.bodyPartCollider.enabled = true;
            }

            brain.body.isKinematic = false;
            agent.enabled = true;
        });
    }

    public LayerMask playerLayer;

    void TryDetectPlayer()
    {
        var dir = PlayerController.instance.transform.position - transform.position;
        Debug.DrawRay(transform.position, dir.normalized * 5, Color.magenta);
        if (!Physics.Raycast(transform.position, dir.normalized, out RaycastHit hit,rushRange, playerLayer)) return;
        if (!hit.collider.gameObject.CompareTag("Player"))return;
        
        Debug.Log("Player Detected");
        currentState = States.Repositioning;
        agent.speed = rushSpeed;
        SetAgentDestination();
    }

    async void SetAgentDestination()
    {
        if (!Application.isPlaying) return;

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
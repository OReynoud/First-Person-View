using System;
using System.Collections;
using DG.Tweening;
using Mechanics;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CollectorBehavior : Enemy
{    
    [HorizontalLine(color: EColor.Red)]
    [InfoBox("Collector Behavior")]
    [InfoBox("Cercle vert = Zone de patrouille;  Cercle rouge = Zone d'aggro", EInfoBoxType.Warning) ]
    [BoxGroup] public NavMeshAgent agent;
    [BoxGroup] public CollectorBullet bulletPrefab;
    [BoxGroup] public ChargerBehavior bouffonPrefab;
    [BoxGroup] public Animation transitionState;
    [BoxGroup] public float speedTransition;
    
    [SerializeField]
    //private float pathUpdateFrequency;
    public enum States
    {
        Roam,
        Repositioning,
        Shoot,
        Stunned,
        Paralysed
    }
    [Foldout("Roam State")] [SerializeField]
    private float flyAreaRange;
    
    [Foldout("Roam State")] [SerializeField]
    private float aggroRange;
    
    [Foldout("Roam State")] [SerializeField]
    private float flySpeed;
    
    [Foldout("Roam State")] [SerializeField]
    private float aggressiveFlySpeed;
    
    [Foldout("Shoot State")] [SerializeField]
    private int numberOfBullets;

    [Foldout("Shoot State")] [SerializeField]
    private float bulletSpeed;
    
    [Foldout("Shoot State")] [SerializeField]
    private float timeBetweenBullets;

    [Foldout("Shoot State")] [SerializeField]
    private Transform[] bulletSpawnPos;
    
    [Foldout("Shoot State")] [SerializeField]
    private float weaknessTime;
    
    [Foldout("Spawn State")] [SerializeField]
    private int numberOfSpawn;
    
    [Foldout("Spawn State")] [SerializeField]
    private Transform[] spawnEnemyPos;
    
    [Foldout("Spawn State")] [SerializeField]
    private float spawnBulletSpeed;
    
    private AnimationCurve transitionCurve = new AnimationCurve();
    [Foldout("Debug")] [SerializeField] private bool repositioning;
    [Foldout("Debug")] [SerializeField] private float timer;
    [Foldout("Debug")] public States currentState;
    [Foldout("Debug")] [SerializeField] private float bulletTimer;
    [Foldout("Debug")] [SerializeField] private float bulletsLeft;
    [Foldout("Debug")] [SerializeField] private bool spawning;
    [Foldout("Debug")] [SerializeField] private bool seenPlayer;
    [Foldout("Debug")] [SerializeField] private bool facingPlayer;
    [Foldout("Debug")] [SerializeField] private float weaknessTimer;
    private Vector3 origin;
    
    
    private void OnDrawGizmosSelected()
    {
        Handles.color = Color.green;
        if (Application.isPlaying)
        {
            Handles.DrawWireDisc(origin, Vector3.up, flyAreaRange, 2);
        }
        else
        {
            Handles.DrawWireDisc(transform.position, Vector3.up, flyAreaRange, 2);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position,aggroRange);
    }
    // Update is called once per frame
    public override void Awake()
    {
        base.Awake();
        transitionCurve.AddKey(0, 0);
        transitionCurve.AddKey(speedTransition, 1);
        origin = transform.position;
    }
    public override void Start()
    {
        base.Start();
        bulletsLeft = numberOfBullets;
        SetAnimationTimer(true);
        currentState = States.Roam;
        agent.enabled = true;
        agent.speed = flySpeed;
        seenPlayer = false;
    }

    public LayerMask playerLayer;
    public float rotationSpeed;

    void Update()
    {
        if (seenPlayer)
        {
            agent.speed = aggressiveFlySpeed;
        }
        
        switch (currentState)
        {
            case States.Roam:
                AnimateMasks(true);
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        repositioning = false;
                        var dir = PlayerController.instance.transform.position - transform.position;
                        Debug.DrawRay(transform.position, dir.normalized * 5, Color.magenta);
                        if (Physics.Raycast(transform.position, dir.normalized, out RaycastHit hit, aggroRange,
                                playerLayer))
                        {
                            if (hit.collider.gameObject.CompareTag("Player"))
                            {
                                agent.enabled = false;
                                currentState = States.Shoot;
                                Debug.Log("Player Detected");
                            }
                        }

                    }
                }
                if (repositioning) break;
                repositioning = true;
                agent.SetDestination(origin + new Vector3(Random.Range(-1f,1f) * flyAreaRange, 0,
                    Random.Range(-1f,1f) * flyAreaRange));
                break;
            case States.Repositioning:
                AnimateMasks(true);
                break;
            case States.Shoot:
                AnimateMasks(false);
                RotateModel();
                if (facingPlayer)
                {
                    ShootMethod();
                }
                break;
            case States.Stunned:
                break;
            case States.Paralysed:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void RotateModel()
    {
        var dir = PlayerController.instance.transform.position - transform.position;
        dir.Normalize();
        dir = new Vector3(dir.x,0,dir.z);
        //var angle = Mathf.Atan2(dir.z, dir.x);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), rotationSpeed);
        if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(dir)) < 10)
        {
            facingPlayer = true;
        }
        else
        {
            facingPlayer = false;
        }

    }
    private void ShootMethod()
    {
        if (bulletsLeft <= 0)
        {
            weaknessTimer += Time.deltaTime;
            if (weaknessTimer > weaknessTime)
            {
                weaknessTimer = 0;
                agent.enabled = true;
                currentState = States.Roam;
                bulletsLeft = numberOfBullets;
                agent.SetDestination(origin + new Vector3(Random.Range(-1f,1f) * flyAreaRange, 0,
                    Random.Range(-1f,1f) * flyAreaRange));
            }
            return;

        }
        if (bulletTimer < timeBetweenBullets)
        {
            bulletTimer += Time.deltaTime;
            return;
        }

        bulletsLeft--;
        bulletTimer = 0;
        var rand = Random.Range(0, bulletSpawnPos.Length);
        var dir = PlayerController.instance.transform.position - bulletSpawnPos[rand].position;
        var bullet = Instantiate(bulletPrefab, bulletSpawnPos[rand].position, Quaternion.LookRotation(dir.normalized));
        bullet.rb.velocity = dir * bulletSpeed;
    }

    void SetAnimationTimer(bool isRetract)
    {
        if (isRetract)
        {
            timer = speedTransition;
        }
        else
        {
            timer = 0;
        }
    }

    void AnimateMasks(bool isRetract)
    {
        if (!isRetract)
        {
            if (timer < speedTransition)
            {
                timer += Time.deltaTime;
            }
        }
        else
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
        }
        transitionState.clip.SampleAnimation(gameObject,transitionCurve.Evaluate(timer));
    }
    
    public override void ApplyStun()
    {
        if (currentState != States.Stunned)
        {
            currentState = States.Stunned;
            StartCoroutine(Stun());
        }
    }

    IEnumerator Stun()
    {
        foreach (var part in allMasks)
        {
            if (!part.broken)
            {
                part.meshRenderer.material = stunnedMat;
            }
        }
        agent.enabled = false;
        transform.DOShakeScale(0.2f, Vector3.one * 0.2f);
        yield return new WaitForSeconds(stunDuration);

        agent.enabled = true;            
        foreach (var part in allMasks)
        {
            if (!part.broken)
            {
                part.meshRenderer.material = defaultMat;
            }
        }
        agent.SetDestination(PlayerController.instance.transform.position);
        currentState = States.Repositioning;

    }
}

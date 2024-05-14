using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [InfoBox("Cercle vert = Zone de patrouille;  Cercle rouge = Zone d'aggro", EInfoBoxType.Warning)]
    [BoxGroup]
    public CollectorBullet bulletPrefab;

    [BoxGroup] public ChargerEgg spawnBulletPrefab;
    [BoxGroup] public Animation transitionState;
    [BoxGroup] public float transitionTime;


    public enum States
    {
        Roam,
        Repositioning,
        Shoot,
        Stunned,
        Paralysed,
        KnockBack
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
    public Transform[] spawnEnemyPos;

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

    [Foldout("Debug")] [SerializeField] public List<ChargerBehavior> children = new List<ChargerBehavior>();
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
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }

    // Update is called once per frame
    public override void Awake()
    {
        base.Awake();
        transitionCurve.AddKey(0, 0);
        transitionCurve.AddKey(transitionTime, 1);
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
                    if (repositioning && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f))
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
                                return;
                            }
                        }
                    }
                }

                if (repositioning) break;
                repositioning = true;
                agent.SetDestination(origin + new Vector3(Random.Range(-1f, 1f) * flyAreaRange, 0,
                    Random.Range(-1f, 1f) * flyAreaRange));
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
                AnimateMasks(false);
                break;
            case States.KnockBack:
                if (!knockedBack) currentState = States.Roam;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    

    #region Shoot

    void RotateModel()
    {
        var dir = PlayerController.instance.transform.position - transform.position;
        dir.Normalize();
        dir = new Vector3(dir.x, 0, dir.z);
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
                agent.SetDestination(origin + new Vector3(Random.Range(-1f, 1f) * flyAreaRange, 0,
                    Random.Range(-1f, 1f) * flyAreaRange));

                TrySpawnChargers();
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


    private List<Transform> validSpawns = new List<Transform>();
    private int spawnedEnemies = 0;
    private ChargerBehavior spawnedEnemy;

    private bool childrenInRange;

    void TrySpawnChargers()
    {
        // Try
        childrenInRange = false;
        if (children.Count > 0)
        {
            foreach (var child in children)
            {
                if (Vector3.Distance(child.transform.position, transform.position) < aggroRange)
                {
                    childrenInRange = true;
                    break;
                }
            }
        }

        if (childrenInRange) return;

        foreach (var spawnPos in spawnEnemyPos)
        {
            var dir = spawnPos.position - transform.position;
            dir.Normalize();
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, 100, LayerMask.GetMask("Default")))
            {
                if (spawnEnemyPos.Contains(hit.transform))
                {
                    validSpawns.Add(hit.transform);
                }
            }
        }

        if (validSpawns.Count == 0) return;
        //

        Debug.Log("Valid spawns Found");

        // Spawn
        foreach (var spawn in validSpawns)
        {
            var egg = Instantiate(spawnBulletPrefab, transform.position, Quaternion.identity);
            egg.parent = this;
            egg.speed = spawnBulletSpeed;
            egg.destination = spawn;
            egg.LayEgg();

            spawnedEnemies++;
            if (spawnedEnemies == numberOfSpawn) break;
        }

        validSpawns.Clear();
        spawnedEnemies = 0;
        //
    }

    #endregion

    #region Stunned

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

    #endregion
    
    #region Anim

    void SetAnimationTimer(bool isRetract)
    {
        if (isRetract)
        {
            timer = transitionTime;
        }
        else
        {
            timer = 0;
        }
    }

    void AnimateMasks(bool isRetract)
    {
        //activeMasks = timer >= transitionTime * 0.5f;
        if (!isRetract)
        {
            if (timer < transitionTime)
            {
                timer += Time.deltaTime;
            }

            if (!allMasks[0].maskCollider.enabled && timer >= transitionTime * 0.5f)
            {
                foreach (var mask in allMasks)
                {
                    mask.maskCollider.enabled = true;
                }
            }
        }
        else
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            if (allMasks[0].maskCollider.enabled && timer <= transitionTime * 0.5f)
            {
                foreach (var mask in allMasks)
                {
                    mask.maskCollider.enabled = false;
                }
            }
        }

        transitionState.clip.SampleAnimation(gameObject, transitionCurve.Evaluate(timer));
        
        
    }

    #endregion

    public override void ApplyTelekinesis()
    {
        base.ApplyTelekinesis();
        if (currentState != States.Paralysed)
        {
            currentState = States.Paralysed;
            body.constraints = RigidbodyConstraints.FreezeAll;
            agent.enabled = false;
        }
        else
        {
            currentState = States.Roam;
            repositioning = false;
            agent.enabled = true;
        }
    }


}
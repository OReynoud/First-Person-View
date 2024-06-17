using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Mechanics;
using NaughtyAttributes;
using Unity.VisualScripting;
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

    [BoxGroup] public Transform mesh;
    [BoxGroup] public ParticleSystem[] bullet_VFX;
    
    [BoxGroup] public ParticleSystem[] body_VFX;
    [BoxGroup] public TrailRenderer[] body_Trail;

    [BoxGroup] public float bodySize;
    [BoxGroup] public GameObject inkCrown;


    [HideInInspector] public Arena arena;
    [HideInInspector] public bool arenaSpawn = false;
    


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
    private float aggressiveAggroRange;
    
    [Foldout("Roam State")] [SerializeField]
    private float minFlyRange = 4;

    [Foldout("Roam State")] [SerializeField]
    private float flySpeed;

    [Foldout("Roam State")] [SerializeField]
    private float aggressiveFlySpeed;

    [Foldout("Roam State")] [SerializeField]
    private float idleTime;
    
    [Foldout("Shoot State")] [SerializeField]
    private int numberOfBullets;
    
    [Foldout("Shoot State")] [SerializeField]
    private float angleArcTolerance = 20;

    [Foldout("Shoot State")] [SerializeField]
    private float bulletSpeed;
    
    [Foldout("Shoot State")] [SerializeField]
    private float bulletDamage = 1;

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
    [Foldout("Debug")] [SerializeField] private float idleTimer;
    [Foldout("Debug")] public States currentState;
    [Foldout("Debug")] [SerializeField] private float bulletTimer;
    [Foldout("Debug")] [SerializeField] private float bulletsLeft;
    [Foldout("Debug")] [SerializeField] private bool spawning;
    [Foldout("Debug")] [SerializeField] private bool seenPlayer;
    [Foldout("Debug")] [SerializeField] private bool facingPlayer;
    [Foldout("Debug")] [SerializeField] private float weaknessTimer;
    [Foldout("Debug")] [SerializeField] private bool recentlyAttacked;

    [Foldout("Debug")] [SerializeField] public List<ChargerBehavior> children = new List<ChargerBehavior>();
    private Vector3 origin;

    private float baseMeshSize;

#if UNITY_EDITOR
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
#endif


    // Update is called once per frame
    public override void Awake()
    {
        base.Awake();
        transitionCurve.AddKey(0, 0);
        transitionCurve.AddKey(transitionTime, 1);
        origin = transform.position;
        baseMeshSize = mesh.localScale.x;
        
        var main = new ParticleSystem().main;
        foreach (var vfx in bullet_VFX)
        {
            main = vfx.main;
            main.startSpeed = bulletSpeed;
            vfx.Stop();
        }

        aggressiveAggroRange = aggroRange * 5;
    }

    public override void Start()
    {
        base.Start();
        bulletsLeft = numberOfBullets;
        SetAnimationTimer(true);
        currentState = States.Roam;
        agent.enabled = true;
        agent.speed = flySpeed;

        seenPlayer = arenaSpawn;
    }

    public LayerMask playerLayer;
    public float rotationSpeed;

    public override void Update()
    {
        base.Update();
        if (seenPlayer)
        {
            agent.speed = aggressiveFlySpeed;
            aggroRange = aggressiveAggroRange;
        }

        var dir = PlayerController.instance.transform.position - transform.position;
        RaycastHit hit;
        switch (currentState)
        {
            case States.Roam:
                AnimateMasks(true);
                
                Debug.DrawRay(transform.position, dir.normalized * 5, Color.magenta);
                if (Physics.Raycast(transform.position, dir.normalized, out hit, aggroRange,
                        playerLayer) && !recentlyAttacked)
                {
                    if (hit.collider.gameObject.CompareTag("Player"))
                    {
                        seenPlayer = true;
                        agent.enabled = false;
                        currentState = States.Shoot;
                        Debug.Log("Player Detected");
                        return;
                    }
                }
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (repositioning && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f))
                    {
                        repositioning = false;
                        recentlyAttacked = false;
                        currentState = States.Repositioning;
                        
                        //SON
                        break;
                    }
                }

                if (repositioning) break;
                repositioning = true;
                Vector3 temp = origin + new Vector3(Random.Range(-1f, 1f) * flyAreaRange, 0,
                    Random.Range(-1f, 1f) * flyAreaRange);
                temp = temp + (transform.position + temp).normalized * minFlyRange;
                agent.SetDestination(temp);
                break;
            case States.Repositioning:
                AnimateMasks(true);
                idleTimer += Time.deltaTime;
                if (idleTimer >= idleTime)
                {
                    idleTimer = 0;
                    currentState = States.Roam;
                }
                repositioning = false;
                if (Physics.Raycast(transform.position, dir.normalized, out hit, aggroRange,
                        playerLayer) && !recentlyAttacked)
                {
                    if (hit.collider.gameObject.CompareTag("Player"))
                    {
                        seenPlayer = true;
                        agent.enabled = false;
                        currentState = States.Shoot;
                        Debug.Log("Player Detected");
                    }
                }
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
                seenPlayer = true;
                AnimateMasks(false);
                break;
            case States.KnockBack:
                if (!knockedBack) currentState = States.Roam;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private GameObject audioBuildUp;
    
    #region Shoot

    void RotateModel()
    {
        if (audioBuildUp == null)
        {
            audioBuildUp = AudioManager.instance.PlaySound(7, 4, gameObject, 0f, true);
        }
        
        var dir = PlayerController.instance.transform.position - transform.position;
        dir.Normalize();
        dir = new Vector3(dir.x, 0, dir.z);
        //var angle = Mathf.Atan2(dir.z, dir.x);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), rotationSpeed);
        if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(dir)) < angleArcTolerance)
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
        if (audioBuildUp != null)
        {
            Destroy(audioBuildUp);
        }
        
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

                recentlyAttacked = true;
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
        var dir = PlayerController.instance.playerCam.position - bulletSpawnPos[rand].position + new Vector3(Random.Range(-0.5f,0.5f),Random.Range(-0.5f,0.5f),0);
        Debug.DrawRay( bulletSpawnPos[rand].position,dir.normalized * 10, Color.cyan, 5);
        var bullet = Instantiate(bulletPrefab, bulletSpawnPos[rand].position, Quaternion.LookRotation(dir.normalized));
        bullet_VFX[0].transform.position = bulletSpawnPos[rand].position;
        bullet_VFX[0].transform.rotation = Quaternion.LookRotation(dir.normalized);
        bullet_VFX[0].Play();
        bullet.rb.velocity = dir.normalized * bulletSpeed;
        bullet.damage = bulletDamage;

        AudioManager.instance.PlaySound(7, 0, gameObject, 0.1f, false);

        //SON
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


        Debug.Log("Valid spawns Found");

        // Spawn
        foreach (var spawn in spawnEnemyPos)
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
            seenPlayer = true;
            currentState = States.Stunned;
            StartCoroutine(Stun());
        }
        base.ApplyStun();
    }

    IEnumerator Stun()
    {

        transitionState.Play("A_CollectorStunStart");
        agent.enabled = false;
        transform.DOShakeScale(0.2f, Vector3.one * 0.2f);
        
        yield return new WaitForSeconds(stunDuration);

        agent.enabled = true;

        agent.SetDestination(PlayerController.instance.transform.position);
        currentState = States.Repositioning;
        transitionState.Play("A_CollectorStunEnd");
        //SON
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

            if (!allMasks[0].maskCollider.enabled && timer >= transitionTime * 0.8f)
            {
                foreach (var mask in allMasks)
                {
                    mask.maskCollider.enabled = true;
                }

                if (!inkCrown.activeInHierarchy)
                {
                    inkCrown.SetActive(true);
                }
                if (body_VFX[0].isPlaying)
                {
                    foreach (var vfx in body_VFX)
                    {
                        vfx.Stop();
                    }
                }
            }
        }
        else
        {
            foreach (var trail in body_Trail)
            {
                trail.enabled = true;
            }

            if (inkCrown.activeInHierarchy)
            {
                inkCrown.SetActive(false);
            }
            if (!body_VFX[0].isPlaying)
            {
                foreach (var vfx in body_VFX)
                {
                    vfx.Play();
                }
            }

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

        body_VFX[0].transform.localScale = Vector3.Lerp(Vector3.one * bodySize,Vector3.zero, timer/transitionTime);
        
        mesh.localScale = Vector3.Lerp(Vector3.one * 0.1f,Vector3.one * baseMeshSize, timer/transitionTime);
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
            TrySpawnChargers();
            recentlyAttacked = true;
            repositioning = false;
            agent.enabled = true;
        }
    }
    
    public override void KnockBack(Vector3 dir, float force)
    {
        base.KnockBack(dir,force);
        body.constraints |= RigidbodyConstraints.FreezePositionY;
        body.useGravity = false;

        //StartCoroutine(ApplyKnockBack(dir, force));
    }
    
    public override void TakeDamage(Collider part, Vector3 dir, float damage, float knockBack)
    {
        base.TakeDamage(part, dir,  damage, knockBack);
        AudioManager.instance.PlaySound(7, 8, gameObject, 0.1f, false);
    }

    public override void Die()
    {
        //SON
        if (arenaSpawn)
        {
            arena.currentEnemies.Remove(this);
        }

        transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            base.Die();
        });
    }
}
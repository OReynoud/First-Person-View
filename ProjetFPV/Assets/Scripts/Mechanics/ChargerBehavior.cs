using System;
using System.Collections;
using System.Runtime.InteropServices;
using DG.Tweening;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Mechanics
{
    public class ChargerBehavior : Enemy
    {
        [InfoBox("Bouffon Behavior")]
        public NavMeshAgent agent;
        [InfoBox("Cercle vert = Zone de patrouille;  Cercle rouge = Zone d'aggro;  Cercle violet = portée de l'attaque", EInfoBoxType.Warning) ]
        [SerializeField]
        private float pathUpdateFrequency;



        [Foldout("Reposition state")] [SerializeField]
        private float disappearDuration = 0.5f;

        [Foldout("Reposition state")] [SerializeField]
        private float idleDuration = 1f;

        [Foldout("Reposition state")] [SerializeField]
        private float appearDuration = 0.5f;

        [Foldout("Reposition state")] [SerializeField]
        public Transform[] spawnPositions;

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
    
        [Foldout("Attack state")] [SerializeField]
        private float timeToAttack = 0.3f;

        [Foldout("Attack state")] [SerializeField]
        private float waitAfterAttack = 0.2f;

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

        private void OnEnable()
        {
            currentState = States.Neutral;
        }

        private void OnDrawGizmosSelected()
        {
            Handles.color = Color.green;
            if (Application.isPlaying)
            {
                Handles.DrawWireDisc(origin, Vector3.up, walkAreaRange, 2);
            }
            else
            {
                Handles.DrawWireDisc(transform.position, Vector3.up, walkAreaRange, 2);
            }

            Handles.color = Color.red;
            Handles.DrawWireDisc(transform.position, Vector3.up, rushRange, 2);

            Handles.color = Color.magenta;
            Handles.DrawWireDisc(transform.position, Vector3.up, atkRange, 2);
        }

        public override void ApplyTelekinesis()
        {
            base.ApplyTelekinesis();
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

        // Start is called before the first frame update
        private Vector3 origin;

        public override void Awake()
        {
            base.Awake();
            origin = transform.position;
            agent = GetComponent<NavMeshAgent>();
        }

        IEnumerator Wander()
        {
            var oui = Random.insideUnitCircle * walkAreaRange;
            agent.speed = wanderSpeed + Random.Range(-1f, 1f);
            agent.SetDestination(origin + new Vector3(oui.x, 0, oui.y));
            yield return new WaitForSeconds(wanderFrequency + Random.Range(-wanderFrequency * .2f,wanderFrequency * .2f));
            if (currentState == States.Neutral)
            {
                StartCoroutine(Wander());
            }
        }

        [HideInInspector] public bool arenaSpawn;
        [HideInInspector] public Vector3 locationToSpawn;
        [HideInInspector] public Arena arena;
        public override void Start()
        {
            base.Start();
            agent.stoppingDistance = atkRange;
            agent.speed = wanderSpeed;
            
            if (arenaSpawn)
            {
                ArenaSpawn();
                return;
            }
            agent.enabled = true;

            currentState = States.Neutral;

            if (pathRoutine != null)
            {
                StopCoroutine(pathRoutine);
            }
            pathRoutine = null;
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
                    if (pathRoutine == null)
                    {
                        pathRoutine = StartCoroutine(SetAgentDestination());
                    }
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
            var temp = allMasks[0].tr.localScale;
            foreach (var part in allMasks)
            {
                part.maskCollider.enabled = false;
                part.tr.DOLocalMove(Vector3.zero, disappearDuration);
                part.tr.DOScale(Vector3.one * 0.8f, disappearDuration * 0.5f);
            }

            body.isKinematic = true;
            transform.DOMove(transform.position + Vector3.down * 5, disappearDuration);
            yield return new WaitForSeconds(disappearDuration + idleDuration);
            
            transform.position = GetRandomSpawnPoint() + Vector3.down * 5;
            transform.DOMove(transform.position + Vector3.up * 5, appearDuration).OnComplete(() =>
            {
                repositioning = false;
                currentState = States.Rush;
                foreach (var part in allMasks)
                {
                    part.maskCollider.enabled = true;
                    part.tr.DOLocalMove(part.origin, appearDuration * 2);
                    part.tr.DOScale(temp, appearDuration);
                }

                body.isKinematic = false;
                agent.enabled = true;
            });
        }

        public LayerMask playerLayer;

        void TryDetectPlayer()
        {
            var dir = PlayerController.instance.transform.position - transform.position;
            Debug.DrawRay(transform.position, dir.normalized * 5, Color.magenta);
            if (!Physics.Raycast(transform.position, dir.normalized, out RaycastHit hit, rushRange, playerLayer)) return;
            if (!hit.collider.gameObject.CompareTag("Player")) return;

            Debug.Log("Player Detected");
            currentState = States.Repositioning;
            agent.speed = rushSpeed;
            SetAgentDestination();
        }

        private Coroutine pathRoutine;

        private IEnumerator SetAgentDestination()
        {
            yield return new WaitForSeconds(pathUpdateFrequency);
            while (currentState != States.Rush)
            {
                yield return new WaitForSeconds(pathUpdateFrequency);
            }


            agent.SetDestination(PlayerController.instance.transform.position);

            pathRoutine = StartCoroutine(SetAgentDestination());
        }


        IEnumerator AttackPlayer()
        {
            currentState = States.Attack;
            Debug.Log("j'attaque le joueur");
            yield return new WaitForSeconds(timeToAttack);
            var colliders = Physics.OverlapSphere(transform.position + transform.forward, 1f, playerLayer);
            foreach (var col in colliders)
            {
                if (col.TryGetComponent(out PlayerController player))
                {
                    Debug.Log("J'ai touché le joueur");
                    player.TakeDamage(atkDamage);
                }
            }

            yield return new WaitForSeconds(waitAfterAttack);
            currentState = States.Repositioning;
        }

        
        void ArenaSpawn()
        {
            currentState = States.Rush;
            agent.enabled = false;
            body.isKinematic = true;
            transform.DOMove(transform.position + Vector3.up * 6, appearDuration).OnComplete(() =>
            {
                body.isKinematic = false;
                agent.enabled = true;
            });
        }

        public override void TakeDamage(Collider part, bool superShot)
        {
            base.TakeDamage(part, superShot);
            if (currentState == States.Neutral)
            {
                currentState = States.Rush;
            }
        }

        public override void Die()
        {
            if (arenaSpawn)
            {
                arena.currentEnemies.Remove(this);
            }
            
            base.Die();
        }

        Vector3 GetRandomSpawnPoint()
        {
            var random = Random.Range(0, spawnPositions.Length);
            return spawnPositions[random].position;
        }
    }
}
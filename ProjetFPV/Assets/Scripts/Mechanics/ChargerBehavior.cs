using System;
using System.Collections;
using System.Collections.Generic;
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
        [HorizontalLine(color: EColor.Red)]
        [InfoBox("Charger Behavior")]
        [InfoBox("Cercle vert = Zone de patrouille;  Cercle rouge = Zone d'aggro;  Cercle violet = portée de l'attaque", EInfoBoxType.Warning)]
        [BoxGroup][SerializeField]
        private float pathUpdateFrequency;



        [Foldout("Reposition state")] [SerializeField]
        private float disappearDuration = 0.5f;

        [Foldout("Reposition state")] [SerializeField]
        private float idleDuration = 1f;

        [Foldout("Reposition state")] [SerializeField]
        private float appearDuration = 0.5f;
        
        [Foldout("Reposition state")] [SerializeField]
        private float playerProximityTolerance = 5f;

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
        private float jumpOverShoot;
      
        [Foldout("Attack state")] [SerializeField]
        private float jumpHeight;
        
        [MinMaxSlider(0f,10f)][Foldout("Attack state")] [SerializeField]
        private Vector2 jumpDuration;
    
        [Foldout("Attack state")] [SerializeField]
        private float waitBeforeJump = 0.3f;

        [Foldout("Attack state")] [SerializeField]
        private float waitAfterAttack = 0.2f;

        public enum States
        {
            Neutral,
            Repositioning,
            Rush,
            Attack,
            Stunned,
            Paralysed,
            KnockBack
        }

        public States currentState;
        private bool repositioning;
        private Coroutine attackRoutine;
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
        
        
        
        private void OnEnable()
        {
            currentState = States.Neutral;
        }
        public override void Awake()
        {
            base.Awake();
            origin = transform.position;
            agent = GetComponent<NavMeshAgent>();
        }
        

        
        public override void Start()
        {
            base.Start();
            //agent.stoppingDistance = atkRange;
            agent.speed = wanderSpeed;
            
            if (arenaSpawn)
            {
                ArenaSpawn();
                return;
            }

            if (collectorSpawn)
            {
                CollectorSpawn();
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
        public override void Update()
        {
            base.Update();
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
                        attackRoutine = StartCoroutine(AttackPlayer());
                    }

                    break;
                case States.KnockBack:
                    if (!knockedBack) currentState = States.Repositioning;
                    break;
                case States.Attack:
                    break;
                case States.Stunned:
                    break;
                case States.Paralysed:
                    if (PlayerController.instance.controlledProp == this) break;
                    //Debug.DrawRay(transform.position,Vector3.down * 1.5f);
                    if (Physics.Raycast(transform.position,Vector3.down,1.7f,LayerMask.GetMask("Default")))
                    {
                        Debug.Log("Landed");
                        currentState = States.Rush;
                        agent.enabled = true;
                        agent.SetDestination(PlayerController.instance.transform.position);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Paralysed

        public override void ApplyTelekinesis()
        {
            base.ApplyTelekinesis();
            if (currentState != States.Paralysed)
            {
                currentState = States.Paralysed;
                body.constraints = RigidbodyConstraints.FreezeAll;
                InterruptAttack();
            }
            else
            {
                body.constraints = RigidbodyConstraints.FreezeRotation;
            }
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
            body.constraints = RigidbodyConstraints.FreezeRotation;
            InterruptAttack();
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
            body.constraints = RigidbodyConstraints.FreezeAll;
            agent.SetDestination(PlayerController.instance.transform.position);
            currentState = States.Repositioning;

        }

        #endregion
        
        #region Wander

        private Vector3 origin;
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

        #endregion

        #region Rush

        
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


        #endregion

        #region Reposition

        private IEnumerator Reposition()
        {
            agent.enabled = false;
            repositioning = true;
            var temp = allMasks[0].tr.localScale;
            foreach (var part in allMasks)
            {
                part.maskCollider.enabled = false;
                part.tr.DOLocalMove(origin + Vector3.down * 0.5f, disappearDuration);
                part.tr.DOScale(temp * 0.8f, disappearDuration * 0.5f);
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

        #endregion
        
        #region Attack

        public LayerMask playerLayer;
        private Vector3 playerPos;
        private Vector3 actualDestination;
        public Tween jumpTween;
        public Tween jumpRotationTween;

        IEnumerator AttackPlayer()
        {
            currentState = States.Attack;
            playerPos = PlayerController.instance.transform.position + transform.forward;
            agent.SetDestination(playerPos);
            actualDestination = new Vector3(agent.destination.x,playerPos.y, agent.destination.z);
            agent.enabled = false;
            Debug.Log("j'attaque le joueur");
            yield return new WaitForSeconds(waitBeforeJump);
            transform.rotation = Quaternion.Euler(40,transform.eulerAngles.y,transform.eulerAngles.z);

            var calcJumpTime = (Vector3.Distance(transform.position, playerPos) * jumpDuration.y)/atkRange;
            if (calcJumpTime < jumpDuration.x) calcJumpTime = jumpDuration.x;
            jumpTween = transform.DOJump(actualDestination + transform.forward * jumpOverShoot, jumpHeight, 1, calcJumpTime);
            jumpRotationTween = transform.DORotate(new Vector3(0,transform.eulerAngles.y,transform.eulerAngles.z) , calcJumpTime);
            yield return new WaitForSeconds(calcJumpTime);
            // var colliders = Physics.OverlapSphere(transform.position , 2f, playerLayer);
            // foreach (var col in colliders)
            // {
            //     if (col.TryGetComponent(out PlayerController player))
            //     {
            //         player.TakeDamage(atkDamage);
            //     }
            // }

            yield return new WaitForSeconds(waitAfterAttack);
            hasDealtDamage = false;
            currentState = States.Repositioning;
        }

        #endregion

        #region SpawnTypes
        [HideInInspector] public bool arenaSpawn;
        [HideInInspector] public bool collectorSpawn;
        [HideInInspector] public Arena arena;
        [HideInInspector] public CollectorBehavior parentEnemy;
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

        void CollectorSpawn()
        {
            currentState = States.Rush;
            agent.enabled = false;
            body.isKinematic = true;
            transform.DOMove(transform.position + Vector3.up, appearDuration).OnComplete(() =>
            {
                body.isKinematic = false;
                agent.enabled = true;
            });
        }

        #endregion


        public override void TakeDamage(Collider part, Vector3 dir, float damage, float knockBack)
        {
            base.TakeDamage(part, dir,  damage, knockBack);
            if (currentState == States.Neutral)
            {
                currentState = States.Rush;
            }
        }

        public override void TakeDamage(float knockBackValue, Vector3 knockBackDir, Vector3 pointOfForce, float damage, Collider maskCollider)
        {
            base.TakeDamage(knockBackValue, knockBackDir, pointOfForce, damage, maskCollider);
            foreach (var mask in allMasks)
            {
                if (mask.maskCollider != maskCollider)continue;
                mask.maskHealth -= damage;
            }
        }

        public override void KnockBack(Vector3 dir, float force)
        {
            base.KnockBack(dir, force);
            
            jumpTween.Kill();
            jumpRotationTween.Kill(true);
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
            }
            currentState = States.KnockBack;
        }

        public override void Die()
        {
            if (arenaSpawn)
            {
                arena.currentEnemies.Remove(this);
            }

            if (collectorSpawn)
            {
                parentEnemy.children.Remove(this);
            }
            
            base.Die();
        }

        void InterruptAttack()
        {
            agent.enabled = false;
            jumpTween.Kill();
            jumpRotationTween.Kill(true);
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
            }
        }

        private bool hasDealtDamage = false;
        private void OnCollisionEnter(Collision other)
        {
            if (currentState != States.Attack || !other.gameObject.TryGetComponent(out PlayerController player) || hasDealtDamage) return;

            hasDealtDamage = true;
            Debug.Log("J'ai touché le joueur");
            player.TakeDamage(atkDamage);
        }

        private List<Vector3> validPos = new List<Vector3>();
        Vector3 GetRandomSpawnPoint()
        {
            validPos.Clear();
            foreach (var pos in spawnPositions)
            {
                if (Vector3.Distance(pos.position, PlayerController.instance.transform.position) 
                    < playerProximityTolerance) 
                    continue;
                
                validPos.Add(pos.position);
            }

            var random = Random.Range(0, validPos.Count);
            if (validPos.Count == 0)
            {
                Debug.LogError("Aucun spawn valide trouvé, le joueur est trop près de tous les spawns possibles");
                random = Random.Range(0, spawnPositions.Length);
                return spawnPositions[random].position;
            }
            return validPos[random];
        }
    }
}
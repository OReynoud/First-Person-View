using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
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
        [BoxGroup][SerializeField]
        private Collider bodyColl;



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
        
        [Foldout("Reposition state")] [SerializeField]
        [Range(0,1)]public float percentHealthToTriggerReposition = 0.5f;

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
        
        [Foldout("Attack state")] [SerializeField]
        private float predictDistance;
        
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
            KnockBack,
            Dying
        }

        public States currentState;
        private bool repositioning;
        private Coroutine attackRoutine;
        private Animation anim;

#if UNITY_EDITOR
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
        
#endif
        
        
        
        private void OnEnable()
        {
            currentState = States.Neutral;
        }
        public override void Awake()
        {
            base.Awake();
            origin = transform.position;
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animation>();
        }


        private float timeràLaCon;
        public override void Start()
        {
            base.Start(); 
            
            timeràLaCon = 0.2f;
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

        private bool tpViaDmg = false;
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
                    var dir = PlayerController.instance.transform.position - transform.position;
                    if (Physics.Raycast(transform.position, dir.normalized, out RaycastHit hit, rushRange, playerLayer) 
                        && 
                        Vector3.Distance(PlayerController.instance.transform.position, transform.position) < atkRange)
                    {
                        if (!hit.collider.gameObject.CompareTag("Player")) break;
                        attackRoutine = StartCoroutine(AttackPlayer());
                    }

                    break;
                case States.KnockBack:
                    if (!knockedBack)
                    {
                        if (!tpViaDmg && allMasks[0].maskHealth <= allMasks[0].baseHealth * percentHealthToTriggerReposition)
                        {
                            tpViaDmg = true;
                            currentState = States.Repositioning;
                        }
                        else
                        {
                            currentState = States.Rush;
                        }
                    }
                    break;
                case States.Attack:
                    break;
                case States.Stunned:
                    if (!anim.isPlaying) anim.Play("TEMP_StunLoop");
                    break;
                case States.Paralysed:
                    if (PlayerController.instance.tkManager.controlledProp == this) break;
                    timeràLaCon -= Time.deltaTime;
                    if (timeràLaCon > 0) break;
                    if (Physics.Raycast(transform.position,Vector3.down,1.7f,LayerMask.GetMask("Ground")))
                    {
                        Debug.Log("Landed");
                        currentState = States.Rush;
                        agent.enabled = true;
                        agent.SetDestination(PlayerController.instance.transform.position);
                        timeràLaCon = 0.2f;
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
            anim.Play("TEMP_StunStart");
            yield return new WaitForSeconds(stunDuration);

            anim.Stop();
            anim.Play("TEMP_StunEnd");
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
                part.tr.DOLocalMove(part.tr.localPosition + Vector3.down * 0.5f, disappearDuration);
                part.tr.DOScale(temp * 0.8f, disappearDuration * 0.5f);
            }

            body.isKinematic = true;
            bodyColl.enabled = false;
            transform.DOMove(transform.position + Vector3.down * 5, disappearDuration);
            AudioManager.instance.PlaySound(8, 6, gameObject, 0.1f, false);
            if (Random.Range(0f, 1f) <= 0.3f)
            {
                AudioManager.instance.PlaySound(8, 7, gameObject, 0.1f, false);
            }
            
            yield return new WaitForSeconds(disappearDuration + idleDuration);
            
            transform.position = GetRandomSpawnPoint() + Vector3.down * 5;
            transform.LookAt(new Vector3(PlayerController.instance.transform.position.x, transform.position.y,PlayerController.instance.transform.position.z));
            AudioManager.instance.PlaySound(8, 1, gameObject, 0.1f, false);
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
                bodyColl.enabled = true;
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
            agent.enabled = false;
            Debug.Log("j'attaque le joueur");
            var t = Time.time + waitBeforeJump;
            while (Time.time < t)
            {
                playerPos = PlayerController.instance.transform.position;
                transform.LookAt(playerPos);
                transform.rotation = Quaternion.Euler(0,transform.eulerAngles.y,transform.eulerAngles.z);
                yield return null;
            }

            playerPos = PlayerController.instance.transform.position + transform.forward;
            actualDestination = playerPos + PlayerController.instance.rb.velocity.normalized * predictDistance;
            transform.LookAt(actualDestination);
            transform.rotation = Quaternion.Euler(0,transform.eulerAngles.y,transform.eulerAngles.z);
            var calcJumpTime = (Vector3.Distance(transform.position, playerPos) * jumpDuration.y)/atkRange;
            if (calcJumpTime < jumpDuration.x) calcJumpTime = jumpDuration.x;
            jumpTween = transform.DOJump(actualDestination + transform.forward * jumpOverShoot, jumpHeight, 1, calcJumpTime);
            AudioManager.instance.PlaySound(8, 0, gameObject, 0.1f, false);
            transform.rotation = Quaternion.Euler(40,transform.eulerAngles.y,transform.eulerAngles.z);
            jumpRotationTween = transform.DORotate(new Vector3(0,transform.eulerAngles.y,transform.eulerAngles.z) , calcJumpTime);
            yield return new WaitForSeconds(calcJumpTime);


            yield return new WaitForSeconds(PlayerPrefs.GetInt("difficulty") == 0 ? waitAfterAttack : waitAfterAttack * 2f);
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
            AudioManager.instance.PlaySound(8, 1, gameObject, 0.1f, false);
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
            AudioManager.instance.PlaySound(8, 1, gameObject, 0.1f, false);
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
            AudioManager.instance.PlaySound(8, 5, gameObject, 0.1f, false);
            
            if (currentState == States.Neutral)
            {
                currentState = States.Rush;
            }
        }

        public override void TakeDamage(float knockBackValue, Vector3 knockBackDir, Vector3 pointOfForce, float damage, Collider maskCollider)
        {
            base.TakeDamage(knockBackValue, knockBackDir, pointOfForce, damage, maskCollider);
            AudioManager.instance.PlaySound(8, 5, gameObject, 0.1f, false);
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

        public override async void Die()
        {
            anim.Play("A_BouffonDeath");
            GameManager.instance.OnKillEnemy();
            
            if (isGrabbed)PlayerController.instance.ReleaseProp(new InputAction.CallbackContext());
            grabbedTween.Complete();
            grabbedTween.Kill(true);
            var oui = Instantiate(GameManager.instance.inkStainPrefab, transform.position, Quaternion.Euler(-90,0,0));
            oui.storedInk = inkIncrease;
            transform.DOMove(new Vector3(transform.position.x,0,transform.position.z), 0.4f).SetEase(Ease.InCubic);
            while (anim.isPlaying)
            {
                await Task.Delay(10);
            }
            if (arenaSpawn)
            {
                arena.currentEnemies.Remove(this);
            }

            if (collectorSpawn)
            {
                parentEnemy.children.Remove(this);
            }
        
            if (respawnOnDeath) GameManager.instance.Respawn(this);
            gameObject.SetActive(false);
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
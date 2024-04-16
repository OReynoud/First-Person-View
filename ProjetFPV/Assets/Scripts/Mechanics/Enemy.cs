using System;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mechanics
{
    public class Enemy : ControllableProp
    {
        [InfoBox("Universal Behavior")] 
        
        [BoxGroup("Masks")] public BodyPart[] allMasks;
        
        [BoxGroup("Masks")] [ReadOnly] public int maskCount;
        
        [BoxGroup("Stunned state")] public float stunDuration = 2f;
    
        [BoxGroup("Stunned state")] public Material defaultMat;
    
        [BoxGroup("Stunned state")] [SerializeField] public Material stunnedMat;

        
        [Foldout("Dummy Target")] public bool isImmobile = true;
        [Foldout("Dummy Target")] public bool respawnOnDeath = true;

        [Foldout("Dummy Target")] [HideIf("isImmobile")] public List<Transform> waypoints;
        [Foldout("Dummy Target")] [HideIf("isImmobile")] public float translationSpeed = 0.1f;
        [Foldout("Dummy Target")] [ShowIf("respawnOnDeath")] public Transform respawnPoint;

        private int currentIndex = 1;

        private int previousIndex = 0;


        private void OnValidate()
        {
            maskCount = allMasks.Length;
        }


        // Start is called before the first frame update
        public override void Awake()
        {
            base.Awake();
            currentIndex = 1;
            previousIndex = 0;
            foreach (var mask in allMasks)
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (mask.maskHealth <= 0)
                {
                    mask.maskHealth = 1;
                }
                mask.baseHealth = mask.maskHealth;
                mask.tr = mask.maskCollider.transform;
                mask.origin = mask.tr.localPosition;
                mask.meshRenderer = mask.tr.GetComponent<MeshRenderer>();
            }
        }
        public virtual void Start()
        {
            maskCount = allMasks.Length;
            foreach (var mask in allMasks)
            {
                mask.maskCollider.gameObject.SetActive(true);
                mask.maskHealth = mask.baseHealth;
                mask.broken = false;
                mask.meshRenderer.material = defaultMat;

            }
            
            body.constraints = RigidbodyConstraints.FreezeAll;
            body.isKinematic = false;
        }
        

        public virtual void ApplyStun()
        {

        }

        public void OnDrawGizmosSelected()
        {
            if (isImmobile)
            {
                return;
            }
            for (int i = 1; i < waypoints.Count; i++)
            {
                Debug.DrawLine(waypoints[i - 1].position,waypoints[i].position );
            }
        
            Debug.DrawLine(waypoints[^1].position,waypoints[0].position );
        }

        public virtual void TakeDamage(Collider partHit)
        {
            for (int i = 0; i < allMasks.Length; i++)
            {
                if (partHit != allMasks[i].maskCollider)continue;

                allMasks[i].maskHealth--;
                if ( allMasks[i].maskHealth <= 0)
                {
                    allMasks[i].maskCollider.gameObject.SetActive(false);
                    allMasks[i].broken = true;
                    maskCount--;
                }
                break;
            }

            if (maskCount <= 0) Die();
        }

        public void TakeDamage(float knockBackValue, Vector3 knockBackDir, Vector3 pointOfForce)
        {

            InputAction.CallbackContext dummy = new InputAction.CallbackContext();
            if (isGrabbed) PlayerController.instance.ReleaseProp(dummy);
            ApplyStun();
            // body.constraints = RigidbodyConstraints.None;
            // body.useGravity = true;
            // body.AddForceAtPosition(knockBackDir * knockBackValue,pointOfForce, ForceMode.Impulse);
        }

        private void Die()
        {
            GameManager.instance.OnKillEnemy();
        
        
            grabbedTween.Complete();
            grabbedTween.Kill();
        
        
            if (respawnOnDeath) GameManager.instance.Respawn(this);
            gameObject.SetActive(false);

        }

        // Update is called once per frame
        void Update()
        {
            if (isImmobile || isGrabbed) return;
            MoveBetweenWaypoints();
        }

        private float lerp;

        void MoveBetweenWaypoints()
        {
            lerp += Time.deltaTime * translationSpeed;
            transform.position = Vector3.Lerp(waypoints[previousIndex].position, waypoints[currentIndex].position, lerp);
            if (lerp >= 1)
            {
                lerp = 0;
                previousIndex = currentIndex;
                currentIndex++;
                if (currentIndex >= waypoints.Count)
                {
                    currentIndex = 0;
                }
            }
        }

        private Tweener grabbedTween;
        public void GrabbedBehavior(float levitateValue, float shakeAmplitude, int shakeFrequency)
        {
            transform.DOMove(transform.position + Vector3.up * levitateValue, 0.2f).OnComplete(() =>
            {
                GrabTween(shakeAmplitude, shakeFrequency);
                CameraShake.instance.StartInfiniteShake(0);
            });
        }

        private void GrabTween(float shakeAmplitude, int shakeFrequency)
        {
            grabbedTween = transform.DOShakePosition(0.1f, Vector3.one * shakeAmplitude, shakeFrequency).OnComplete(() =>
            {
                if (isGrabbed)
                {
                    GrabTween(shakeAmplitude, shakeFrequency);
                }
            });
        }
    }
    
    
            
    [Serializable]
    public class BodyPart
    {
        [HorizontalLine(color:EColor.White)]
        public string maskName;
        [HideInInspector] public Vector3 origin;
        [HideInInspector] public int baseHealth;
        [Range(1,10)]public int maskHealth = 1;
        public Collider maskCollider;
        [HideInInspector] public Transform tr;
        [HideInInspector] public MeshRenderer meshRenderer;
        [ReadOnly] public bool broken;
    }

}

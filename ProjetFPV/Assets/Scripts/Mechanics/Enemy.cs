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
        public float maxHealth;
        public float currentHealth;

        public BodyPart[] bodyParts;
        
        public bool isImmobile = true;
        public bool respawnOnDeath = true;
        [SerializeField] private float stunDurationTK;

        [HideIf("isImmobile")] public List<Transform> waypoints;
        [HideIf("isImmobile")] public float translationSpeed = 0.1f;
        [ShowIf("respawnOnDeath")] public Transform respawnPoint;

        private int currentIndex = 1;

        private int previousIndex = 0;
        [Serializable]
        public struct BodyPart
        {
            [HorizontalLine(color:EColor.White)]
            public string bodyPartName;
            public Collider bodyPartCollider;
            public float damageMultiplier;
        }
        

        // Start is called before the first frame update
        public override void Awake()
        {
            base.Awake();
            currentIndex = 1;
            previousIndex = 0;
        }
        public virtual void Start()
        {
            currentHealth = maxHealth;
            transform.rotation = Quaternion.identity;
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

        public void TakeDamage(int damage, Collider partHit)
        {
            for (int i = 0; i < bodyParts.Length; i++)
            {
                if (partHit != bodyParts[i].bodyPartCollider)continue;
                
                var totalDmg = damage * bodyParts[i].damageMultiplier;
                currentHealth -= totalDmg;
                break;
            }

            if (currentHealth <= 0) Die();
        }

        public void TakeDamage(int damage, float knockBackValue, Vector3 knockBackDir, Vector3 pointOfForce)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die();
            }

            InputAction.CallbackContext dummy = new InputAction.CallbackContext();
            if (isGrabbed) PlayerController.instance.ReleaseProp(dummy);

            body.constraints = RigidbodyConstraints.None;
            body.useGravity = true;
            body.AddForceAtPosition(knockBackDir * knockBackValue * damage,pointOfForce, ForceMode.Impulse);
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
}

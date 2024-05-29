using System;
using Mechanics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerBullet : MonoBehaviour
    {
        public Rigidbody rb;
        public bool superShot;
        public float weakSpotDamage;
        public float weakSpotKnockBack;
        public float bodyDamage;
        public float bodyKnockBack;
        private bool hasCollided;

        public PlayerBullet(bool SuperShot, float WeakSpotDamage, float WeakSpotKnockBack)
        {
            superShot = SuperShot;
            weakSpotDamage = WeakSpotDamage;
            weakSpotKnockBack = WeakSpotKnockBack;
        }

        public MeshRenderer meshRenderer;
        // Start is called before the first frame update


        private void OnCollisionEnter(Collision other)
        {
            if (hasCollided) return;
            hasCollided = true;
            
            Debug.LogFormat("<color=yellow>Collision :</color> {0}",other.gameObject.name);
            
            if (other.collider.CompareTag(Ex.Tag_Head))
            {
                var enemy = other.collider.GetComponentInParent<Enemy>();
                enemy.TakeDamage(other.collider, transform.forward, weakSpotDamage, weakSpotKnockBack);
                GameManager.instance.HitMark(true);

                Destroy(gameObject);
                return;
            }
            
            if (other.collider.CompareTag(Ex.Tag_Body))
            {
                var enemy = other.collider.GetComponentInParent<Enemy>();
                enemy.TakeDamage(other.collider, transform.forward, bodyDamage, bodyKnockBack);
                GameManager.instance.HitMark(false);

                Destroy(gameObject);
                return;
            }

            if (other.gameObject.TryGetComponent(out IDestructible target))
            {
                target.TakeDamage();

                Destroy(gameObject);
                return;
            }


            //Coucou, Thomas est passé par là (jusqu'au prochain commentaire)
            var decal = Instantiate(GameManager.instance.inkStainDecal,
                other.GetContact(0).point + other.GetContact(0).normal * 0.02f,
                Quaternion.identity, other.transform);
            decal.transform.forward = -other.GetContact(0).normal;
            decal.transform.RotateAround(decal.transform.position, decal.transform.forward, Random.Range(-180f, 180f));
            //Je m'en vais !

            Destroy(gameObject);
        }

        private void LateUpdate()
        {
            hasCollided = false;
        }
    }
}
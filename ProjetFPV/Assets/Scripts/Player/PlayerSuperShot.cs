using System;
using System.Collections;
using System.Threading.Tasks;
using Mechanics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Player
{
    public class PlayerSuperShot : MonoBehaviour
    {
        public Transform hitboxHolder;
        public LayerMask mask;
        public float damage;
        public float knockBack;

        public Collider coll;
        public Vector3 scale;


        public PlayerSuperShot( float Damage, float KnockBack)
        {
            damage = Damage;
            knockBack = KnockBack;
        }

        // Start is called before the first frame update
        private async void OnEnable()
        {
            StartCoroutine(DisableHitbox());
            await Task.Delay(30);
            coll.enabled = true;
            hitboxHolder.localScale = scale;
            await Task.Delay(30);

            coll.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!ValidateHit(other)) return;
            if (other.CompareTag("Head"))
            {
                if (other.transform.parent.TryGetComponent(out Enemy enemy))
                {
                    enemy.TakeDamage(other, PlayerController.instance.playerCam.forward, damage, knockBack);


                    GameManager.instance.HitMark(true);
                }
            }

            if (other.gameObject.TryGetComponent(out IDestructible target))
            {
                target.TakeDamage();
            }


            // //Coucou, Thomas est passé par là (jusqu'au prochain commentaire)
            // var decal = Instantiate(GameManager.instance.inkStainDecal, other.GetContact(0).point + other.GetContact(0).normal * 0.02f, Quaternion.identity, other.transform);
            // decal.transform.forward = -other.GetContact(0).normal;
            // decal.transform.RotateAround(decal.transform.position, decal.transform.forward, Random.Range(-180f, 180f));
            // //Je m'en vais !
        }


        bool ValidateHit(Collider other)
        {
            var dir = PlayerController.instance.transform.position - other.transform.position;
            dir.Normalize();
            Debug.DrawRay(other.transform.position,dir * 5, Color.blue, 3f);
            
            Physics.Raycast(
                other.transform.position,
                dir,
                out RaycastHit hit,
                1000, mask);
            
            return hit.collider.CompareTag(Ex.Tag_Player);
        }

        IEnumerator DisableHitbox()
        {
            yield return new WaitForSeconds(0.3f);
            gameObject.SetActive(false);
        }
    }
}
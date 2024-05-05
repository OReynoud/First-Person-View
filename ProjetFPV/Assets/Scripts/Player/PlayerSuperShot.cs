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
        public bool superShot;
        public float damage;
        public float knockBack;

        public Collider coll;
        public Vector3 scale;
        
        

        public PlayerSuperShot(bool SuperShot, float Damage, float KnockBack)
        {
            superShot = SuperShot;
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
            Debug.Log("Hit something" + other.gameObject, other.gameObject);
            if (other.CompareTag("Head"))
            {
                if (other.transform.TryGetComponent(out Enemy enemy))
                {
                
                    enemy.TakeDamage(other, superShot, damage, knockBack);
                

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


        IEnumerator DisableHitbox()
        {
            yield return new WaitForSeconds(0.3f);
            gameObject.SetActive(false);
        }
    }
}

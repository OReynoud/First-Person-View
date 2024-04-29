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

        public MeshRenderer meshRenderer;
        // Start is called before the first frame update


        private void OnCollisionEnter(Collision other)
        { 
            Debug.Log("Hit something" + other.gameObject, other.gameObject);
            if (other.collider.CompareTag("Head"))
            {
                if (other.transform.TryGetComponent(out Enemy enemy))
                {
                
                    enemy.TakeDamage(other.collider, superShot);
                

                    GameManager.instance.HitMark(true);
                }
            }

            if (other.gameObject.TryGetComponent(out IDestructible target))
            {
                target.TakeDamage();
            }


            //Coucou, Thomas est passé par là (jusqu'au prochain commentaire)
            var decal = Instantiate(GameManager.instance.inkStainDecal, other.GetContact(0).point + other.GetContact(0).normal * 0.02f, Quaternion.identity, other.transform);
            decal.transform.forward = -other.GetContact(0).normal;
            decal.transform.RotateAround(decal.transform.position, decal.transform.forward, Random.Range(-180f, 180f));
            //Je m'en vais !
        
            Destroy(gameObject);
        }
    }
}

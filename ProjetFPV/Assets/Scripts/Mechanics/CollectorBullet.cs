using System;
using UnityEngine;

namespace Mechanics
{
    [RequireComponent(typeof(Rigidbody))]
    public class CollectorBullet : MonoBehaviour
    {
        public Rigidbody rb;

        public float damage;
        // Start is called before the first frame update
        private void OnTriggerEnter(Collider other)
        {

            if (other.TryGetComponent(out PlayerController player))
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}

using System;
using UnityEngine;

namespace Mechanics
{
    [RequireComponent(typeof(Rigidbody))]
    public class CollectorBullet : MonoBehaviour
    {
        public Rigidbody rb;
        // Start is called before the first frame update
        private void OnTriggerEnter(Collider other)
        {
            
            
            Destroy(gameObject);
        }
    }
}

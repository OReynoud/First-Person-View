using UnityEngine;

namespace Mechanics
{
    public class ArenaTrigger : MonoBehaviour
    {
        public Arena associatedArena;
        // Start is called before the first frame update

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Ex.Tag_Player))
            {
                associatedArena.TriggerArenaEvent();
        
        
                Destroy(gameObject);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mechanics
{
    public class AbsorbInk : ControllableProp
    {
        [HideInInspector] public float maxInk;
        [HideInInspector] public Vector3 baseScale;
        [HideInInspector] public DecalProjector decal;
        
        public float storedInk;
        [SerializeField] private bool respawn;
        [SerializeField] private float respawnTimer;
        
        
        // Start is called before the first frame update
        void Start()
        {
            maxInk = storedInk;
            decal = transform.GetChild(0).GetComponent<DecalProjector>();
            baseScale = decal.size;
        }

        // Update is called once per frame
    }
}

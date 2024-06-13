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
        public bool respawn;
        [SerializeField] private float respawnTimer;
        private float t;
        private bool isGettingAbsorbed;
        private BoxCollider boxCol;
        
        
        // Start is called before the first frame update
        void Start()
        {
            maxInk = storedInk;
            decal = transform.GetChild(0).GetComponent<DecalProjector>();
            baseScale = decal.size;
            boxCol = GetComponent<BoxCollider>();
        }

        public void StartAbsorbInk()
        {
            isGettingAbsorbed = true;
        }

        public void StopAbsorbing()
        {
            isGettingAbsorbed = false;
            t = respawnTimer;

            if (storedInk <= 0.5f)
            {
                boxCol.enabled = false;
            }
        }

        void Update()
        {
            if (!respawn) return;
            
            if (isGettingAbsorbed) return;

            t -= Time.deltaTime;

            if (t <= 0)
            {
                Respawn();
            }
        }

        void Respawn()
        {
            decal.size = baseScale;
            decal.fadeFactor = 1f;
            storedInk = maxInk;
            boxCol.enabled = true;
            
            t = respawnTimer;
        }
    }
}

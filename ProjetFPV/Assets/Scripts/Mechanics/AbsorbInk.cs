using System.Collections;
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
        private Coroutine cor;
        [SerializeField] private bool isADecal;
        
        
        // Start is called before the first frame update
        void Start()
        {
            t = respawnTimer;
            maxInk = storedInk;
            if (isADecal)
            {
                decal = transform.GetChild(0).GetComponent<DecalProjector>();
                baseScale = decal.size;
                boxCol = GetComponent<BoxCollider>();
            }
        }

        public void StartAbsorbInk()
        {
            StopCoroutine(cor);
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
                cor = StartCoroutine(Respawn());
            }
        }

        private IEnumerator Respawn()
        {
            t = respawnTimer;

            var x = 0f;
            boxCol.enabled = true;
            var currentSize = decal.size;
            var currentFade = decal.fadeFactor;
            var currentInk = storedInk;

            while (x < 2f)
            {
                x += Time.deltaTime;

                decal.size = Vector3.Lerp(currentSize, baseScale, x);
                decal.fadeFactor = Mathf.Lerp(currentFade, 1f, x);
                storedInk = Mathf.Lerp(currentInk, maxInk, x);

                yield return null;
            }
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace Mechanics
{
    public class AbsorbInk : ControllableProp
    {
        [HideInInspector] public float maxInk;
        [HideInInspector] public Vector3 baseDecalScale;
        [HideInInspector] public Vector3 baseGOScale;
        [HideInInspector] public DecalProjector decal;
        
        public float storedInk;
        public bool respawn;
        [SerializeField] private float respawnTimer;
        private float t;
        private bool isGettingAbsorbed;
        private BoxCollider boxCol;
        private Coroutine cor;
        public bool isADecal;
        
        
        // Start is called before the first frame update
        void Start()
        {
            t = respawnTimer;
            maxInk = storedInk;
            if (isADecal)
            {
                decal = transform.GetChild(0).GetComponent<DecalProjector>();
                baseDecalScale = decal.size;
                boxCol = GetComponent<BoxCollider>();
            }
            else
            {
                baseGOScale = transform.localScale;
            }
        }

        public void StartAbsorbInk()
        {
            if (!isADecal) return;
            if (cor is not null)
            {
                StopCoroutine(cor);
            }
            isGettingAbsorbed = true;
        }

        public void StopAbsorbing()
        {
            if (!isADecal) return;
            isGettingAbsorbed = false;
            t = respawnTimer;

            if (storedInk <= 0.5f)
            {
                boxCol.enabled = false;
            }
        }

        void Update()
        {
            if (!isADecal) return;
            
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

                decal.size = Vector3.Lerp(currentSize, baseDecalScale, x);
                decal.fadeFactor = Mathf.Lerp(currentFade, 1f, x);
                storedInk = Mathf.Lerp(currentInk, maxInk, x);

                yield return null;
            }
        }
    }
}

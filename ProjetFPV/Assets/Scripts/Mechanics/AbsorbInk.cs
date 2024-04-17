using UnityEngine;

namespace Mechanics
{
    public class AbsorbInk : ControllableProp
    {
        [HideInInspector] public float maxInk;
        public float storedInk;

        public Vector3 baseScale;
        // Start is called before the first frame update
        void Start()
        {
            maxInk = storedInk;
            baseScale = transform.localScale;
        }

        // Update is called once per frame
    }
}

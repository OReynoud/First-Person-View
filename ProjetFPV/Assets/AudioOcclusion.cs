using UnityEngine;

public class AudioOcclusion : MonoBehaviour
{
    private Transform player;
    private float distance;
    private Transform[] casters;
    private int[] collisions;
    private AudioLowPassFilter lowPass;
    [SerializeField] private AnimationCurve lowPassCurve;
    public float min;
    public float max;

    void Start()
    {
        lowPass = GetComponent<AudioLowPassFilter>();
        
        player = GameObject.FindGameObjectWithTag("Player").transform;

        var childCount = transform.childCount;
        
        casters = new Transform[childCount];
        collisions = new int[childCount];
        
        for (int i = 0; i < childCount; i++)
        {
            casters[i] = transform.GetChild(i);
        }
    }

    void Update()
    {
        distance = Vector3.Distance(transform.position, player.position);
        
        Debug.DrawRay(casters[0].position, player.position - casters[0].position, Color.green);
        Debug.DrawRay(casters[1].position, player.position - casters[1].position, Color.green);
        Debug.DrawRay(casters[2].position, player.position - casters[2].position, Color.green);
        Debug.DrawRay(casters[3].position, player.position - casters[3].position, Color.green);
        Debug.DrawRay(casters[4].position, player.position - casters[4].position, Color.green);
        
        if (distance <= 40)
        {
            RaycastHit hit0;
            RaycastHit hit1;
            RaycastHit hit2;
            RaycastHit hit3;
            RaycastHit hit4;

            if (Physics.Raycast(casters[0].position, player.position - casters[0].position, out hit0, distance - 1f))
            {
                collisions[0] = 1;
            }
            else
            {
                collisions[0] = 0;
            }
            
            if (Physics.Raycast(casters[1].position, player.position - casters[1].position, out hit1, distance - 1f))
            {
                collisions[1] = 1;
            }
            else
            {
                collisions[1] = 0;
            }
            
            if (Physics.Raycast(casters[2].position, player.position - casters[2].position, out hit2, distance - 1f))
            {
                collisions[2] = 1;
            }
            else
            {
                collisions[2] = 0;
            }
            
            if (Physics.Raycast(casters[3].position, player.position - casters[3].position, out hit3, distance - 1f))
            {
                collisions[3] = 1;
            }
            else
            {
                collisions[3] = 0;
            }
            
            if (Physics.Raycast(casters[4].position, player.position - casters[4].position, out hit4, distance - 1f))
            {
                collisions[4] = 1;
            }
            else
            {
                collisions[4] = 0;
            }

            var x = collisions[0] + collisions[1] + collisions[2] + collisions[3] + collisions[4];

            lowPass.cutoffFrequency = Mathf.Lerp(lowPass.cutoffFrequency, lowPassCurve.Evaluate(x) * max + min, 0.02f);
        }
    }
}
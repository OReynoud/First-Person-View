using UnityEngine;

public class AudioOcclusion : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform caster;
    [SerializeField] private AnimationCurve lowPassCurve;
    [SerializeField] private AnimationCurve highPassCurve;
    private float distance;
    private AudioLowPassFilter lowPass;
    private AudioHighPassFilter highPass;
    private float thickness;

    private AudioSubtitles subtitles;

    void Start()
    {
        subtitles = GetComponent<AudioSubtitles>();
        subtitles.StartTimer(0);
        
        lowPass = GetComponent<AudioLowPassFilter>();
        highPass = GetComponent<AudioHighPassFilter>();
    }

    void Update()
    {
        distance = Vector3.Distance(transform.position, player.position);
        
        if (distance <= 40)
        {
            RaycastHit hitFront;
            RaycastHit hitBack;

            if (Physics.Raycast(caster.position, player.position - caster.position, out hitFront, distance - 1f))
            {
                if (Physics.Raycast(player.position, caster.position - player.position, out hitBack, distance - 1f))
                {
                    thickness = Vector3.Distance(hitFront.point, hitBack.point);
                }
            }

            //subtitles.hasToDisplay = thickness < 3;
            
            highPass.cutoffFrequency = Mathf.Lerp(highPass.cutoffFrequency,highPassCurve.Evaluate(thickness) * 1000f - 200f, 0.1f);
            lowPass.cutoffFrequency = Mathf.Lerp(lowPass.cutoffFrequency,lowPassCurve.Evaluate(thickness) * 1000f + 200f, 0.1f);
        }
    }
}

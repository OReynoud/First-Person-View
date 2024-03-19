using UnityEngine;

public class TiltHead : MonoBehaviour
{
    public GameObject frontRC;
    public GameObject backRC;

    private bool objectInFront;
    private bool objectInBack;
    public Transform cam;

    [SerializeField] private Quaternion angle;
    [SerializeField] private float speed;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        var directionFront = frontRC.transform.forward * 1f;
        var directionBack = backRC.transform.forward * 1f;
        Gizmos.DrawRay(frontRC.transform.position, directionFront);
        Gizmos.DrawRay(backRC.transform.position, directionBack);
    }

    void Update()
    {
        RaycastHit hitFront;
        RaycastHit hitBack;

        objectInFront = Physics.Raycast(frontRC.transform.position, frontRC.transform.forward, out hitFront, 0.8f);
        
        objectInBack = Physics.Raycast(backRC.transform.position, backRC.transform.forward, out hitBack, 0.8f);
        
        if (objectInFront || objectInBack)
        {
            cam.localRotation = Quaternion.Lerp(cam.localRotation, angle, Mathf.SmoothStep(0.0f, 1.0f, 0.02f*speed));
        }
        else
        {
            cam.localRotation = Quaternion.Lerp(cam.localRotation, Quaternion.identity, Mathf.SmoothStep(0.0f, 1.0f, 0.02f*speed));
        }
        
    }
}

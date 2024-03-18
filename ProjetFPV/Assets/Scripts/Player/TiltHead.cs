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
        var directionFront = frontRC.transform.forward * 0.5f;
        var directionBack = backRC.transform.forward * 0.5f;
        Gizmos.DrawRay(frontRC.transform.position, directionFront);
        Gizmos.DrawRay(backRC.transform.position, directionBack);
    }

    void Update()
    {
        RaycastHit hitFront;
        RaycastHit hitBack;

        objectInFront = Physics.Raycast(frontRC.transform.position, frontRC.transform.forward, out hitFront, 0.5f);
        
        objectInBack = Physics.Raycast(backRC.transform.position, backRC.transform.forward, out hitBack, 0.5f);
        
        if (objectInFront || objectInBack)
        {
            cam.rotation = Quaternion.Lerp(cam.rotation, angle, 0.02f*speed);
        }
        else
        {
            cam.rotation = Quaternion.Lerp(cam.rotation, Quaternion.identity, 0.02f*speed);
        }
        
    }
}

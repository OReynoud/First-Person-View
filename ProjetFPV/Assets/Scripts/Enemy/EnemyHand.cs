using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHand : MonoBehaviour
{
    private Vector3 origin;

    private Vector3 wantedPos;
    private Rigidbody rb;

    public float handFrequency = 0.2f;
    public float handSpeed = 0.1f;
    public float maxDistance = 2;
    
    private float timer;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        origin = transform.localPosition;
        wantedPos = GetWantedPos();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var dir = wantedPos - transform.localPosition;
        rb.AddForce(dir.normalized * handSpeed);
        if (Vector3.Distance(origin, transform.localPosition) > maxDistance)
        {
            rb.velocity = -rb.velocity;
        }
        if (timer < handFrequency)
        {
            timer += Time.deltaTime;
            return;
        }
        

        timer = 0;
        wantedPos = GetWantedPos();
    }

    Vector3 GetWantedPos()
    {
        return origin + Random.insideUnitSphere * 1.5f;
    }
}

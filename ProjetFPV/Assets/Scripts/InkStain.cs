using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InkStain : MonoBehaviour
{
    private DecalProjector decal;
    private float t;
    
    void Start()
    {
        decal = GetComponent<DecalProjector>();
        decal.fadeFactor = Random.Range(0.85f, 1f);
        decal.size *= Random.Range(0.8f, 1.2f);
        t = 2f;
        Destroy(gameObject, 8f);
    }

    void Update()
    {
        if (t >= 0)
        {
            t -= Time.deltaTime;
            return;
        }
        
        decal.fadeFactor -= 0.5f * Time.deltaTime;
    }
    
    

    
    
}

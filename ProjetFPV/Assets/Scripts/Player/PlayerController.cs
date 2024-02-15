using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    [SerializeField] private Camera playerCam;

    [SerializeField] private Vector3 cameraOffset;

    [SerializeField] private bool autoStabiliseVertical = true;
    
    // Start is called before the first frame update

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + cameraOffset, 0.2f);
    }

    public override void Awake()
    {
        base.Awake();
        playerCam = Camera.main;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

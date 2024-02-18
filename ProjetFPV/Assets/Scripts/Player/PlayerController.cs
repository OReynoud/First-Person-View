using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Singleton<PlayerController>
{
    [SerializeField] private Camera playerCam;

    [SerializeField] private Vector3 cameraOffset;

    [SerializeField] private bool autoStabiliseVertical = true;
    [SerializeField] private bool independentCam = true;
    [SerializeField] private float verticalSpeedModifier;
    [SerializeField] private float horizontalSpeedModifier;
    [SerializeField] private float rotationSpeed;
    [SerializeField] [Range(0,1)]private float drag;
    private bool moveCam = false;
    [SerializeField] private Transform playerModel;
    private Rigidbody rb;

    public InputMapping controls;
    private InputActionMap currentControls;
    [Dropdown("GetInputMaps")]
    public string currentInputMap;

    public PlayerInput inputs;

    private Vector3 playerDir;

    private bool appliedForce;
    // Start is called before the first frame update

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position + cameraOffset, 0.2f);
    }

    private void OnValidate()
    {
        inputs = GetComponent<PlayerInput>();
    }

    List<string> GetInputMaps()
    {
        List<string> temp = new List<string>();

        for (int i = 0; i < inputs.actions.actionMaps.Count; i++)
        {
            temp.Add(inputs.actions.actionMaps[i].name);
        }

        return temp;
    }
    public override void Awake()
    {
        base.Awake();
        playerCam = Camera.main;
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        inputs = GetComponent<PlayerInput>();
        inputs.actions.Enable();
        currentControls = inputs.actions.FindActionMap(currentInputMap);
        Debug.Log(currentControls);
        currentControls.Enable();
        //currentControls.FindAction("Test",true).Enable();
        currentControls.FindAction("Toggle Vertical Stabilize",true).performed += ToggleVerticalStabilize;
        currentControls.FindAction("Toggle Independent Camera",true).performed += ToggleIndependentCamera;
        currentControls.FindAction("Toggle Move Camera",true).performed += ToggleMoveCamera;
        currentControls.FindAction("Toggle Move Camera",true).canceled += ToggleMoveCamera;

    }

    private void ToggleMoveCamera(InputAction.CallbackContext obj)
    {
        moveCam = !moveCam;
    }

    private void ToggleIndependentCamera(InputAction.CallbackContext obj)
    {
        independentCam = !independentCam;
    }

    private void ToggleVerticalStabilize(InputAction.CallbackContext obj)
    {
        autoStabiliseVertical = !autoStabiliseVertical;
    }

    // Update is called once per frame
    void Update()
    {
        playerDir = Vector3.zero;
        if (currentControls.FindAction("Test", true).IsPressed())
        {
            Debug.Log("Testing");
        }
        UpAndDown();
        ForwardMovement();
        SidewaysMovement();
        Rotate();
        
    }

    private void FixedUpdate()
    {
        playerDir.Normalize();
        
        rb.AddForce(playerDir);

        if (!appliedForce)
        {
            if (autoStabiliseVertical)
            {
                rb.AddForce(0,9.81f,0);
                Debug.Log("oui?");
            }
            
            rb.velocity = new Vector3(rb.velocity.x * drag, rb.velocity.y * drag, rb.velocity.z * drag);
            
        }
        appliedForce = false;
    }

    #region Controls

    void UpAndDown()
    {
        if (currentControls.FindAction("ThrottleUp", true).IsPressed())
        {
            playerDir += Vector3.up;
            appliedForce = true;
        }
        if (currentControls.FindAction("ThrottleDown", true).IsPressed())
        {
            playerDir += Vector3.down;
            appliedForce = true;
        }
    }

    void ForwardMovement()
    {
        if (currentControls.FindAction("ForwardTilt", true).IsPressed())
        {
            playerDir += transform.forward;
            appliedForce = true;
        }
        if (currentControls.FindAction("BackwardTilt", true).IsPressed())
        {
            playerDir -= transform.forward;
            appliedForce = true;
        }
    }

    void SidewaysMovement()
    {
        if (currentControls.FindAction("RightRoll", true).IsPressed())
        {
            playerDir += transform.right;
            appliedForce = true;
        }
        if (currentControls.FindAction("LeftRoll", true).IsPressed())
        {
            playerDir -= transform.forward;
            appliedForce = true;
        }
    }

    void Rotate()
    {
        if (currentControls.FindAction("YawRight", true).IsPressed())
        {
            transform.rotation = Quaternion.Euler(0,rotationSpeed,0);
        }
        if (currentControls.FindAction("YawLeft", true).IsPressed())
        {
            transform.rotation = Quaternion.Euler(0,-rotationSpeed,0);
        }
    }
    

    #endregion
}

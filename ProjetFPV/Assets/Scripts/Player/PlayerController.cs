using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;

public class PlayerController : Singleton<PlayerController>
{
    [BoxGroup("Refs")][SerializeField] private Transform playerCam;

//    [SerializeField] private Vector3 cameraOffset;

    private bool moveCam = false;
    private Rigidbody rb;

    private InputActionMap currentControls;
    [Dropdown("GetInputMaps")]
    [BoxGroup("Refs")]public string currentInputMap;

    [BoxGroup("Refs")]public PlayerInput inputs;


    // Start is called before the first frame update
    

    [BoxGroup("Movement")][Tooltip("Acceleration when walking")][SerializeField] private float walkAccel;
    [BoxGroup("Movement")][Tooltip("Acceleration when running")][SerializeField] private float runAccel;
    [BoxGroup("Movement")][Tooltip("Force applied on a jump")][SerializeField] private float jumpForce;
    [BoxGroup("Movement")][Tooltip("Maximum Horizontal Velocity when walking")][SerializeField] private float maxHorizontalVelocity;
    [BoxGroup("Movement")][Tooltip("Multiplies with maxHorizontalVelocity when running")][SerializeField] private float runMaxVelocityFactor;
    [BoxGroup("Movement")][Tooltip("To make a jump more or less floaty")][SerializeField] private AnimationCurve jumpCurve;
    [BoxGroup("Movement")][Tooltip("Camera sensitivity")][SerializeField] private float lookSpeed;
    [BoxGroup("Movement")][Tooltip("Limit to the camera being able to look up or down")][SerializeField] private float lookXLimit;
    [BoxGroup("Movement")][Tooltip("How effective horizontal movement is in the air")][SerializeField][Range(0,1)] private float airMobilityFactor;
    [BoxGroup("Movement")][Tooltip("How much fast the avatar slows down when no inputs are given")][SerializeField] [Range(0,1)]private float drag;
    
    [Foldout("Debug")][Tooltip("")][SerializeField]private bool appliedForce;
    [Foldout("Debug")][Tooltip("")][SerializeField] private bool canMove = true;
    [Foldout("Debug")][Tooltip("")][SerializeField] private bool isRunning = false;
    [Foldout("Debug")][Tooltip("")][SerializeField] private bool isJumping = false;
    [Foldout("Debug")][Tooltip("")][SerializeField] private bool isGrounded = false;


    private float rotationX;
    private Vector3 playerDir;
    // private void OnDrawGizmosSelected()
    // {
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawSphere(transform.position + cameraOffset, 0.2f);
    // }

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
        currentControls.FindAction("Jump",true).performed += Jump;
        
        currentControls.FindAction("ToggleSprint",true).performed += ToggleSprint;
        currentControls.FindAction("ToggleSprint",true).canceled += ToggleSprint;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    


    private Vector2Control lastmousePos = new Vector2Control();
    // Update is called once per frame
    void Update()
    {
        
        playerDir = Vector3.zero;
        ForwardMovement();
        SidewaysMovement();
        Rotate();
        playerCam.position = Vector3.Lerp(playerCam.position, transform.position, 0.8f);
        playerCam.position = new Vector3(playerCam.position.x, transform.position.y + 0.5f,
            playerCam.position.z);
        if (Physics.Raycast(transform.position,Vector3.down, out RaycastHit hit, 1.1f,LayerMask.GetMask("Ground")) && !isJumping)
        {
            isGrounded = true;
        }
        
    }

    private float jumpTime;
    private Vector2 horizontalVelocity;
    private void FixedUpdate()
    {
        playerDir.Normalize();
        //expectedForce = playerDir * (runSpeed or walkSpeed, depending on the state of isRunning) * (1 or airMobilityFactor[value between 0 and 1] depending on the player being grounded)
        var expectedForce = playerDir * ((isRunning ? runAccel : walkAccel) * (isGrounded ? 1 : airMobilityFactor));

        var dirDiff = Vector3.Angle(playerDir, rb.velocity.normalized);
        Debug.Log(dirDiff);
        var finalDir = Vector3.Lerp(expectedForce, rb.velocity, (1 - (dirDiff / 180))) * drag;
        rb.velocity = new Vector3(finalDir.x,rb.velocity.y,finalDir.z);
        
        
        if (expectedForce.magnitude < 1)
        {
            appliedForce = false;
        }
        rb.AddForce(expectedForce);
        if (!appliedForce)
        {
            rb.velocity = new Vector3(rb.velocity.x * drag, rb.velocity.y, rb.velocity.z * drag);
        }

        if (isJumping)
        {
            jumpTime += Time.deltaTime;
            rb.AddForce(Vector3.up * jumpForce * jumpCurve.Evaluate(jumpTime));
        }

        if (jumpTime > jumpCurve.keys[^1].time)
        {
            isJumping = false;
            jumpTime = 0;
        }

        horizontalVelocity = new Vector2(rb.velocity.x, rb.velocity.z);
        if (!isRunning)
        {
            horizontalVelocity = Vector2.ClampMagnitude(horizontalVelocity , maxHorizontalVelocity);
        }
        else
        {
            horizontalVelocity = Vector2.ClampMagnitude(horizontalVelocity , maxHorizontalVelocity * runMaxVelocityFactor);
        }
        //Clamp horizontal speed with a min and max speed
        rb.velocity = new Vector3(horizontalVelocity.x,rb.velocity.y,horizontalVelocity.y);
    }

    #region Controls
    
    void ForwardMovement()
    {
        if (currentControls.FindAction("Forward", true).IsPressed())
        {
            playerDir += transform.forward;
            appliedForce = true;
        }
        if (currentControls.FindAction("Backward", true).IsPressed())
        {
            playerDir -= transform.forward;
            appliedForce = true;
        }
    }

    void SidewaysMovement()
    {
        if (currentControls.FindAction("Right", true).IsPressed())
        {
            playerDir += transform.right;
            appliedForce = true;
        }
        if (currentControls.FindAction("Left", true).IsPressed())
        {
            playerDir -= transform.right;
            appliedForce = true;
        }
    }


    void Rotate()
    {
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCam.localEulerAngles = new Vector3(rotationX,playerCam.localEulerAngles.y,playerCam.localEulerAngles.z);
        // float angle = Mathf.Atan2(transform.rotation.z,transform.rotation.x) * Mathf.Rad2Deg;
        // if (angle < 0) angle += 360;
        // if (angle > 360) angle -= 360;
        // angle += Input.GetAxis("Mouse X") * lookSpeed;
        // transform.localEulerAngles += new Vector3(0,angle,0);
        // playerCam.transform.localEulerAngles = new Vector3(playerCam.transform.localEulerAngles.x,angle,playerCam.transform.localEulerAngles.z);
        transform.rotation *= Quaternion.Euler(0,Input.GetAxis("Mouse X") * lookSpeed,0);
        playerCam.localEulerAngles = new Vector3(playerCam.localEulerAngles.x,transform.localEulerAngles.y,playerCam.localEulerAngles.z);

    }
    
    private void Jump(InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            isGrounded = false;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = true;
        }
    }
    
    private void ToggleSprint(InputAction.CallbackContext obj)
    {
        isRunning = !isRunning;
    }
    
    #endregion
}

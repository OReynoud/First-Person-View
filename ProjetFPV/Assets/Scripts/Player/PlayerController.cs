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
    [BoxGroup("Refs")]public LineRenderer shootTrail;
    
    [BoxGroup("Refs")] [SerializeField] private Transform hands;
    [BoxGroup("Refs")] [SerializeField] private Transform leftHand;
    [BoxGroup("Refs")] [SerializeField] private Transform rightHand; 
    [Foldout("Debug")][Tooltip("")][SerializeField] private Transform shootingHand;


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
    
    [BoxGroup("Shoot")][Tooltip("")][SerializeField]private LayerMask shootMask;
    [BoxGroup("Shoot")][Tooltip("")][SerializeField]private int bulletDmg;
    [BoxGroup("Shoot")][Tooltip("")][SerializeField]private int magSize;
    [BoxGroup("Shoot")][Tooltip("")][SerializeField]private float shootSpeed;
    [BoxGroup("Shoot")][Tooltip("")][SerializeField]private float maxRange;
    [BoxGroup("Shoot")][Tooltip("")][SerializeField]private float trailTime;
    
    

    [BoxGroup("Bobbing")] [SerializeField] private float amplitude;
    [BoxGroup("Bobbing")] [SerializeField] private float frequeny;
    [BoxGroup("Bobbing")] [SerializeField] private bool bobbing;
    [BoxGroup("Bobbing")] [SerializeField] private float toggleSpeed;
    [BoxGroup("Bobbing")] [SerializeField] private Vector3 startPos;
    [BoxGroup("Bobbing")] [SerializeField] private float sideTilting;
    
    [Foldout("Debug")][Tooltip("")][SerializeField]private bool appliedForce;
    [Foldout("Debug")][Tooltip("")][SerializeField] private bool canMove = true;
    [Foldout("Debug")][Tooltip("")][SerializeField] private bool isRunning = false;
    [Foldout("Debug")][Tooltip("")][SerializeField] private bool isJumping = false;
    [Foldout("Debug")][Tooltip("")][SerializeField] private bool isGrounded = false;
    [Foldout("Debug")][Tooltip("")][SerializeField] private float shootReload;
    
    
    


    private float rotationX;
    private Vector3 playerDir;
    private float jumpTime;
    private Vector2 horizontalVelocity;
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

    private void OnDisable()
    {
        currentControls.Disable();
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
        currentControls.FindAction("Shoot",true).performed += Shoot;
        //currentControls.FindAction("Telekinesis",true).performed += ;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        startPos = hands.localPosition;
        
        CheckShootingHand();
    }
    
    void CheckShootingHand()
    {
        if (currentControls.FindAction("Shoot",true).bindings[0].path == "<Mouse>/leftButton")
        {
            Debug.Log("Shooting with left hand");
            shootingHand = leftHand;
        }
        else
        {
            
            Debug.Log("Shooting with right hand");
            shootingHand = rightHand;
        }
    }

    private LineRenderer currentTrail;
    private void Shoot(InputAction.CallbackContext obj)
    {
        if (shootReload > 0) return;
        shootReload = shootSpeed;
        currentTrail = Instantiate(shootTrail);
        Destroy(currentTrail.gameObject,trailTime);
        currentTrail.SetPosition(0,shootingHand.position);
        if (Physics.Raycast(playerCam.position,playerCam.forward,out RaycastHit hit, maxRange, shootMask))
        {
            Debug.Log("Hit something");
            /*if (hit.collider.TryGetComponent(typeof(Enemy) out enemy))
            {
                //enemy.TakeDamage();
            }
            if (hit.collider.TryGetComponent(typeof(Destructible) out target))
            {
                //target.GetHit();
            }*/
            
            currentTrail.SetPosition(1,hit.point);
        }
        else
        {
            Debug.Log("Hit some air");
            currentTrail.SetPosition(1,playerCam.forward * maxRange);
        }
        
    }


    private Vector2Control lastmousePos = new Vector2Control();
    // Update is called once per frame
    void Update()
    {
        
        playerDir = Vector3.zero;
        ForwardInput();
        SidewaysInput();
        Rotate();
        ArmBobbing();

        if (shootReload >= 0)
        {
            shootReload -= Time.deltaTime;
        }
        
        playerCam.position = Vector3.Lerp(playerCam.position, transform.position, 0.8f);
        playerCam.position = new Vector3(playerCam.position.x, transform.position.y + 0.5f,
            playerCam.position.z);
        if (Physics.Raycast(transform.position,Vector3.down, out RaycastHit hit, 1.1f,LayerMask.GetMask("Ground")) && !isJumping)
        {
            isGrounded = true;
        }
        
    }

    private void FixedUpdate()
    {
        HorizontalMovement();
        VerticalMovement();
        
        ClampVelocity();

    }

    



    #region Physics

    private void HorizontalMovement()
    {
        playerDir.Normalize();

        var expectedForce = playerDir * ((isRunning ? runAccel : walkAccel) * (isGrounded ? 1 : airMobilityFactor));

        var dirDiff = Vector3.Angle(playerDir, rb.velocity.normalized);
        var finalDir = Vector3.Lerp(expectedForce, rb.velocity, (1 - (dirDiff / 180))) * drag;
        rb.velocity = new Vector3(finalDir.x, rb.velocity.y, finalDir.z);


        if (expectedForce.magnitude < 1)
        {
            appliedForce = false;
        }

        rb.AddForce(expectedForce);
        if (!appliedForce)
        {
            rb.velocity = new Vector3(rb.velocity.x * drag, rb.velocity.y, rb.velocity.z * drag);
        }
    }
    
    private void VerticalMovement()
    {
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
    }
    private void ClampVelocity()
    {
        if (!isRunning)
        {
            horizontalVelocity = Vector2.ClampMagnitude(horizontalVelocity, maxHorizontalVelocity);
        }
        else
        {
            horizontalVelocity = Vector2.ClampMagnitude(horizontalVelocity, maxHorizontalVelocity * runMaxVelocityFactor);
        }

        //Clamp horizontal speed with a min and max speed
        rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.y);
    }
    #endregion

    #region Controls
    
    void ForwardInput()
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

    void SidewaysInput()
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
        
        transform.rotation *= Quaternion.Euler(0,Input.GetAxis("Mouse X") * lookSpeed,0);
        var camRotation = new Vector3(playerCam.localEulerAngles.x, transform.localEulerAngles.y, playerCam.localEulerAngles.z);
        playerCam.localEulerAngles = Vector3.Lerp(playerCam.localEulerAngles,camRotation,0.8f);

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

    #region Bobbing

    Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y += Mathf.Sin(Time.time * frequeny) * amplitude;
        //pos.x += Mathf.Cos(Time.time * frequeny/2) * amplitude;
        
        return pos;
    }
    
    private void PlayMotion(Vector3 motion)
    {
        hands.localPosition += motion;
    }

    void ResetBobbing()
    {
        hands.localPosition = Vector3.Lerp(hands.localPosition, startPos, 1 * Time.deltaTime);
    }
    private void ArmBobbing()
    {
        if (appliedForce && isGrounded)
        {
            PlayMotion(FootStepMotion());
        }
        else
        {
            ResetBobbing();
        }
    }
    #endregion
}

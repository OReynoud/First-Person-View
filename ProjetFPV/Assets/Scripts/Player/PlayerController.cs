using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;

public class PlayerController : Singleton<PlayerController>
{
//    [SerializeField] private Vector3 cameraOffset;

    private bool moveCam = false;
    private Rigidbody rb;

    private InputActionMap currentControls;

    #region Deprecated

    [Foldout("Obsolete")] [Tooltip("Acceleration when running")] [SerializeField]
    private float runAccel;
    [Foldout("Obsolete")] [Tooltip("Multiplies with maxHorizontalVelocity when running")] [SerializeField]
    private float runMaxVelocityFactor;

    [Foldout("Obsolete")] [Tooltip("To make a jump more or less floaty")] [SerializeField]
    private AnimationCurve jumpCurve;
    [Foldout("Obsolete")] [Tooltip("CameraFOV when player is running")] [SerializeField]
    private float runningFOV;
    #endregion
    
    #region Refs

    [Dropdown("GetInputMaps")] [BoxGroup("Refs")]
    public string currentInputMap;

    [Foldout("Refs")] public PlayerInput inputs;
    [Foldout("Refs")] public LineRenderer shootTrail;
    [Foldout("Refs")] [SerializeField] private Transform playerCam;
    [Foldout("Refs")] [SerializeField] private Transform hands;
    [Foldout("Refs")] [SerializeField] private Transform leftHand;
    [Foldout("Refs")] [SerializeField] private Transform rightHand;
    [Foldout("Refs")] [SerializeField] private RectTransform telekinesisPointer;
    [Foldout("Refs")] [SerializeField] private Transform offsetPosition;
    [Foldout("Debug")] [SerializeField] private Transform shootingHand;

    #endregion

    #region Movement Variables

    [Foldout("Movement")] [Tooltip("Acceleration when walking")] [SerializeField]
    private float walkAccel;



    [Foldout("Movement")] [Tooltip("Force applied on a jump")] [SerializeField]
    private float jumpForce;

    [Foldout("Movement")] [Tooltip("Maximum Horizontal Velocity when walking")] [SerializeField]
    private float maxHorizontalVelocity;



    [Foldout("Movement")] [Tooltip("Camera sensitivity")] [SerializeField]
    private float lookSpeed;

    [Foldout("Movement")] [Tooltip("Limit to the camera being able to look up or down")] [SerializeField]
    private float lookXLimit;

    [Foldout("Movement")] [Tooltip("How effective horizontal movement is in the air")] [SerializeField] [Range(0, 1)]
    private float airMobilityFactor;

    [Foldout("Movement")]
    [Tooltip("How much fast the avatar slows down when no inputs are given")]
    [SerializeField]
    [Range(0, 1)]
    private float drag;

    [Foldout("Movement")] [Tooltip("CameraFOV when Player is walking")] [SerializeField]
    private float normalFOV;


    [Foldout("Movement")] [Tooltip("How fast the FOV changes")] [SerializeField] [Range(0, 1)]
    private float lerpFOV;

    #endregion

    #region Shoot variables

    [Foldout("Shoot")] [Tooltip("Which layers will get hit by the hit scan")] [SerializeField]
    private LayerMask shootMask;

    [Foldout("Shoot")] [Tooltip("Base damage of a bullet")] [SerializeField]
    private int bulletDmg;
    
    [Foldout("Shoot")] [Tooltip("How much ammo can the player carry? (Current mag not included)")] [SerializeField]
    private int maxStoredAmmo;

    [Foldout("Shoot")] [Tooltip("(Not yet used) Max ammo before player has to reload")] [SerializeField]
    public int magSize;

    [Foldout("Shoot")] [Tooltip("How fast player can shoot")] [SerializeField]
    private float shootSpeed;
    
    [Foldout("Shoot")] [Tooltip("Time needed to reload")] [SerializeField]
    private float reloadSpeed;

    [Foldout("Shoot")] [Tooltip("Range of the hit scan")] [SerializeField]
    private float maxRange;

    [Foldout("Shoot")] [Tooltip("How long the trail of the shot stays visible")] [SerializeField]
    private float trailTime;

    [Foldout("Shoot")] [Tooltip("base knockback inflicted on enemies")] [SerializeField]
    private float baseKnockBack;

    #endregion

    #region Telekinesis Variables

    [Foldout("Telekinesis")]
    [InfoBox(
        "Blue ball indicates the resting Position (Must be updated manually in editor via the button 'Update Resting Pos' at the bottom, otherwise updates automatically in play mode)")]
    [Label("RestingPos")]
    [SerializeField]
    private Vector3 restingPosOffset;


    [Foldout("Telekinesis")] [Tooltip("How fast the targeted object travels to the resting position")] [SerializeField]
    private float travelSpeed;

    [Foldout("Telekinesis")]
    [Tooltip(
        "*KEEP THIS VARIABLE LOW* Minimum distance between the grabbed object and the resting position before the object is considered as 'grabbed'")]
    [SerializeField]
    private float grabDistanceBuffer;

    [Foldout("Telekinesis")] [Tooltip("Maximum stamina of the player")] [SerializeField]
    private float maxStamina;

    [Foldout("Telekinesis")]
    [Tooltip("How much stamina the player currently has")]
    [SerializeField]
    [ProgressBar("maxStamina", EColor.Green)]
    private float currentStamina;

    [Foldout("Telekinesis")]
    [Tooltip("How fast stamina regenerates when player is not using telekinesis")]
    [SerializeField]
    private float staminaRegen;

    [Foldout("Telekinesis")] [Tooltip("Cost per second of using telekinesis on an object")] [SerializeField]
    private float holdObjectCost;

    [Foldout("Telekinesis")] [Tooltip("Cost per second of using telekinesis on an enemy")] [SerializeField]
    private float holdEnemyCost;

    [Foldout("Telekinesis")] [Tooltip("Cost of releasing an object from telekinesis")] [SerializeField]
    private float throwCost;

    [Foldout("Telekinesis")]
    [Tooltip("Force applied to grabbed object when released from telekinesis")]
    [SerializeField]
    private float throwForce;

    #endregion

    #region Bobbing Variables

    [Foldout("Bobbing")] [Tooltip("How big the arm bobbing is")] [SerializeField]
    private float amplitude;

    [Foldout("Bobbing")] [Tooltip("Speed of the arm bobbing")] [SerializeField]
    private float frequeny;

    [Foldout("Bobbing")] [Tooltip("(NOT USED YET) Minimum velocity for bobbing to start")] [SerializeField]
    private float toggleSpeed;

    [Foldout("Bobbing")]
    [Tooltip("(NOT USED YET) Camera tilting (in degrees) when player is moving left or right")]
    [SerializeField]
    private float sideTilting;

    #endregion

    #region Debug

    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    private bool appliedForce;

    [Foldout("Debug")] [SerializeField] private bool bobbing;

    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    private bool canMove = true;

    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    private bool isRunning = false;

    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    private bool isJumping = false;

    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    private bool isGrounded = false;

    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    private float shootSpeedTimer;

    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    private ControllableProp controlledProp;
    
    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    public int currentAmmo;
    
    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    public int inventoryAmmo;

    #endregion

    #region Misc

    private float rotationX;
    private Vector3 playerDir;
    private float jumpTime;
    private Vector2 horizontalVelocity;
    private Vector3 startPos;

    #endregion

    [Button]
    void UpdateRestingPos()
    {
        if (playerCam.forward.y < 0)
        {
            offsetPosition.position = transform.position;
            offsetPosition.localPosition += restingPosOffset;
        }
        else
        {
            offsetPosition.position = transform.position;
            offsetPosition.localPosition += new Vector3(restingPosOffset.x,
                restingPosOffset.y * (playerCam.forward.y + 1), restingPosOffset.z * (playerCam.forward.z));
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        //offsetPosition.position = restingPosOffset;
        Gizmos.DrawSphere(offsetPosition.position, 0.2f);
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
        rb = GetComponent<Rigidbody>();
        camera1 = Camera.main;
        inputs = GetComponent<PlayerInput>();
        inputs.actions.Enable();
        currentControls = inputs.actions.FindActionMap(currentInputMap);
        Debug.Log(currentControls);
        currentControls.Enable();
        //currentControls.FindAction("Test",true).Enable();
        currentControls.FindAction("Jump", true).performed += Jump;
        currentControls.FindAction("ToggleSprint", true).performed += ToggleSprint;
        currentControls.FindAction("ToggleSprint", true).canceled += ToggleSprint;
        currentControls.FindAction("Shoot", true).performed += Shoot;
        currentControls.FindAction("Telekinesis", true).canceled += ReleaseProp;
        currentControls.FindAction("Reload", true).performed += Reload;
        
        //currentControls.FindAction("Telekinesis",true).performed += ;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        startPos = hands.localPosition;
        currentStamina = maxStamina;
        currentAmmo = magSize;
        inventoryAmmo = maxStoredAmmo;
    }

    private void OnDisable()
    {
        currentControls.Disable();
    }

    void Start()
    {

        CheckShootingHand();
    }

    private Coroutine reloadCoroutine;
    private void Reload(InputAction.CallbackContext obj)
    {
        Debug.Log(reloadCoroutine);
        if (reloadCoroutine != null || currentAmmo == magSize || inventoryAmmo == 0)
            return;
        
        reloadCoroutine = StartCoroutine(Reload2());
        
    }
    
    private const float reloadHandMove = 1f;
    private IEnumerator Reload2()
    {
        shootingHand.DOMove(shootingHand.position - hands.up * reloadHandMove, 0.4f);
        yield return new WaitForSeconds(reloadSpeed);
        shootingHand.DOMove(shootingHand.position + hands.up * reloadHandMove, 0.4f);
        if (inventoryAmmo < magSize - currentAmmo)
        {
            currentAmmo = inventoryAmmo;
            inventoryAmmo = 0;
        }
        else
        {
            inventoryAmmo -= magSize - currentAmmo;
            currentAmmo = magSize;
        }

        reloadCoroutine = null;
    }


    void CheckShootingHand()
    {
        if (currentControls.FindAction("Shoot", true).bindings[0].path == "<Mouse>/leftButton")
        {
            Debug.Log("Shooting with left hand");
            shootingHand = leftHand;
        }
        else
        {
            Debug.Log("Shooting with right hand");
            shootingHand = rightHand;
            restingPosOffset = new Vector3(-restingPosOffset.x, restingPosOffset.y, restingPosOffset.z);
        }

        UpdateRestingPos();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRestingPos();
        playerDir = Vector3.zero;

        isGrounded = GroundCheck();

        ForwardInput();
        SidewaysInput();
        Rotate();
        ArmBobbing();
        TelekinesisInput();

        if (shootSpeedTimer >= 0)
        {
            shootSpeedTimer -= Time.deltaTime;
        }

        playerCam.position = Vector3.Lerp(playerCam.position, transform.position, 0.8f);
        playerCam.position = new Vector3(playerCam.position.x, transform.position.y + 0.5f,
            playerCam.position.z);


        if (!controlledProp)
        {
            CheckTelekinesisTarget();

            currentStamina =
                GameManager.instance.UpdatePlayerStamina(currentStamina, maxStamina, Time.deltaTime * staminaRegen);
        }
        else if (telekinesisPointer.gameObject.activeSelf)
            telekinesisPointer.gameObject.SetActive(false);

        if (isRunning && isGrounded)
        {
            camera1.fieldOfView = Mathf.Lerp(camera1.fieldOfView, runningFOV, lerpFOV);
        }
        else
        {
            camera1.fieldOfView = Mathf.Lerp(camera1.fieldOfView, normalFOV, lerpFOV);
        }
    }

    private bool GroundCheck()
    {
        bool check = false;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.1f, LayerMask.GetMask("Ground")))
        {
            if (!isJumping)
            {
                check = true;
            }
        }
        else
        {
            check = false;
        }

        return check;
    }

    private void CheckTelekinesisTarget()
    {
        if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit hit, maxRange, shootMask)
            && hit.collider.TryGetComponent(out ControllableProp prop))
        {
            if (!telekinesisPointer.gameObject.activeSelf)
                telekinesisPointer.gameObject.SetActive(true);

            telekinesisPointer.position = Camera.main.WorldToScreenPoint(prop.transform.position);
            telekinesisPointer.rotation *= Quaternion.Euler(0, 0, 2);
            return;
        }

        if (telekinesisPointer.gameObject.activeSelf)
            telekinesisPointer.gameObject.SetActive(false);
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
            horizontalVelocity =
                Vector2.ClampMagnitude(horizontalVelocity, maxHorizontalVelocity * runMaxVelocityFactor);
        }

        //Clamp horizontal speed with a min and max speed
        rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.y);
    }


    private void TelekinesisPhysics()
    {
        switch (controlledProp)
        {
            case TelekinesisObject:

                currentStamina =
                    GameManager.instance.UpdatePlayerStamina(currentStamina, maxStamina,
                        Time.deltaTime * -holdObjectCost);

                var dir = offsetPosition.position - controlledProp.transform.position;
                dir.Normalize();
                if (!controlledProp.isGrabbed)
                {
                    controlledProp.body.velocity = dir * travelSpeed;
                }
                else
                {
                    controlledProp.body.velocity = dir * (travelSpeed *
                                                          (Vector3.Distance(controlledProp.transform.position,
                                                              offsetPosition.position) / grabDistanceBuffer));
                }

                if (grabDistanceBuffer > Vector3.Distance(controlledProp.transform.position, offsetPosition.position))
                {
                    controlledProp.isGrabbed = true;
                }

                break;
            case Enemy:
                break;
        }

        if (currentStamina < 0)
        {
            controlledProp.ApplyTelekinesis();
            controlledProp.isGrabbed = false;
            controlledProp = null;
        }
    }


    private void ReleaseProp(InputAction.CallbackContext obj)
    {
        if (!controlledProp) return;
        controlledProp.ApplyTelekinesis();
        if (!controlledProp.isGrabbed)
        {
            controlledProp.body.velocity *= 0.1f;
            controlledProp = null;
            return;
        }

        controlledProp.isGrabbed = false;

        currentStamina =
            GameManager.instance.UpdatePlayerStamina(currentStamina, maxStamina, -throwCost);

        if (currentStamina < 0) currentStamina = 0;


        controlledProp.body.velocity = Vector3.zero;
        controlledProp.body.AddForce(playerCam.forward * throwForce, ForceMode.Impulse);
        controlledProp = null;
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
        playerCam.localEulerAngles = new Vector3(rotationX, playerCam.localEulerAngles.y, playerCam.localEulerAngles.z);

        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        var camRotation = new Vector3(playerCam.localEulerAngles.x, transform.localEulerAngles.y,
            playerCam.localEulerAngles.z);
        //playerCam.localEulerAngles = Vector3.Lerp(playerCam.localEulerAngles, camRotation, 0.8f);
        playerCam.localEulerAngles = camRotation;
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


    private LineRenderer currentTrail;
    private Camera camera1;

    private void Shoot(InputAction.CallbackContext obj)
    {
        if (shootSpeedTimer > 0) return;
        if (currentAmmo == 0) return;
        currentAmmo--;
        shootSpeedTimer = shootSpeed;
        currentTrail = Instantiate(shootTrail);
        Destroy(currentTrail.gameObject, trailTime);
        currentTrail.SetPosition(0, shootingHand.position);
        if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit hit, maxRange, shootMask))
        {
            Debug.Log("Hit something");
            if (hit.collider.CompareTag("Head"))
            {
                if (hit.collider.transform.parent.TryGetComponent(out Enemy enemy))
                {
                    enemy.TakeDamage(bulletDmg, true, baseKnockBack, transform.forward);
                    GameManager.instance.HitMark(true);
                }
            }

            if (hit.collider.CompareTag("Body"))
            {
                if (hit.collider.transform.parent.TryGetComponent(out Enemy enemy))
                {
                    enemy.TakeDamage(bulletDmg, false, baseKnockBack, transform.forward);
                    GameManager.instance.HitMark(false);
                }
            }

            if (hit.collider.TryGetComponent(out Destructible target))
            {
                target.GetHit();
            }

            currentTrail.SetPosition(1, hit.point);
        }
        else
        {
            Debug.Log("Hit some air");
            currentTrail.SetPosition(1, playerCam.forward * maxRange);
        }
    }

    private void TelekinesisInput()
    {
        if (currentStamina < throwCost * 1.5f) return;

        if (currentControls.FindAction("Telekinesis", true).IsPressed())
        {
            if (!controlledProp)
            {
                if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit hit, maxRange, shootMask))
                {
                    if (hit.collider.TryGetComponent(out ControllableProp prop))
                    {
                        controlledProp = prop;
                        prop.ApplyTelekinesis();
                    }
                }

                return;
            }

            TelekinesisPhysics();
        }
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
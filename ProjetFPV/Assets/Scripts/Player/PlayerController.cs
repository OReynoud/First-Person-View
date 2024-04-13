using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mechanics;
using NaughtyAttributes;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class PlayerController : Singleton<PlayerController>
{
//    [SerializeField] private Vector3 cameraOffset;

    private bool moveCam = false;
    public Rigidbody rb;

    private InputActionMap currentControls;

    [Tooltip("Intensité de la vignette selon les PV perdus")] 
    [SerializeField] private AnimationCurve vignetteIntensity; //Intensité de la vignette

    [SerializeField] private Volume volume;
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float currentHealth;
    [SerializeField] private float timeToRegenerateHealth;
    [SerializeField] private float regenSpeed;

    #region Refs

    [Dropdown("GetInputMaps")] [Foldout("Refs")]
    public string currentInputMap;

    [Foldout("Refs")] public PlayerInput inputs;
    [Foldout("Refs")] public LineRenderer shootTrail;
    [Foldout("Refs")] [SerializeField] public Transform playerCam;
    [Foldout("Refs")] [SerializeField] private Transform hands;
    [Foldout("Refs")] [SerializeField] private Transform leftHand;
    [Foldout("Refs")] [SerializeField] private Transform rightHand;
    [Foldout("Refs")] [SerializeField] private RectTransform telekinesisPointer;
    [Foldout("Refs")] [SerializeField] private Transform offsetPosition;

    [Foldout("Refs")] [SerializeField] public CapsuleCollider standingCollider;
    [Foldout("Refs")] [SerializeField] private CapsuleCollider crouchedCollider;

    #endregion

    #region Movement Variables

    [Foldout("Movement")] [Tooltip("Acceleration when walking")] [SerializeField] [HorizontalLine(color: EColor.Black)]
    private AnimationCurve runCurve;

    [Foldout("Movement")] [Tooltip("Acceleration when walking")] [SerializeField]
    private float runVelocity;

    [Foldout("Movement")] [Tooltip("Acceleration when walking")] [SerializeField] [HorizontalLine(color: EColor.Black)]
    private AnimationCurve crouchCurve;

    [Foldout("Movement")] [Tooltip("Acceleration when walking")] [SerializeField]
    private float crouchVelocity;

    [Foldout("Movement")] [Tooltip("Acceleration when walking")] [SerializeField] [HorizontalLine(color: EColor.Black)]
    private AnimationCurve sprintCurve;

    [Foldout("Movement")] [Tooltip("Acceleration when walking")] [SerializeField]
    private float sprintVelocity;

    [Foldout("Movement")] [Tooltip("Acceleration when walking")] [SerializeField] [HorizontalLine(color: EColor.Black)]
    private AnimationCurve dragCurve;

    [Space(30)] [Foldout("Movement")] [Tooltip("Camera sensitivity")] [SerializeField]
    private float lookSpeed;

    [Foldout("Movement")] [Tooltip("Camera sensitivity")] [SerializeField]
    private float maxSteepness;

    [Foldout("Movement")] [Tooltip("Limit to the camera being able to look up or down")] [SerializeField]
    private LayerMask groundLayer;

    [Foldout("Movement")] [Tooltip("Limit to the camera being able to look up or down")] [SerializeField]
    private float lookXLimit;


    [Foldout("Movement")] [Tooltip("CameraFOV when Player is walking")] [SerializeField]
    private float normalFOV;

    [Foldout("Movement")] [Tooltip("CameraFOV when player is running")] [SerializeField]
    private float runningFOV;

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

    [Foldout("Telekinesis")]
    [Tooltip("Delay after releasing Telekinesis before you can use it on another object")]
    [SerializeField]
    private float inputSpamBuffer;

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

    [Foldout("Debug")] [SerializeField] private Transform shootingHand;

    [Foldout("Debug")] [SerializeField] private Vector3 inputVelocity;

    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    private bool appliedForce;

    [Foldout("Debug")] [SerializeField] private bool bobbing;

    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    public bool canMove = true;


    enum PlayerStates
    {
        Standing,
        Sprinting,
        Crouching
    }

    private PlayerStates state = PlayerStates.Standing;

    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    private bool isJumping = false;

    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    private bool isGrounded = false;

    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    private float shootSpeedTimer;

    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    public ControllableProp controlledProp;

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
    private float moveInputTimer;
    private bool recentlyDepletedStamina = false;

    #endregion

    [SerializeField] private GameObject inkStainDecal;

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
                restingPosOffset.y * (playerCam.forward.y + 1), restingPosOffset.z);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(offsetPosition.position, 0.2f);


        Debug.DrawLine(playerCam.position, playerCam.position + playerCam.forward * 20, Color.black);
    }

    private void OnValidate()
    {
        if (!inputs)
        {
            inputs = GetComponent<PlayerInput>();
        }
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

        playerLayer = LayerMask.GetMask("Player") + shootMask;

        RegisterInputs();

        //currentControls.FindAction("Telekinesis",true).performed += ;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        startPos = hands.localPosition;
        currentStamina = maxStamina;
        currentAmmo = magSize;
        inventoryAmmo = maxStoredAmmo;
        currentHealth = maxHealth;
    }




    private void Interact(InputAction.CallbackContext obj)
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 2, ~LayerMask.GetMask("Player")))
        {
            if (hit.transform.TryGetComponent(out ICanInteract interactable))
            {
                interactable.Interact(-hit.normal);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        CameraShake.instance.ShakeOneShot(3);
        currentHealth -= damage;

        if (Regen != null) StopCoroutine(Regen);
        Regen = StartCoroutine(Regenerate());
    }

    private Coroutine Regen;

    private IEnumerator Regenerate()
    {
        yield return new WaitForSeconds(timeToRegenerateHealth);
        while (currentHealth < maxHealth)
        {
            currentHealth += regenSpeed * Time.deltaTime;
            yield return null;
        }

        currentHealth = maxHealth;
    }

    private void OnDisable()
    {
        UnRegisterInputs();
    }

    void Start()
    {
        CheckShootingHand();
    }

    private Coroutine reloadCoroutine;
    private bool reloading = false;

    private void Reload(InputAction.CallbackContext obj)
    {
        if (reloading || currentAmmo == magSize || inventoryAmmo == 0)
            return;
        reloading = true;
        reloadCoroutine = StartCoroutine(Reload2());
    }

    #region LogicChecks
    private void RegisterInputs()
    {
        currentControls.Enable();

        currentControls.FindAction("ToggleCrouch", true).performed += ToggleCrouch;
        currentControls.FindAction("ToggleCrouch", true).canceled += ToggleCrouch;

        currentControls.FindAction("ToggleSprint", true).performed += ToggleSprint;
        currentControls.FindAction("ToggleSprint", true).canceled += ToggleSprint;

        currentControls.FindAction("Shoot", true).performed += Shoot;

        currentControls.FindAction("Telekinesis", true).canceled += ReleaseProp;

        currentControls.FindAction("Reload", true).performed += Reload;

        currentControls.FindAction("Interact", true).performed += Interact;
    }

    void UnRegisterInputs()
    {

        currentControls.FindAction("ToggleCrouch", true).performed -= ToggleCrouch;
        currentControls.FindAction("ToggleCrouch", true).canceled -= ToggleCrouch;

        currentControls.FindAction("ToggleSprint", true).performed -= ToggleSprint;
        currentControls.FindAction("ToggleSprint", true).canceled -= ToggleSprint;

        currentControls.FindAction("Shoot", true).performed -= Shoot;

        currentControls.FindAction("Telekinesis", true).canceled -= ReleaseProp;

        currentControls.FindAction("Reload", true).performed -= Reload;

        currentControls.FindAction("Interact", true).performed -= Interact;
        
        currentControls.Disable();
    }
    void CheckShootingHand()
    {
        if (currentControls.FindAction("Shoot", true).bindings[0].path == "<Mouse>/leftButton")
        {
            Debug.Log("Shooting with left hand");
            shootingHand = rightHand;
            restingPosOffset = new Vector3(-restingPosOffset.x, restingPosOffset.y, restingPosOffset.z);
        }
        else
        {
            Debug.Log("Shooting with right hand");
            shootingHand = leftHand;
        }

        UpdateRestingPos();
    }

    private bool GroundCheck(out RaycastHit hit)
    {
        bool check = false;
        if (Physics.SphereCast(transform.position, 0.3f, Vector3.down, out hit, 1.1f, groundLayer))
        {
            if (!isJumping)
            {
                check = true;
            }

            if (Vector3.Angle(hit.normal, transform.up) > maxSteepness)
            {
                check = false;
            }
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

    #endregion

    private const float reloadHandMove = 2f;
    private Vector3 reloadBasePos;

    private IEnumerator Reload2()
    {
        reloadBasePos = shootingHand.localPosition;
        shootingHand.DOLocalMove(reloadBasePos - Vector3.forward * reloadHandMove, 0.4f);
        yield return new WaitForSeconds(reloadSpeed);
        shootingHand.DOLocalMove(reloadBasePos, 0.4f);
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

        GameManager.instance.UpdateAmmoUI();
        reloading = false;
    }


    // Update is called once per frame
    void Update()
    {
        UpdateRestingPos();
        playerDir = Vector3.zero;


        var lostHealth = (maxHealth - currentHealth) / maxHealth;

        volume.weight = Mathf.Lerp(volume.weight, vignetteIntensity.Evaluate(lostHealth), .01f);
        
        if (canMove)
        {
            Rotate();
            ForwardInput();
            SidewaysInput();
            ArmBobbing();
            TelekinesisInput();
        }

        if (shootSpeedTimer >= 0)
        {
            shootSpeedTimer -= Time.deltaTime;
        }

        switch (state)
        {
            case PlayerStates.Crouching:
                playerCam.position =
                    Vector3.Lerp(playerCam.position, transform.position + crouchedCollider.center, 0.8f);
                break;

            case PlayerStates.Sprinting:
                playerCam.position = Vector3.Lerp(playerCam.position, transform.position, 0.8f);
                playerCam.position = new Vector3(playerCam.position.x, transform.position.y + 0.5f,
                    playerCam.position.z);

                camera1.fieldOfView = Mathf.Lerp(camera1.fieldOfView, runningFOV, lerpFOV);
                break;

            case PlayerStates.Standing:
                playerCam.position = Vector3.Lerp(playerCam.position, transform.position, 0.8f);
                playerCam.position = new Vector3(playerCam.position.x, transform.position.y + 0.5f,
                    playerCam.position.z);

                camera1.fieldOfView = Mathf.Lerp(camera1.fieldOfView, normalFOV, lerpFOV);
                break;
        }

        if (!controlledProp)
        {
            CheckTelekinesisTarget();

            currentStamina =
                GameManager.instance.UpdatePlayerStamina(currentStamina, maxStamina, Time.deltaTime * staminaRegen);
        }
        else if (telekinesisPointer.gameObject.activeSelf)
            telekinesisPointer.gameObject.SetActive(false);
    }


    private void LateUpdate()
    {
        HorizontalMovement();
    }


    #region Physics

    private void HorizontalMovement()
    {
        isGrounded = GroundCheck(out RaycastHit hit);
        if (playerDir == Vector3.zero)
        {
            appliedForce = false;
        }

        playerDir.Normalize();

        if (!isGrounded)
        {
            rb.AddForce(Vector3.down * 10);
            // moveInputTimer = 0;
            return;
        }

        if (appliedForce)
        {
            moveInputTimer += Time.deltaTime;
        }
        else
        {
            moveInputTimer -= Time.deltaTime;
            moveInputTimer = Mathf.Clamp(moveInputTimer, 0, dragCurve.keys[^1].time);
        }


        if (!appliedForce)
        {
            inputVelocity = rb.velocity.normalized * (dragCurve.Evaluate(moveInputTimer) *
                                                      new Vector2(rb.velocity.x, rb.velocity.z).magnitude);
            rb.velocity = new Vector3(inputVelocity.x, 0, inputVelocity.z);
            return;
        }

        switch (state)
        {
            case PlayerStates.Standing:
                inputVelocity = playerDir *
                                (runCurve.Evaluate(moveInputTimer) * runVelocity);
                Mathf.Clamp(moveInputTimer, 0, runCurve.keys[^1].time);
                break;

            case PlayerStates.Sprinting:
                inputVelocity = playerDir * (sprintCurve.Evaluate(moveInputTimer) *
                                             sprintVelocity);
                Mathf.Clamp(moveInputTimer, 0, sprintCurve.keys[^1].time);
                break;

            case PlayerStates.Crouching:
                inputVelocity = playerDir * (crouchCurve.Evaluate(moveInputTimer) *
                                             crouchVelocity);
                Mathf.Clamp(moveInputTimer, 0, crouchCurve.keys[^1].time);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        // float magnitude = inputVelocity.magnitude;
        // Debug.Log(Vector3.Cross(transform.up,inputVelocity.normalized));
        rb.velocity = new Vector3(inputVelocity.x, rb.velocity.y, inputVelocity.z);
    }

    private LayerMask playerLayer;
    private Vector3 tempWorldToScreen;

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
                    break;
                }

                if (grabDistanceBuffer > Vector3.Distance(controlledProp.transform.position, offsetPosition.position))
                {
                    controlledProp.isGrabbed = true;
                    CameraShake.instance.StartInfiniteShake(0);
                }

                break;
            case Enemy enemy:
                currentStamina =
                    GameManager.instance.UpdatePlayerStamina(currentStamina, maxStamina,
                        Time.deltaTime * -holdEnemyCost);

                tempWorldToScreen = camera1.WorldToScreenPoint(controlledProp.transform.position);
                if (tempWorldToScreen.x < 0 || tempWorldToScreen.x > Screen.width ||
                    tempWorldToScreen.y < 0 || tempWorldToScreen.y > Screen.height ||
                    tempWorldToScreen.z < 0)
                {
                    ReleaseProp(new InputAction.CallbackContext());
                    return;
                }

                Vector3 dir2 = controlledProp.transform.position - transform.position;
                if (Physics.Raycast(controlledProp.transform.position, -dir2.normalized, out RaycastHit hit, maxRange,
                        playerLayer))
                {
                    if (!hit.collider.TryGetComponent(out PlayerController controller))
                    {
                        ReleaseProp(new InputAction.CallbackContext());
                        return;
                    }
                }

                if (enemy.isGrabbed) break;


                enemy.body.constraints = RigidbodyConstraints.FreezeAll;
                enemy.isGrabbed = true;
                enemy.GrabbedBehavior(1, 0.1f, 30);
                break;
        }

        if (currentStamina < 1)
        {
            CameraShake.instance.StopInfiniteShake();
            controlledProp.ApplyTelekinesis();
            controlledProp.isGrabbed = false;
            controlledProp = null;
            recentlyDepletedStamina = true;
        }
    }


    public void ReleaseProp(InputAction.CallbackContext obj)
    {
        recentlyDepletedStamina = false;
        if (controlledProp == null)
        {
            CameraShake.instance.ResetCoroutine();
            return;
        }

        CameraShake.instance.StopInfiniteShake();
        switch (controlledProp)
        {
            case TelekinesisObject:
                controlledProp.ApplyTelekinesis();
                if (!controlledProp.isGrabbed)
                {
                    controlledProp.body.velocity *= 0.1f;
                }
                else
                {
                    controlledProp.isGrabbed = false;

                    currentStamina =
                        GameManager.instance.UpdatePlayerStamina(currentStamina, maxStamina, -throwCost);

                    controlledProp.body.velocity = Vector3.zero;

                    var dir = Vector3.zero;
                    if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit hit, maxRange,
                            LayerMask.GetMask("Telekinesis")))
                    {
                        dir = hit.point - offsetPosition.position;
                    }
                    else
                    {
                        dir = playerCam.forward;
                    }

                    dir.Normalize();
                    controlledProp.body.AddForce(dir * throwForce, ForceMode.Impulse);
                }

                break;
            case Enemy:

                controlledProp.isGrabbed = false;
                controlledProp.ApplyTelekinesis();
                controlledProp.body.constraints = RigidbodyConstraints.FreezeRotation;

                break;
        }

        if (currentStamina < 0) currentStamina = 0;
        StartCoroutine(controlledProp.BufferGrabbing());
        controlledProp = null;
    }

    #endregion

    #region Controls

    public void ImmobilizePlayer()
    {
        canMove = !canMove;

        if (canMove)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            currentControls.Enable();
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            currentControls.Disable();
        }
    }

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

    private void ToggleCrouch(InputAction.CallbackContext obj)
    {
        hands.localPosition = Vector3.Lerp(hands.localPosition, startPos, 0.6f);

        if (obj.canceled)
        {
            if (state == PlayerStates.Sprinting)
            {
            }
            else
            {
                state = PlayerStates.Standing;
            }
        }
        else
        {
            state = PlayerStates.Crouching;
        }

        crouchedCollider.enabled = state == PlayerStates.Crouching;
        standingCollider.enabled = state != PlayerStates.Crouching;
    }

    private void ToggleSprint(InputAction.CallbackContext obj)
    {
        hands.localPosition = startPos;
        if (obj.canceled)
        {
            if (state == PlayerStates.Crouching)
            {
                return;
            }

            state = PlayerStates.Standing;
        }
        else
        {
            state = PlayerStates.Sprinting;
        }
    }


    private LineRenderer currentTrail;
    private Camera camera1;

    private void Shoot(InputAction.CallbackContext obj)
    {
        if (shootSpeedTimer > 0) return;

        if (currentAmmo == 0) return;

        if (reloading) return;
        if (stagger != null) StopCoroutine(stagger);
        stagger = StartCoroutine(StaggerSprint(state == PlayerStates.Sprinting));

        CameraShake.instance.ShakeOneShot(1);
        currentAmmo--;
        GameManager.instance.UpdateAmmoUI();

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
                    enemy.TakeDamage(bulletDmg, hit.collider);
                    GameManager.instance.HitMark(true);
                }
            }

            if (hit.collider.CompareTag("Body"))
            {
                if (hit.collider.transform.parent.TryGetComponent(out Enemy enemy))
                {
                    enemy.TakeDamage(bulletDmg, hit.collider);
                    GameManager.instance.HitMark(false);
                }
            }

            if (hit.collider.TryGetComponent(out IDestructible target))
            {
                target.TakeDamage();
            }

            currentTrail.SetPosition(1, hit.point);

            //Coucou, Thomas est passé par là (jusqu'au prochain commentaire)
            var decal = Instantiate(inkStainDecal, hit.point + hit.normal * 0.02f, Quaternion.identity, hit.transform);
            decal.transform.forward = -hit.normal;
            decal.transform.RotateAround(decal.transform.position, decal.transform.forward, Random.Range(-180f, 180f));
            //Je m'en vais !
        }
        else
        {
            Debug.Log("Hit some air");
            currentTrail.SetPosition(1, playerCam.forward * maxRange + playerCam.position);
        }

        if (currentAmmo == 0)
        {
            reloading = true;
            reloadCoroutine = StartCoroutine(Reload2());
        }
    }

    private Coroutine stagger;

    IEnumerator StaggerSprint(bool sprinting)
    {
        if (sprinting)
        {
            state = PlayerStates.Standing;
            yield return new WaitForSeconds(1f);
            if (currentControls.FindAction("ToggleSprint", true).IsPressed())
                state = PlayerStates.Sprinting;
        }
    }

    private void TelekinesisInput()
    {
        if (currentControls.FindAction("Telekinesis", true).IsPressed() && !recentlyDepletedStamina)
        {
            if (!controlledProp)
            {
                if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit hit, maxRange, shootMask))
                {
                    if (hit.collider.TryGetComponent(out TelekinesisObject TK))
                    {
                        if (!TK.canBeGrabbed) return;
                        controlledProp = TK;
                        controlledProp.ApplyTelekinesis();
                    }

                    if (hit.collider.transform.parent.TryGetComponent(out Enemy enemy))
                    {
                        if (!enemy.canBeGrabbed) return;
                        controlledProp = enemy;
                        controlledProp.ApplyTelekinesis();
                    }


                    CameraShake.instance.ShakeOneShot(2);
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
        if (state == PlayerStates.Sprinting)
        {
            pos.x += Mathf.Cos(Time.time * frequeny / 2) * amplitude;
        }

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


    #region Deprecated

    [Foldout("Obsolete")] [Tooltip("Force applied on a jump")] [SerializeField]
    private float jumpForce;

    [Foldout("Obsolete")] [Tooltip("Acceleration when running")] [SerializeField]
    private float runAccel;

    [Foldout("Obsolete")] [Tooltip("Multiplies with maxHorizontalVelocity when running")] [SerializeField]
    private float runMaxVelocityFactor;

    [Foldout("Obsolete")] [Tooltip("To make a jump more or less floaty")] [SerializeField]
    private AnimationCurve jumpCurve;

    #endregion
}
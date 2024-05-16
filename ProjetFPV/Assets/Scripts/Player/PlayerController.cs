using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mechanics;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class PlayerController : Singleton<PlayerController>
{

    private AudioSource audioSource;

    [SerializeField] private AudioClip shootClip;
    
    private bool moveCam = false;
    public Rigidbody rb;

    private InputActionMap currentControls;

    [Tooltip("Intensité de la vignette selon les PV perdus")] 
    [SerializeField] private AnimationCurve vignetteIntensity; //Intensité de la vignette

    [SerializeField] private Volume volume;

    [Tooltip("Maximum Ink of the player")] [SerializeField]
    public float maxInk;
    
    [Tooltip("How much stamina the player currently has")]
    [SerializeField] //[ProgressBar("maxInk", EColor.Green)]
    public float currentInk;
    
    [SerializeField] private int maxHealth = 10; 
    [SerializeField] [ProgressBar("maxHealth", EColor.Red)]
    private float currentHealth;
    [SerializeField] public int healPackCapacity;
    [SerializeField] private float healAmount;
    //[SerializeField] private float timeToRegenerateHealth;
    //[SerializeField] private float regenSpeed;

    #region Refs

    [Dropdown("GetInputMaps")] [Foldout("Refs")]
    public string currentInputMap;

    [Foldout("Refs")] public PlayerInput inputs;
    [Foldout("Refs")] [SerializeField] public Transform playerCam;
    [Foldout("Refs")] [SerializeField] private Transform hands;
    [Foldout("Refs")] [SerializeField] private Transform leftHand;
    [Foldout("Refs")] [SerializeField] private Transform rightHand;
    [Foldout("Refs")] [SerializeField] private RectTransform telekinesisPointer;
    [Foldout("Refs")] [SerializeField] private Transform offsetPosition;

    [Foldout("Refs")] [SerializeField] public CapsuleCollider standingCollider;
    [Foldout("Refs")] [SerializeField] private CapsuleCollider crouchedCollider;
    
    [Foldout("Refs")] [SerializeField] private ParticleSystem[] VFX_TKStart;
    [Foldout("Refs")] [SerializeField] private ParticleSystem VFX_TKEnd;

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
    
    [Foldout("Shoot")] [Tooltip("Ink drain per second when in surplus")] [SerializeField]
    private float surplusDrainRate;

    [Foldout("Shoot")] [Tooltip("Time needed to reload")] [SerializeField]
    private float reloadSpeed;
    

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


    
    [Foldout("Telekinesis")] [Tooltip("Cost per second of using telekinesis on an object")] [SerializeField]
    private float holdObjectCost;

    [Foldout("Telekinesis")] [Tooltip("Cost per second of using telekinesis on an enemy")] [SerializeField]
    private float holdEnemyCost;
    
    [Foldout("Telekinesis")]
    [SerializeField]
    [Range(0,1)]public float holdObjectYTolerance = 0.6f;

    [Foldout("Telekinesis")] [Tooltip("Cost of releasing an object from telekinesis")] [SerializeField]
    private float throwCost;

    [Foldout("Telekinesis")]
    [Tooltip("Force applied to grabbed object when released from telekinesis")]
    [SerializeField]
    private float throwForce;

    [Foldout("Telekinesis")]
    [SerializeField]
    public float inkAbsorbSpeed;

    #endregion

    #region Bobbing Variables

    [Foldout("Bobbing")] [Tooltip("How big the arm bobbing is")] [SerializeField]
    private float amplitude;

    [Foldout("Bobbing")] [Tooltip("Speed of the arm bobbing")] [SerializeField]
    private float crouchFrequency;
    
    [Foldout("Bobbing")] [Tooltip("Speed of the arm bobbing")] [SerializeField]
    private float walkFrequency;
    
    [Foldout("Bobbing")] [Tooltip("Speed of the arm bobbing")] [SerializeField]
    private float sprintFrequency;

    [Foldout("Bobbing")]
    [Tooltip("(NOT USED YET) Camera tilting (in degrees) when player is moving left or right")]
    [SerializeField]
    private float sideTilting;

    #endregion

    #region Debug

    [Foldout("Debug")] [SerializeField] private Transform shootingHand;
    
    [Foldout("Debug")] [SerializeField] public int currentHealPackAmount;

    [Foldout("Debug")] [SerializeField] private Vector3 inputVelocity;

    [Foldout("Debug")] [Tooltip("")] [SerializeField]
    private bool appliedForce;


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

    [Foldout("Debug")] public bool inSurplus;


    #endregion

    #region Misc

    private float rotationX;
    private Vector3 playerDir;
    private float jumpTime;
    private Vector2 horizontalVelocity;
    private Vector3 startPos;
    private float moveInputTimer;
    private bool recentlyDepletedStamina = false;
    [HideInInspector] public ShootingHand socketManager;

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
        socketManager = GetComponent<ShootingHand>();

        playerLayer = LayerMask.GetMask("Player") + socketManager.shootMask;

        RegisterInputs();

        //currentControls.FindAction("Telekinesis",true).performed += ;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        startPos = camera1.transform.localPosition;
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(float damage)
    {
        CameraShake.instance.ShakeOneShot(3);
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Debug.Log("Je suis mort");
            GameManager.instance.PlayerDeath();
        }
    }

    //private Coroutine Regen;

    // private IEnumerator Regenerate()
            // {
            //     yield return new WaitForSeconds(timeToRegenerateHealth);
            //     while (currentHealth < maxHealth)
            //     {
            //         currentHealth += regenSpeed * Time.deltaTime;
            //         yield return null;
            //     }
            //
            //     currentHealth = maxHealth;
            // }

    private void OnDisable()
    {
        UnRegisterInputs();
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentInk = GameManager.instance.UpdatePlayerStamina(currentInk, maxInk, 0);
        CheckShootingHand();
    }

    private Coroutine reloadCoroutine;
    

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

        currentControls.FindAction("Reload", true).performed += RequestReload;

        currentControls.FindAction("Interact", true).performed += Interact;
        
        currentControls.FindAction("UseHealPack", true).performed += UseHealPack;
    }

    void UnRegisterInputs()
    {

        currentControls.FindAction("ToggleCrouch", true).performed -= ToggleCrouch;
        currentControls.FindAction("ToggleCrouch", true).canceled -= ToggleCrouch;

        currentControls.FindAction("ToggleSprint", true).performed -= ToggleSprint;
        currentControls.FindAction("ToggleSprint", true).canceled -= ToggleSprint;

        currentControls.FindAction("Shoot", true).performed -= Shoot;

        currentControls.FindAction("Telekinesis", true).canceled -= ReleaseProp;

        currentControls.FindAction("Reload", true).performed -= RequestReload;

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
        if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit hit, socketManager.maxRange, socketManager.shootMask)
            && hit.collider.TryGetComponent(out ControllableProp prop))
        {
            if (!telekinesisPointer.gameObject.activeSelf)
                telekinesisPointer.gameObject.SetActive(true);

            telekinesisPointer.position = camera1.WorldToScreenPoint(prop.transform.position);
            telekinesisPointer.rotation *= Quaternion.Euler(0, 0, 2);
            return;
        }

        if (telekinesisPointer.gameObject.activeSelf)
            telekinesisPointer.gameObject.SetActive(false);
    }

    void CheckInteractableTarget()
    {
        if (Physics.SphereCast(playerCam.position, 0.3f, playerCam.forward, out RaycastHit hit, 4, ~LayerMask.GetMask("Player")))
        {
            if (hit.transform.TryGetComponent(out ICanInteract interactable))
            {
                if (!GameManager.instance.interactText.enabled)
                {
                    GameManager.instance.interactText.enabled = true;
                }

                interactable.ShowContext();
                return;
            }

        }
        if (GameManager.instance.interactText.enabled)
        {
            GameManager.instance.interactText.enabled = false;
        }
    }

    #endregion

    private const float ReloadHandMove = 2f;
    private Vector3 reloadBasePos;

    private IEnumerator Reload2()
    {
        
        reloadBasePos = shootingHand.localPosition;
        shootingHand.DOLocalMove(reloadBasePos - Vector3.forward * ReloadHandMove, 0.4f);
        yield return new WaitForSeconds(currentInk < maxInk ? reloadSpeed : socketManager.surplusReloadTime);
        socketManager.ReloadSockets();
        shootingHand.DOLocalMove(reloadBasePos, 0.4f);

        reloading = false;
    }


    // Update is called once per frame
    private void Update()
    {
        UpdateTKCylinder(); // THOMAS
        
        UpdateRestingPos();
        playerDir = Vector3.zero;
        

        var lostHealth = (maxHealth - currentHealth) / maxHealth;

        volume.weight = Mathf.Lerp(volume.weight, vignetteIntensity.Evaluate(lostHealth), .01f);
        
        if (canMove)
        {
            Rotate();
            ForwardInput();
            SidewaysInput();
            TelekinesisInput();
        }

        ArmBobbing();
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
                //playerCam.position = transform.position;
                playerCam.position = new Vector3(playerCam.position.x, transform.position.y + 0.5f,
                    playerCam.position.z);

                camera1.fieldOfView = Mathf.Lerp(camera1.fieldOfView, runningFOV, lerpFOV);
                break;

            case PlayerStates.Standing:
                playerCam.position = Vector3.Lerp(playerCam.position, transform.position, 0.8f);
                //playerCam.position = transform.position;
                playerCam.position = new Vector3(playerCam.position.x, transform.position.y + 0.5f,
                    playerCam.position.z);

                camera1.fieldOfView = Mathf.Lerp(camera1.fieldOfView, normalFOV, lerpFOV);
                break;
        }

        if (!controlledProp)
        {
            CheckTelekinesisTarget();
            CheckInteractableTarget();
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

                currentInk =
                    GameManager.instance.UpdatePlayerStamina(currentInk, maxInk,
                        Time.deltaTime * -holdObjectCost);

                var dir = offsetPosition.position - controlledProp.transform.position;
                dir.Normalize();
                if (playerCam.forward.y < -holdObjectYTolerance)
                {
                    CameraShake.instance.StopInfiniteShake();
                    controlledProp.ApplyTelekinesis();
                    controlledProp.isGrabbed = false;
                    controlledProp = null;
                    recentlyDepletedStamina = true;
                    return;
                }
                if (!controlledProp.isGrabbed)
                {
                    controlledProp.body.velocity = dir * travelSpeed;
                }
                else
                {
                    controlledProp.body.velocity = dir * (travelSpeed *
                                                          (Vector3.Distance(controlledProp.transform.position,
                                                              offsetPosition.position) / grabDistanceBuffer));
                    return;
                }

                if (grabDistanceBuffer > Vector3.Distance(controlledProp.transform.position, offsetPosition.position))
                {
                    controlledProp.isGrabbed = true;
                    CameraShake.instance.StartInfiniteShake(0);
                }


                return;
            
            case AbsorbInk absorbInk:
                absorbInk.storedInk -= inkAbsorbSpeed * Time.deltaTime;
                currentInk =
                    GameManager.instance.UpdatePlayerStamina(currentInk, maxInk, inkAbsorbSpeed * Time.deltaTime);
                var lerpValue = Mathf.Clamp(1 - absorbInk.storedInk / absorbInk.maxInk, 0, 0.8f);
                absorbInk.transform.localScale = Vector3.Lerp(absorbInk.baseScale, Vector3.zero, lerpValue);

                if (!controlledProp.isGrabbed)
                {
                    controlledProp.isGrabbed = true;
                    CameraShake.instance.StartInfiniteShake(0);
                }

                if (absorbInk.storedInk < 0)
                {
                    recentlyDepletedStamina = true;
                    ReleaseProp(new InputAction.CallbackContext());
                    return;
                }
                return;
            
            case Enemy enemy:
                currentInk =
                    GameManager.instance.UpdatePlayerStamina(currentInk, maxInk,
                        Time.deltaTime * -holdEnemyCost);

                tempWorldToScreen = camera1.WorldToScreenPoint(controlledProp.transform.position);
                if (tempWorldToScreen.x < 0 || tempWorldToScreen.x > Screen.width ||
                    tempWorldToScreen.y < 0 || tempWorldToScreen.y > Screen.height ||
                    tempWorldToScreen.z < 0)
                {
                    
                    recentlyDepletedStamina = true;
                    ReleaseProp(new InputAction.CallbackContext());
                    return;
                }

                Vector3 dir2 = controlledProp.transform.position - transform.position;
                if (Physics.Raycast(controlledProp.transform.position, -dir2.normalized, out RaycastHit hit, socketManager.maxRange,
                        playerLayer))
                {
                    if (!hit.collider.TryGetComponent(out PlayerController controller))
                    {
                        
                        recentlyDepletedStamina = true;
                        ReleaseProp(new InputAction.CallbackContext());
                        return;
                    }
                }

                if (enemy.isGrabbed) break;


                enemy.body.constraints = RigidbodyConstraints.FreezeAll;
                enemy.isGrabbed = true;
                enemy.knockedBack = false;
                if (enemy is ChargerBehavior chargerBehavior)
                {
                    chargerBehavior.GrabbedBehavior(1, 0.1f, 30);
                }
                else
                {
                    enemy.GrabbedBehavior(0, 0.1f, 30);
                }
                break;
            
        }

        if (currentInk < 1)
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
        
        if (currentInk < 1)
        {
            ThrowTKObject(); // THOMAS
            CameraShake.instance.StopInfiniteShake();
            controlledProp.ApplyTelekinesis();
            controlledProp.isGrabbed = false;
            controlledProp = null;
            recentlyDepletedStamina = true;
            return;
        }

        switch (controlledProp)
        {
            case TelekinesisObject:
                if (!controlledProp.isGrabbed)
                {
                    controlledProp.body.velocity *= 0.1f;
                }
                else
                {
                    ThrowTKObject(); // THOMAS
                    
                    controlledProp.isGrabbed = false;

                    currentInk =
                        GameManager.instance.UpdatePlayerStamina(currentInk, maxInk, -throwCost);

                    controlledProp.body.velocity = Vector3.zero;

                    var dir = Vector3.zero;
                    if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit hit, socketManager.maxRange,
                            ~LayerMask.GetMask("Telekinesis")))
                    {
                        dir = (hit.point + hit.normal * 0.5f) - offsetPosition.position;
                        Debug.DrawRay(hit.point,hit.normal * 2, Color.magenta,2);
                    }
                    else
                    {
                        dir = playerCam.forward;
                    }
                    
                    dir.Normalize();
                    controlledProp.body.AddForce(dir * throwForce, ForceMode.Impulse);
                }

                controlledProp.ApplyTelekinesis();
                break;
            case Enemy:
                
                controlledProp.isGrabbed = false;
                controlledProp.ApplyTelekinesis();
                ThrowTKObject();
                break;
            
            case AbsorbInk absorbInk:
                absorbInk.isGrabbed = false;
                if (absorbInk.storedInk < 0)
                {
                    Destroy(absorbInk.gameObject);
                }
                break;
        }

        if (currentInk < 0) currentInk = 0;
        StartCoroutine(controlledProp.BufferGrabbing());
        controlledProp = null;
        CameraShake.instance.StopInfiniteShake();
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

        var camRotation = new Vector3(playerCam.localEulerAngles.x,0,
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


    private Camera camera1;
    
    private void Interact(InputAction.CallbackContext obj)
    {
        Debug.DrawRay(playerCam.position,camera1.transform.forward * 4, Color.blue,3);
        if (Physics.SphereCast(playerCam.position, 0.3f, playerCam.forward, out RaycastHit hit, 4, ~LayerMask.GetMask("Player")))
        {
            if (hit.transform.TryGetComponent(out ICanInteract interactable))
            {
                interactable.Interact(-hit.normal);
            }
        }
    }

    private bool superShot;
    private void Shoot(InputAction.CallbackContext obj)
    {
        if (reloading) return;
        if (socketManager.noBullets || socketManager.overheated)return;
        if (shootSpeedTimer > 0) return;
        if (stagger != null) StopCoroutine(stagger);
        stagger = StartCoroutine(StaggerSprint(state == PlayerStates.Sprinting));

        socketManager.ShootWithSocket(playerCam, shootingHand);

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(shootClip);

    }
    
    private bool reloading = false;
    public void RequestReload(InputAction.CallbackContext obj)
    {
        if (reloading)return;
        if (currentInk < socketManager.reloadCostPerBullet) return;
        foreach (var socket in socketManager.sockets)
        {
            if (socket.state != ShootingHand.SocketStates.Empty)continue;
            reloading = true;
        }
        if (!reloading)return;
        
        reloadCoroutine = StartCoroutine(Reload2());
    }
    private void UseHealPack(InputAction.CallbackContext obj)
    {
        if (currentHealPackAmount <= 0 || currentHealth >= maxHealth) return;
        currentHealPackAmount--;
        GameManager.instance.UpdateHealPackUI();
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
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
                if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit hit, socketManager.maxRange, socketManager.shootMask))
                {
                    CameraShake.instance.ShakeOneShot(2);
                    if (hit.collider.TryGetComponent(out TelekinesisObject TK))
                    {
                        if (!TK.canBeGrabbed) return;
                        controlledProp = TK;
                        controlledProp.ApplyTelekinesis();

                        CreateCylinder(hit.collider); // THOMAS
                        return;
                    }

                    if (hit.collider.TryGetComponent(out AbsorbInk absorb))
                    {
                        if (!absorb.canBeGrabbed) return;
                        controlledProp = absorb;
                        controlledProp.ApplyTelekinesis();
                        return;
                    }


                    if (hit.collider.transform.parent.TryGetComponent(out Enemy enemy))
                    {
                        if (!enemy.canBeGrabbed) return;
                        if (currentInk < 1)return;
                        controlledProp = enemy;
                        controlledProp.ApplyTelekinesis();
                        
                        CreateCylinder(hit.collider);
                        return;
                    }


                }

                return;
            }

            TelekinesisPhysics();
        }
    }

    #endregion

    #region Bobbing

    private float usingFrequency;
    Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        switch (state)
        {
            case PlayerStates.Standing:
                usingFrequency = walkFrequency;
                break;
            case PlayerStates.Sprinting:
                usingFrequency = sprintFrequency;
                break;
            case PlayerStates.Crouching:
                usingFrequency = crouchFrequency;
                break;
        }
        pos.y += Mathf.Sin(Time.time * usingFrequency) * amplitude;
        pos.x += Mathf.Cos(Time.time * usingFrequency / 2) * amplitude * 0.5f;
        

        return pos;
    }

    Vector3 FocusTarget()
    {
        Vector3 pos = new Vector3(transform.position.x, transform.position.y + playerCam.transform.localPosition.y,
            transform.position.z);
        pos += playerCam.forward * 15;
        return pos;
    }

    private void PlayMotion(Vector3 motion)
    {
        camera1.transform.localPosition += motion;
    }

    void ResetBobbing()
    {
        camera1.transform.localPosition = Vector3.Lerp(camera1.transform.localPosition, startPos, 1 * Time.deltaTime);
    }

    private void ArmBobbing()
    {
        if (appliedForce && isGrounded)
        {
            PlayMotion(FootStepMotion());
            camera1.transform.LookAt(FocusTarget());
        }
        else
        {
            ResetBobbing();
        }
    }

    #endregion

    [SerializeField] private Transform tkSocket; // THOMAS 
    [SerializeField] private GameObject cylinderPrefab; // THOMAS
    [Range(0f, 5f)] [SerializeField] private float tkCylinderSize;
    private GameObject tkCylinder; // THOMAS 
    private Vector3 tkPoint; // THOMAS 
    private Collider tempColl;
    
    void CreateCylinder(Collider tkColl) // THOMAS (whole method)
    {
        if (tkCylinder != null)
        {
            Destroy(tkCylinder);
        }
        tempColl = tkColl;
        
        tkPoint = tempColl.ClosestPoint(tkSocket.position);

        var cylinder = Instantiate(cylinderPrefab, tkSocket.position, Quaternion.identity);
        
        cylinder.transform.forward = tkPoint - tkSocket.position;
        cylinder.transform.localScale = new Vector3(tkCylinderSize, tkCylinderSize, Vector3.Distance(tkPoint, tkSocket.position) / 2f);
        tkCylinder = cylinder;

        VFX_TKStart[0].Play();
        VFX_TKStart[1].Play();

    }

    void UpdateTKCylinder() // THOMAS (whole method)
    {
        if (tkCylinder == null) return;
        
        tkPoint = tempColl.ClosestPoint(tkSocket.position);
        VFX_TKStart[1].transform.position = tkPoint;
        tkCylinder.transform.position = tkSocket.position;
        tkCylinder.transform.forward = tkPoint - tkSocket.position;
        tkCylinder.transform.localScale = new Vector3(tkCylinderSize, tkCylinderSize, Vector3.Distance(tkPoint, tkSocket.position) / 2f);
    }

    void ThrowTKObject() // THOMAS (whole method)
    {
        if (tkCylinder == null) return;
        foreach (var vfx in VFX_TKStart)
        {
            vfx.Stop();
            vfx.SetParticles(null, 0);
        }
        VFX_TKEnd.Play();

        Destroy(tkCylinder);
    }

    #region Deprecated
    

    #endregion
}
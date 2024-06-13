using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using DG.Tweening;
using Mechanics;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerController : Singleton<PlayerController>
{
    private bool moveCam = false;
    public Rigidbody rb;

    private InputActionMap currentControls;

    [SerializeField] private AudioSource heartBeatAudioSource;
    private float heartBeatVolume;
    [Tooltip("Intensité de la vignette selon les PV perdus")] 
    [SerializeField] private AnimationCurve vignetteIntensity; //Intensité de la vignette

    [SerializeField] private Volume volume;

    [Tooltip("Maximum Ink of the player")] [SerializeField]
    public float maxInk;
    
    [Tooltip("How much stamina the player currently has")]
    [SerializeField] //[ProgressBar("maxInk", EColor.Green)]
    public float currentInk;
    
    [SerializeField] public int maxHealth = 10; 
    [SerializeField] [ProgressBar("maxHealth", EColor.Red)]
    public float currentHealth;
    [SerializeField] public int healPackCapacity;
    [SerializeField] private float healAmount;
    //[SerializeField] private float timeToRegenerateHealth;
    //[SerializeField] private float regenSpeed;
    [SerializeField] private float interactDistance;

    [HideInInspector] public float sensitivity = 1;

    private GameObject reloadSoundStart;

    #region Refs

    [Dropdown("GetInputMaps")] [Foldout("Refs")]
    public string currentInputMap;

    [Foldout("Refs")] public PlayerInput inputs;
    [Foldout("Refs")] [SerializeField] public Transform playerCam;
    [Foldout("Refs")] [SerializeField] private Transform hands;
    [Foldout("Refs")] [SerializeField] private Transform leftHand;
    [Foldout("Refs")] [SerializeField] private Transform rightHand;
    [Foldout("Refs")] [SerializeField] private CanvasGroup telekinesisPointer;
    [Foldout("Refs")] [SerializeField] public Transform offsetPosition;
    [Foldout("Refs")]
    [InfoBox(
        "Blue ball indicates the resting Position (Must be updated manually in editor via the button 'Update Resting Pos' at the bottom, otherwise updates automatically in play mode)")]
    [Label("RestingPos")]
    [SerializeField]
    private Vector3 restingPosOffset;
    

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

    [Foldout("Shoot")] [Tooltip("Time needed to reload")] [SerializeField]
    private float reloadSpeed;
    

    #endregion
    
    #region Bobbing Variables

    [Foldout("Bobbing")] [Tooltip("How big the arm bobbing is")] [SerializeField]
    [Range(0,0.1f)] private float amplitude = 0.003f;

    [Foldout("Bobbing")] [Tooltip("Speed of the arm bobbing")] [SerializeField]
    [Range(1,50)] private float crouchFrequency = 5;
    
    [Foldout("Bobbing")] [Tooltip("Speed of the arm bobbing")] [SerializeField]
    [Range(1,50)] private float walkFrequency = 10;
    
    [Foldout("Bobbing")] [Tooltip("Speed of the arm bobbing")] [SerializeField]
    [Range(1,50)] private float sprintFrequency = 20;

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




    #endregion

    #region Misc

    [HideInInspector] public float rotationX;
    private Vector3 playerDir;
    private float jumpTime;
    private Vector2 horizontalVelocity;
    private Vector3 startPos;
    private float moveInputTimer;
    [HideInInspector] public bool recentlyDepletedStamina = false;
    [HideInInspector] public ShootingHand socketManager;
    [HideInInspector] public TelekinesisModule tkManager;
    [HideInInspector] public AnimHandsController animManager;

    #endregion
    
    #region Audio Variables

    private float walkTimer;
    [SerializeField] private float audioWalkSpeed;
    [HideInInspector] public int isOnWood;
    
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
        camera1 = Camera.main;
        rb = GetComponent<Rigidbody>();
        inputs = GetComponent<PlayerInput>();
        socketManager = GetComponent<ShootingHand>();
        tkManager = GetComponent<TelekinesisModule>();
        animManager = GetComponent<AnimHandsController>();
        
        inputs.actions.Enable();
        currentControls = inputs.actions.FindActionMap(currentInputMap);


        RegisterInputs();


        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        startPos = camera1.transform.localPosition;
        currentHealth = maxHealth;
        
        playerLayer = LayerMask.GetMask("Player") + socketManager.shootMask;
        sensitivity = 1;
    }
    
    public void TakeDamage(float damage)
    {
        CameraShake.instance.ShakeOneShot(3);
        currentHealth -= PlayerPrefs.GetInt("difficulty") == 0 ? damage : damage/2f;

        //SON
        if (currentHealth <= 0)
        {
            Debug.Log("Je suis mort");
            GameManager.instance.PlayerDeath();
        }
    }

    private void OnDisable()
    {
        UnRegisterInputs();
    }

    void Start()
    {
        if (!PlayerPrefs.HasKey("isReloadingSave"))
        {
            PlayerPrefs.SetInt("isReloadingSave", 0);
        }
        
        if (PlayerPrefs.GetInt("isReloadingSave") == 1)
        {
            transform.position = new Vector3(PlayerPrefs.GetFloat("SavePosX"), PlayerPrefs.GetFloat("SavePosY"),
                PlayerPrefs.GetFloat("SavePosZ"));
            currentInk = PlayerPrefs.GetFloat("SaveInkLevel");
            currentHealPackAmount = PlayerPrefs.GetInt("SaveHealKits");
            
            PlayerPrefs.SetInt("isReloadingSave", 0);
        }
        else
        {
            PlayerPrefs.SetFloat("SavePosX", transform.position.x);
            PlayerPrefs.SetFloat("SavePosY", transform.position.y);
            PlayerPrefs.SetFloat("SavePosZ", transform.position.z);
            PlayerPrefs.SetFloat("SaveInkLevel", currentInk);
            PlayerPrefs.SetInt("SaveHealKits", currentHealPackAmount);
        }
        
        
        currentInk = GameManager.instance.UpdatePlayerStamina(currentInk, maxInk, 0);
        CheckShootingHand();
//        heartBeatVolume = AudioManager.instance.GetVolume(3, 18);
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
        if (Physics.SphereCast(transform.position, 0.1f, Vector3.down, out hit, 1f, groundLayer))
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

    private void OnCollisionEnter(Collision other)
    {
        var y = 3;
        if (rb.velocity.y > y)
        {
            //SON
            AudioManager.instance.PlaySound(3, 9, gameObject, 0.1f, false);
        }
    }

    private float pointerTimer = 0;
    private float pointerTime = 0.3f;
    private float fadedScale = 3;
    private void CheckTelekinesisTarget()
    {
        pointerTimer = Mathf.Clamp(pointerTimer, 0, pointerTime);
        if (tkManager.controlledProp)
        {
            pointerTimer += Time.deltaTime;
            telekinesisPointer.alpha = Mathf.Lerp(0,1,pointerTimer/pointerTime);
            telekinesisPointer.transform.localScale =
                Vector3.Lerp(Vector3.one * fadedScale, Vector3.one, pointerTimer / pointerTime);
            telekinesisPointer.transform.rotation *= Quaternion.Euler(0, 0, -2f);
            return;
        }
        
        
        if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit hit, socketManager.maxRange)
            && hit.collider.TryGetComponent(out ControllableProp prop))
        {
            if (!prop.canBeGrabbed)
            {
                pointerTimer -= Time.deltaTime;
                telekinesisPointer.alpha = Mathf.Lerp(0,1,pointerTimer/pointerTime);
                telekinesisPointer.transform.localScale =
                    Vector3.Lerp(Vector3.one * fadedScale, Vector3.one, pointerTimer / pointerTime);
                telekinesisPointer.transform.rotation *= Quaternion.Euler(0, 0, -5);
                return;
            }

            pointerTimer += Time.deltaTime;
            //telekinesisPointer.position = camera1.WorldToScreenPoint(prop.transform.position);
            telekinesisPointer.alpha = Mathf.Lerp(0,1,pointerTimer/pointerTime);
            telekinesisPointer.transform.localScale =
                Vector3.Lerp(Vector3.one * fadedScale, Vector3.one, pointerTimer / pointerTime);
            telekinesisPointer.transform.rotation *= Quaternion.Euler(0, 0, -0.8f);
            return;
        }

        pointerTimer -= Time.deltaTime;
        
        telekinesisPointer.alpha = Mathf.Lerp(0,1,pointerTimer/pointerTime);
        if (animManager.holding)
        {
            telekinesisPointer.transform.localScale =
                Vector3.Lerp(Vector3.one * fadedScale, Vector3.one, pointerTimer / pointerTime);
        }
        else
        {
            telekinesisPointer.transform.localScale =
                Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, pointerTimer / pointerTime);
        }
        telekinesisPointer.transform.rotation *= Quaternion.Euler(0, 0, animManager.holding ? -5f : -0.8f);
    }

    void CheckInteractableTarget()
    {
        if (Physics.Raycast(playerCam.position,  playerCam.forward, out RaycastHit hit, interactDistance, ~LayerMask.GetMask("Player")))   
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

    private float reloadTimer;
    private IEnumerator Reload2()
    {
        reloadBasePos = shootingHand.localPosition;
        animManager.RightHand_ReloadStart();
        yield return new WaitForSeconds(animManager.rightHand.GetClip("A_ReloadStartNew").length);
        
        int numberOfReloads =
            Mathf.CeilToInt(Mathf.Clamp(currentInk / socketManager.reloadCostPerBullet, 0, socketManager.sockets.Count));
        var time = reloadSpeed / numberOfReloads;
        
        reloadSoundStart = AudioManager.instance.PlaySound(3, 3, gameObject, 0.1f, false);
        for (int i = socketManager.sockets.Count - 1; i >= 0; i--)
        {
            if (numberOfReloads == 0) break;
            if (socketManager.sockets[i].state == ShootingHand.SocketStates.Loaded)continue;
            numberOfReloads--;
            reloadTimer = 0;
            while (reloadTimer < time)
            {
                reloadTimer += Time.deltaTime;
                socketManager.sockets[i].socketMesh.material.SetFloat(socketManager.InkLevel,
                    Mathf.Lerp(0,1,reloadTimer/time));

                yield return null;

            }   
            socketManager.sockets[i].socketMesh.material.SetFloat(socketManager.InkLevel,1);
        }
        socketManager.ReloadSockets();
        
        AudioManager.instance.PlaySound(3, 20, gameObject, 0.1f, false);
        Destroy(reloadSoundStart);
        
        animManager.RightHand_ReloadEnd();
        shootingHand.DOLocalMove(reloadBasePos, 0.4f);

        reloading = false;
    }


    // Update is called once per frame
    private void Update()
    { // THOMAS
        walkTimer -= Time.deltaTime;

        #region Cinématique bandes noires
        
        if (!canMove && isEndingCinematic)
        {
            camera1.transform.parent.transform.localRotation = Quaternion.Lerp(camera1.transform.parent.transform.localRotation, Quaternion.Euler(2,0,0), Time.deltaTime * 1.2f);
            return;
        }
        
        if (!canMove && isRepositiong)
        {
            playerDir += new Vector3(cinematicStartPos.x, transform.position.y, cinematicStartPos.z) - transform.position;
            appliedForce = true;
            HorizontalMovement();
            
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0,0,0), Time.deltaTime * 0.8f);
            camera1.transform.parent.transform.localRotation = Quaternion.Lerp(camera1.transform.parent.transform.localRotation, Quaternion.Euler(-40,0,0), Time.deltaTime * 0.1f);
            return;
        }
        
        if (!canMove && isControled)
        {
            playerDir += transform.forward;
            appliedForce = true;
            HorizontalMovement();

            camera1.transform.parent.transform.localRotation = Quaternion.Lerp(camera1.transform.parent.transform.localRotation, Quaternion.Euler(-40,0,0), Time.deltaTime * 0.1f);
            return;
        }
        
        #endregion
        
        UpdateRestingPos();
        playerDir = Vector3.zero;
        

        var lostHealth = (maxHealth - currentHealth) / maxHealth;

        volume.weight = Mathf.Lerp(volume.weight, vignetteIntensity.Evaluate(lostHealth), .01f);
        heartBeatAudioSource.volume = lostHealth * heartBeatVolume;
        
        if (canMove)
        {
            Rotate();
            ForwardInput();
            SidewaysInput();
            TelekinesisInput();
        }

        if (shootSpeedTimer >= 0)
        {
            shootSpeedTimer -= Time.deltaTime;
        }

        switch (state)
        {
            case PlayerStates.Crouching:
                playerCam.position = Vector3.Lerp(playerCam.position, transform.position + crouchedCollider.center, 0.2f);
                break;
            case PlayerStates.Sprinting:
                playerCam.position = Vector3.Lerp(playerCam.position, transform.position + Vector3.up * 0.5f, 0.2f);

                if (appliedForce)
                {
                    camera1.fieldOfView = Mathf.Lerp(camera1.fieldOfView, runningFOV, lerpFOV);
                }
                break;

            case PlayerStates.Standing:
                playerCam.position = Vector3.Lerp(playerCam.position, transform.position + Vector3.up * 0.5f, 0.2f);

                camera1.fieldOfView = Mathf.Lerp(camera1.fieldOfView, normalFOV, lerpFOV);
                break;
        }

        CheckTelekinesisTarget();
        if (!tkManager.controlledProp)
        {
            CheckInteractableTarget();
        }

        
    }

    private void FixedUpdate()
    {
        
        ArmBobbing();
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
            
            //SON DE MARCHE, LA FONCTION EST EN UPDATE

            if (walkTimer <= 0)
            {
                AudioManager.instance.PlaySound(3, isOnWood > 0 ? 0 : 4, gameObject, 0.1f, false);
                walkTimer = state == PlayerStates.Sprinting ? audioWalkSpeed / 2f : isControled ? audioWalkSpeed * 3f : audioWalkSpeed;
                
                switch (state)
                {
                    case PlayerStates.Standing:
                        walkTimer = audioWalkSpeed;
                        break;

                    case PlayerStates.Sprinting:
                        walkTimer = audioWalkSpeed / 1.3f;
                        break;

                    case PlayerStates.Crouching:
                        walkTimer = audioWalkSpeed * 1.6f;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
            animManager.walking = true;
        }
        else
        {
            

            moveInputTimer -= Time.deltaTime;
            moveInputTimer = Mathf.Clamp(moveInputTimer, 0, dragCurve.keys[^1].time);
        }


        if (!appliedForce)
        {
            animManager.walking = false;
            inputVelocity = rb.velocity.normalized * (dragCurve.Evaluate(moveInputTimer) *
                                                      new Vector2(rb.velocity.x, rb.velocity.z).magnitude);
            rb.velocity = new Vector3(inputVelocity.x, 0, inputVelocity.z);
            return;
        }

        switch (state)
        {
            case PlayerStates.Standing:
                inputVelocity = playerDir * (runCurve.Evaluate(moveInputTimer) * runVelocity) / ((isControled || isRepositiong) ? 6f : 1f);
                Mathf.Clamp(moveInputTimer, 0, runCurve.keys[^1].time);
                break;

            case PlayerStates.Sprinting:
                inputVelocity = playerDir * (sprintCurve.Evaluate(moveInputTimer) * sprintVelocity);
                Mathf.Clamp(moveInputTimer, 0, sprintCurve.keys[^1].time);
                break;

            case PlayerStates.Crouching:
                inputVelocity = playerDir * (crouchCurve.Evaluate(moveInputTimer) * crouchVelocity);
                Mathf.Clamp(moveInputTimer, 0, crouchCurve.keys[^1].time);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        // float magnitude = inputVelocity.magnitude;
        // Debug.Log(Vector3.Cross(transform.up,inputVelocity.normalized));
        rb.velocity = new Vector3(inputVelocity.x, rb.velocity.y, inputVelocity.z);
    }

    [HideInInspector]public LayerMask playerLayer;
    public void ReleaseProp(InputAction.CallbackContext obj)
    {
        tkManager.ReleaseProp();
    }

    #endregion

    #region Controls

    public void ImmobilizePlayer()
    {
        canMove = !canMove;

        if (canMove)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
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

    private bool lockedCam = false;

    public void LockCam()
    {
        lockedCam = !lockedCam;
    }

    void Rotate()
    {
        if (lockedCam) return;
        // Debug.Log(Input.GetAxis("Mouse Y"));
        // Debug.Log(Input.GetAxis("Mouse X"));
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed * sensitivity;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCam.localEulerAngles = new Vector3(rotationX, playerCam.localEulerAngles.y, playerCam.localEulerAngles.z);

        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed * sensitivity, 0);

        var camRotation = new Vector3(playerCam.localEulerAngles.x,0,
            playerCam.localEulerAngles.z);
        playerCam.localEulerAngles = camRotation;
    }

    private void ToggleCrouch(InputAction.CallbackContext obj)
    {
        if (!canMove) return;
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
            //SON
            AudioManager.instance.PlaySound(3, 10, gameObject, 0.15f, false);
        }

        crouchedCollider.enabled = state == PlayerStates.Crouching;
        standingCollider.enabled = state != PlayerStates.Crouching;
    }

    private void ToggleSprint(InputAction.CallbackContext obj)
    {
        if (!canMove) return;
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
            //SON
        }
    }


    [HideInInspector] public Camera camera1;
    
    private void Interact(InputAction.CallbackContext obj)
    {
        if (!canMove && !lockedCam) return; //THOMAS
        if (tkManager.controlledProp) return;
        if (Physics.Raycast(playerCam.position, playerCam.forward, out RaycastHit hit, interactDistance, ~LayerMask.GetMask("Player")))
        {
            if (hit.rigidbody)
            {
                if (hit.transform.TryGetComponent(out ICanInteract interactable) && hit.rigidbody.velocity.y == 0)
                {
                    interactable.Interact(-hit.normal);
                }
            }
            else
            {
                if (hit.transform.TryGetComponent(out ICanInteract interactable))
                {
                    interactable.Interact(-hit.normal);
                }
            }
        }
    }

    private bool superShot;
    private void Shoot(InputAction.CallbackContext obj)
    {
        if (!canMove) return;
        if (reloading) return;
        if (socketManager.noBullets || socketManager.overheated)return;
        if (shootSpeedTimer > 0) return;
        if (stagger != null) StopCoroutine(stagger);
        stagger = StartCoroutine(StaggerSprint(state == PlayerStates.Sprinting));

        socketManager.ShootWithSocket(playerCam, shootingHand);

        
        // SON
        AudioManager.instance.PlaySound(3, 1, gameObject, 0.1f, false);
    }
    
    private bool reloading = false;
    public void RequestReload(InputAction.CallbackContext obj)
    {
        if (!canMove) return;
        if (reloading)return;
        if (currentInk < socketManager.reloadCostPerBullet) return;
        foreach (var socket in socketManager.sockets)
        {
            if (socket.state != ShootingHand.SocketStates.Empty)continue;
            reloading = true;
        }
        if (!reloading)return;
        
        //SON
       
        reloadCoroutine = StartCoroutine(Reload2());
    }
    private void UseHealPack(InputAction.CallbackContext obj)
    {
        if (!canMove) return;
        if (currentHealPackAmount <= 0 || currentHealth >= maxHealth) return;
        currentHealPackAmount--;
        GameManager.instance.UpdateHealPackUI();
        currentHealth += healAmount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        //SON
        AudioManager.instance.PlaySound(3, 16, gameObject, 0.1f, false);
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
            if (!tkManager.controlledProp)
            {
                
                tkManager.FindControllableProp();
                if(tkManager.controlledProp)animManager.LeftHand_Grab();
                return;
            }

            tkManager.TelekinesisPhysics();
        }
    }

    #endregion

    
    //THOMAS --> Permet de passer au son de bois
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("InteriorArea"))
        {
            isOnWood++;
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("InteriorArea"))
        {
            isOnWood--;
        }
    }

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

    #region Cinématique bandes noires
    
    public bool isControled;
    private bool isRepositiong;
    private bool isEndingCinematic;
    private Vector3 cinematicStartPos;
    private Vector3 cinematicStartDir;
    
    public void TakeControlIntroTornado(Vector3 startPos, Vector3 startDir)
    {
        cinematicStartPos = startPos;
        cinematicStartDir = startDir;
        canMove = false; 
        StartCoroutine(TakeControlCoroutine());
    }

    private IEnumerator TakeControlCoroutine()
    {
        isControled = true;
        
        yield return new WaitForSeconds(0.5f);

        state = PlayerStates.Standing;

        isRepositiong = true;
        
        yield return new WaitForSeconds(Vector3.Distance(cinematicStartPos, transform.position) * 1.5f);

        isRepositiong = false;

        yield return new WaitForSeconds(4f);

        StartCoroutine(StopControlIntroTornado());
    }

    private IEnumerator StopControlIntroTornado()
    {
        isEndingCinematic = true;
        isRepositiong = false;

        yield return new WaitForSeconds(2f);
        
        canMove = true;
        isControled = false;
        isEndingCinematic = false;

        playerDir += Vector3.zero;
        appliedForce = false;
        HorizontalMovement();
    }
    
    #endregion
    
    #region Save
    
    public float GetInk()
    {
        return currentInk;
    }

    public int GetHealKits()
    {
        return currentHealPackAmount;
    }
    
    #endregion
}
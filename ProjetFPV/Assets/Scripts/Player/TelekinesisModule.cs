using System;
using System.Threading.Tasks;
using DG.Tweening;
using Mechanics;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TelekinesisModule : MonoBehaviour
{

    [Serializable]
    public class HealSlot
    {
        public MeshRenderer mesh;

        public bool full;
        public float fillTime;
        public float timer;
        public async void UpdateVisual()
        {
            while (Application.isPlaying)
            {
                await Task.Delay(10);
                if (full)
                {
                    if (timer < fillTime)
                    {
                        timer += Time.deltaTime;
                    }
                }
                else
                {
                    if (timer > 0)
                    {
                        timer -= Time.deltaTime;
                    }
                }

                if (Application.isPlaying)
                {
                    mesh.material.SetFloat(
                        "_Range",
                        Mathf.Lerp(0, 1, timer/fillTime));
                }
            }
            
        }
    }
    #region Variables

    [Foldout("Refs")] [SerializeField] private ParticleSystem[] VFX_TKStart;
    [Foldout("Refs")] [SerializeField] private ParticleSystem VFX_TKEnd;
    
    [Foldout("Refs")] [SerializeField] public SkinnedMeshRenderer leftHandModule;
    [Foldout("Refs")] [SerializeField] public TextMeshProUGUI moduleText;
    
    [Foldout("Refs")] [SerializeField] public HealSlot[] healSlots;
    
    private PlayerController main;
    public ControllableProp controlledProp;

    private Transform offsetPosition;

    [Foldout("Telekinesis")]
    [Tooltip("How fast the targeted TelekinesisObject travels to the resting position")]
    [SerializeField]
    private float regularTravelSpeed;

    [Foldout("Telekinesis")]
    [Tooltip("How fast the targeted HeavyObject travels to the resting position")]
    [SerializeField]
    private float heavyTravelSpeed;

    [Foldout("Telekinesis")]
    [Tooltip(
        "*KEEP THIS VARIABLE LOW* Minimum distance between the grabbed object and the resting position before the object is considered as 'grabbed'")]
    [SerializeField]
    private float grabDistanceBuffer;

    [Foldout("Telekinesis")] [Tooltip("Cost per second of using telekinesis on an object")] [SerializeField]
    private float holdObjectCost;

    [Foldout("Telekinesis")] [Tooltip("Cost per second of using telekinesis on an enemy")] [SerializeField]
    private float holdEnemyCost;

    [Foldout("Telekinesis")] [SerializeField] [Range(0, 1)]
    public float holdObjectYTolerance = 0.6f;

    [Foldout("Telekinesis")] [Tooltip("Cost of releasing an object from telekinesis")] [SerializeField]
    private float throwCost;

    [Foldout("Telekinesis")]
    [Tooltip("Force applied to grabbed object when released from telekinesis")]
    [SerializeField]
    private float throwForce;

    [Foldout("Telekinesis")] [SerializeField]
    public float inkAbsorbSpeed;

    private bool isGrabbingAnObject;
    private GameObject holdTKAudio;

    // Tess Line VFX
    [SerializeField] private LineRenderer lineVFX;
    [SerializeField] private float currentLineVFXValue;
    [SerializeField] private float sliderFillSpeed = 1;
    private Vector2 minMaxLineVFXSliderValue;
    private static readonly int Slider = Shader.PropertyToID("Slider");
    public GameObject vfxHandz;

    private RaycastHit hitTelekinesis;

    #endregion


    public const float zeroInkFill = 0f;
    public const float fullInkFill = 1f;

    // Start is called before the first frame update
    void Start()
    {
        main = GetComponent<PlayerController>();
        offsetPosition = main.offsetPosition;
        currentLineVFXValue = 0;
        foreach (var slot in healSlots)
        {
            slot.UpdateVisual();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateTKCylinder();
        UpdateLineVFX();

        if (GameManager.instance.ending)
        {
            TelekinesisPhysics();
        }
    }

    private void ShowLineVFX(Collider tkObject)
    {
        tempColl = tkObject;
        
        lineVFX.gameObject.SetActive(true);
        
        vfxHandz.SetActive(true);
        vfxHandz.transform.localPosition = Vector3.zero;
        vfxHandz.transform.DOMove(tempColl.transform.position, sliderFillSpeed);
        
        VFX_TKStart[0].Play();
        VFX_TKStart[1].Play();
    }


    public void UpdateHealPackVisual()
    {
        for (int i = 0; i < healSlots.Length; i++)
        {
            if (i + 1 <= main.currentHealPackAmount)
            {
                healSlots[i].full = true;
            }
            else
            {
                healSlots[i].full = false;
            }

            
        }
    }
    
    private void HideLineVFX()
    {
        lineVFX.gameObject.SetActive(false);
        
        vfxHandz.SetActive(false);
        currentLineVFXValue = minMaxLineVFXSliderValue.x;
        foreach (var vfx in VFX_TKStart)
        {
            vfx.Stop();
            vfx.SetParticles(null, 0);
        }
        Destroy(holdTKAudio);
        VFX_TKEnd.Play();

    }

    private void UpdateLineVFX()
    {
        if (!lineVFX.gameObject.activeSelf)return;

        if (currentLineVFXValue < minMaxLineVFXSliderValue.y)
        {
            currentLineVFXValue += Time.deltaTime / sliderFillSpeed;
            
            lineVFX.material.SetFloat(Slider, currentLineVFXValue);
        }

        Physics.Linecast(tkSocket.position,tempColl.transform.position + tempColl.transform.up *0.1f, out RaycastHit hit, LayerMask.GetMask(LayerMask.LayerToName(tempColl.gameObject.layer)));
        tkPoint = hit.point;
        
        lineVFX.SetPosition(0,tkSocket.position);
        lineVFX.SetPosition(1,tkPoint);
        
        VFX_TKStart[1].transform.position = tkPoint;
    }

    public void FindControllableProp()
    {
        if (Physics.Raycast(main.playerCam.position, main.playerCam.forward, out hitTelekinesis,
                main.socketManager.maxRange, main.socketManager.shootMask, QueryTriggerInteraction.Ignore))
        {
            isGrabbingAnObject = true;

            CameraShake.instance.ShakeOneShot(2);
            if (hitTelekinesis.collider.TryGetComponent(out TelekinesisObject TK))
            {
                if (!TK.canBeGrabbed) return;
                controlledProp = TK;
                controlledProp.ApplyTelekinesis();

               
                ShowLineVFX(hitTelekinesis.collider);
                AudioManager.instance.PlaySound(3, 13, gameObject, 0.1f, false);
                return;
            }

            if (hitTelekinesis.collider.TryGetComponent(out HeavyObject heavy))
            {
                if (!heavy.canBeGrabbed) return;
                if (heavy.transform.position.y < transform.position.y) return;
                controlledProp = heavy;
                controlledProp.ApplyTelekinesis();

                ShowLineVFX(hitTelekinesis.collider);
                AudioManager.instance.PlaySound(3, 13, gameObject, 0.1f, false);
                return;
            }

            if (hitTelekinesis.collider.TryGetComponent(out AbsorbInk absorb))
            {
                if (!absorb.canBeGrabbed) return;
                controlledProp = absorb;
                absorb.StartAbsorbInk();
                controlledProp.ApplyTelekinesis();

                ShowLineVFX(hitTelekinesis.collider);
                AudioManager.instance.PlaySound(3, 13, gameObject, 0.1f, false);
                AudioManager.instance.PlaySound(3, 2, gameObject, 0.1f, false);
                if (hitTelekinesis.collider.TryGetComponent(out Tornado tornado))
                {
                    controlledProp.ApplyTelekinesis();
                }
                return;
            }

            if (hitTelekinesis.collider.TryGetComponent(out UnstableObject unstable))
            {
                if (!unstable.canBeGrabbed) return;
                controlledProp = unstable;
                controlledProp.ApplyTelekinesis();
                
                ShowLineVFX(hitTelekinesis.collider);
                AudioManager.instance.PlaySound(3, 13, gameObject, 0.1f, false);
                return;
            }

            if (hitTelekinesis.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                var enemy = hitTelekinesis.collider.GetComponentInParent<Enemy>();
                if (!enemy.canBeGrabbed) return;
                if (main.currentInk < 0.1f) return;
                controlledProp = enemy;
                ShowLineVFX(hitTelekinesis.collider);
                controlledProp.ApplyTelekinesis();

                AudioManager.instance.PlaySound(3, 13, gameObject, 0.1f, false);
                return;
            }
        }
    }

    private Vector3 tempWorldToScreen;

    public void TelekinesisPhysics()
    {
        switch (controlledProp)
        {
            case TelekinesisObject tkObject:
                Hold_TelekinesisObject(tkObject);
                return;

            case AbsorbInk absorbInk:
                Hold_AbsorbInk(absorbInk);
                return;

            case Enemy enemy:
                Hold_Enemy(enemy);
                break;

            case HeavyObject heavy:
                Hold_HeavyObject(heavy);
                return;
            case UnstableObject unstable:
                return;
        }

        if (main.currentInk <= 0.1f)
        {
            CameraShake.instance.StopInfiniteShake();
            controlledProp.ApplyTelekinesis();
            controlledProp.isGrabbed = false;
            controlledProp = null;
            main.recentlyDepletedStamina = true;
            main.animManager.LeftHand_Release();
            HideLineVFX();
        }
    }


    #region Hold

    

    void Hold_AbsorbInk(AbsorbInk absorbInk)
    {
        absorbInk.storedInk -= inkAbsorbSpeed * Time.deltaTime;
        main.currentInk = GameManager.instance.UpdatePlayerStamina(main.currentInk, main.maxInk, 
            (PlayerPrefs.GetInt("difficulty") == 0 ? inkAbsorbSpeed : inkAbsorbSpeed * 3f) * Time.deltaTime);
        var lerpValue = Mathf.Clamp(1 - absorbInk.storedInk / absorbInk.maxInk, 0, 0.8f);

        if (absorbInk.isADecal)
        {
            absorbInk.decal.size = Vector3.Lerp(absorbInk.baseDecalScale, Vector3.zero, lerpValue);
            absorbInk.decal.fadeFactor = Mathf.Lerp(1f, 0f, lerpValue);
        }
        else
        {
            absorbInk.transform.localScale = Vector3.Lerp(absorbInk.baseGOScale, Vector3.zero, lerpValue);
        }

        if (absorbInk.TryGetComponent(out Tornado tornado))
        {
            if (GameManager.instance.canStartEndingCinematic && !GameManager.instance.ending)
            {
                StartCoroutine(GameManager.instance.StartEndingCinematic());
                
            } 
            absorbInk.storedInk += inkAbsorbSpeed * Time.deltaTime;
        }
        
        if (!controlledProp.isGrabbed)
        {
            controlledProp.isGrabbed = true;
            CameraShake.instance.StartInfiniteShake(0);
        }

        if (absorbInk.storedInk < 0)
        {
            main.recentlyDepletedStamina = true;
            main.ReleaseProp(new InputAction.CallbackContext());
        }

    }

    void Hold_Enemy(Enemy enemy)
    {
        main.currentInk =
            GameManager.instance.UpdatePlayerStamina(main.currentInk, main.maxInk, -holdEnemyCost * Time.deltaTime);

        tempWorldToScreen = main.camera1.WorldToScreenPoint(controlledProp.transform.position);
        if (tempWorldToScreen.x < 0 || tempWorldToScreen.x > Screen.width ||
            tempWorldToScreen.y < 0 || tempWorldToScreen.y > Screen.height ||
            tempWorldToScreen.z < 0)
        {
            main.recentlyDepletedStamina = true;
            main.ReleaseProp(new InputAction.CallbackContext());
            return;
        }

        Vector3 dir2 = controlledProp.transform.position - transform.position;
        if (Physics.Raycast(controlledProp.transform.position, -dir2.normalized, out RaycastHit hit,
                main.socketManager.maxRange,
                main.playerLayer))
        {
            if (!hit.collider.TryGetComponent(out PlayerController controller))
            {
                main.recentlyDepletedStamina = true;
                main.ReleaseProp(new InputAction.CallbackContext());
                return;
            }
        }

        //SON
        if (enemy.isGrabbed) return;


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
    }

    void Hold_TelekinesisObject(TelekinesisObject telekinesisObject)
    {
        main.currentInk =
            GameManager.instance.UpdatePlayerStamina(main.currentInk, main.maxInk, -holdObjectCost);

        var dir = offsetPosition.position - controlledProp.transform.position;
        dir.Normalize();
        if (main.playerCam.forward.y < -holdObjectYTolerance)
        {
            CameraShake.instance.StopInfiniteShake();
            main.recentlyDepletedStamina = true;
            controlledProp.ApplyTelekinesis();
            controlledProp.isGrabbed = false;
            controlledProp = null;
            HideLineVFX();
            main.animManager.LeftHand_Reset();
            return;
        }


        if (!controlledProp.isGrabbed)
        {
            controlledProp.body.velocity = dir * regularTravelSpeed;
        }
        else
        {

            controlledProp.body.angularVelocity = (controlledProp.body.angularVelocity.normalized + controlledProp.transform.up * 0.01f) *
                                                  Mathf.Clamp(controlledProp.body.angularVelocity.magnitude,
                                                      telekinesisObject.minRotationSpeed,
                                                      telekinesisObject.maxRotationSpeed);
            
            
            controlledProp.body.velocity = dir * (regularTravelSpeed *
                                                  (Vector3.Distance(controlledProp.transform.position,
                                                      offsetPosition.position) / grabDistanceBuffer));

            if (isGrabbingAnObject)
            {
                holdTKAudio = AudioManager.instance.PlaySound(3, 14, gameObject, 0.2f, true);

                isGrabbingAnObject = false;
            }

            //SON
            return;
        }

        if (grabDistanceBuffer > Vector3.Distance(controlledProp.transform.position, offsetPosition.position))
        {
            controlledProp.isGrabbed = true;
            CameraShake.instance.StartInfiniteShake(0);
        }


    }

    void Hold_HeavyObject(HeavyObject heavy)
    {
        var dir = transform.position + transform.forward * heavy.restingDistanceToPlayer -
                  controlledProp.transform.position;
        dir.Normalize();

        var magnitude = Mathf.Clamp(heavyTravelSpeed *
                                    (Vector3.Distance(controlledProp.transform.position,
                                        offsetPosition.position) / grabDistanceBuffer), heavyTravelSpeed * 0.3f,
            heavyTravelSpeed * 2f);

        controlledProp.body.velocity = dir * magnitude;


        if (controlledProp.isGrabbed) return;
        if (grabDistanceBuffer > Vector3.Distance(controlledProp.transform.position,
                transform.position + transform.forward * heavy.restingDistanceToPlayer))
        {
            controlledProp.isGrabbed = true;
            CameraShake.instance.StartInfiniteShake(0);
        }
    }

    #endregion
    public void ReleaseProp()
    {
        main.recentlyDepletedStamina = false;
        if (controlledProp == null)
        {
            CameraShake.instance.ResetCoroutine();
            return;
        }

        HideLineVFX(); // THOMAS
        if (main.currentInk < 1)
        {
            CameraShake.instance.StopInfiniteShake();
            main.recentlyDepletedStamina = true;
            controlledProp.ApplyTelekinesis();
            controlledProp.body.velocity *= 0.1f;
            controlledProp.isGrabbed = false;
            controlledProp = null;
            main.animManager.LeftHand_Reset();
            return;
        }

        AudioManager.instance.PlaySound(3, 15, gameObject, 0.2f, false);

        switch (controlledProp)
        {
            case TelekinesisObject tkObject:
                Release_TKObject(tkObject);
                break;
            case Enemy:
                Release_Enemy();
                break;

            case AbsorbInk absorbInk:
                Release_AbsorbInk(absorbInk);
                if (main.currentInk < 0) main.currentInk = 0;
                StartCoroutine(controlledProp.BufferGrabbing());
                controlledProp = null;
                CameraShake.instance.StopInfiniteShake();
                main.animManager.LeftHand_Reset();
                return;
            case HeavyObject heavy:
                Release_HeavyObject();
                break;
            case UnstableObject unstable:
                Release_ObjectToFall(unstable);
                break;
        }

        if (main.currentInk < 0) main.currentInk = 0;
        StartCoroutine(controlledProp.BufferGrabbing());
        controlledProp = null;
        CameraShake.instance.StopInfiniteShake();
        main.animManager.LeftHand_Release();


        isGrabbingAnObject = false;
        //SON
    }

    public void Release_TKObject(TelekinesisObject telekinesisObject)
    {
        if (!controlledProp.isGrabbed)
        {
            controlledProp.body.velocity *= 0.1f;
        }
        else
        {
            controlledProp.isGrabbed = false;

            main.currentInk =
                GameManager.instance.UpdatePlayerStamina(main.currentInk, main.maxInk, -throwCost);

            controlledProp.body.velocity = Vector3.zero;

            var dir = Vector3.zero;
            if (Physics.Raycast(main.playerCam.position, main.playerCam.forward, out RaycastHit hit,
                    main.socketManager.maxRange,
                    ~LayerMask.GetMask("Telekinesis")))
            {
                if (hit.collider.gameObject.layer == LayerMask.GetMask("Enemy"))
                    telekinesisObject.AimAtEnemy(hit.transform);
                dir = (hit.point + hit.normal * 0.5f) - offsetPosition.position;
                Debug.DrawRay(hit.point, hit.normal * 2, Color.magenta, 2);
            }
            else
            {
                dir = main.playerCam.forward;
            }

            dir.Normalize();
            telekinesisObject.body.AddForce(dir * throwForce, ForceMode.Impulse);
        }

        controlledProp.ApplyTelekinesis();
    }

    public void Release_AbsorbInk(AbsorbInk absorbInk)
    {
        absorbInk.StopAbsorbing();
        
        absorbInk.isGrabbed = false;
        if (absorbInk.storedInk < 0)
        {
            if (!absorbInk.respawn)
            {
                Destroy(absorbInk.gameObject);
            }
        }
    }

    public void Release_Enemy()
    {
        controlledProp.isGrabbed = false;
        controlledProp.ApplyTelekinesis();
    }

    public void Release_HeavyObject()
    {
        controlledProp.isGrabbed = false;
        controlledProp.ApplyTelekinesis();
    }

    public void Release_ObjectToFall(UnstableObject unstable)
    {
        controlledProp.ApplyTelekinesis();
    }

    [SerializeField] private Transform tkSocket; // THOMAS 
    [SerializeField] private GameObject cylinderPrefab; // THOMAS
    [Range(0f, 5f)] [SerializeField] private float tkCylinderSize;
    [Range(0f, 10f)] [SerializeField] private float tkCylinderExpansionSpeed = 5;
    private GameObject tkCylinder; // THOMAS 
    private Vector3 tkPoint; // THOMAS 
    private Collider tempColl;


    private float cylinderTimer;

    void CreateCylinder(Collider tkColl) // THOMAS (whole method)
    {
        if (tkCylinder != null)
        {
            Destroy(tkCylinder);
        }

        tempColl = tkColl;
        cylinderTimer = 0;
        tkPoint = tempColl.ClosestPoint(tkSocket.position);

        var cylinder = Instantiate(cylinderPrefab, tkSocket.position, Quaternion.identity);

        cylinder.transform.forward = tkPoint - tkSocket.position;
        cylinder.transform.localScale = new Vector3(tkCylinderSize, tkCylinderSize,
            Vector3.Distance(tkPoint, tkSocket.position) / 2f);
        tkCylinder = cylinder;

        VFX_TKStart[0].Play();
        VFX_TKStart[1].Play();

    }

    void UpdateTKCylinder() // THOMAS (whole method)
    {
        if (tkCylinder == null) return;
        if (cylinderTimer < 1)
        {
            cylinderTimer += tkCylinderExpansionSpeed * Time.deltaTime;
        }

        tkPoint = tempColl.ClosestPoint(tkSocket.position);
        VFX_TKStart[1].transform.position = tkPoint;
        tkCylinder.transform.position = tkSocket.position;
        tkCylinder.transform.forward = tkPoint - tkSocket.position;
        tkCylinder.transform.localScale = new Vector3(tkCylinderSize * cylinderTimer, tkCylinderSize * cylinderTimer,
            Vector3.Distance(tkPoint, tkSocket.position) / 2f);
    }

    public void ThrowTKObject() // THOMAS (whole method)
    {
        Destroy(holdTKAudio);

        if (tkCylinder == null) return;
        foreach (var vfx in VFX_TKStart)
        {
            vfx.Stop();
            vfx.SetParticles(null, 0);
        }

        VFX_TKEnd.Play();

        Destroy(tkCylinder);
    }
}
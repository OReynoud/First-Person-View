using System.Collections;
using System.Collections.Generic;
using Mechanics;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class TelekinesisModule : MonoBehaviour
{
    
    #region Variables

    [Foldout("Refs")] [SerializeField] private ParticleSystem[] VFX_TKStart;
    [Foldout("Refs")] [SerializeField] private ParticleSystem VFX_TKEnd;
    private PlayerController main;
    public ControllableProp controlledProp;

    private Transform offsetPosition;
    
    [Foldout("Telekinesis")] [Tooltip("How fast the targeted TelekinesisObject travels to the resting position")] [SerializeField]
    private float regularTravelSpeed;
    
    [Foldout("Telekinesis")] [Tooltip("How fast the targeted HeavyObject travels to the resting position")] [SerializeField]
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

    
    
    // Start is called before the first frame update
    void Start()
    {
        main = GetComponent<PlayerController>();
        offsetPosition = main.offsetPosition;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTKCylinder();
    }
    public void FindControllableProp()
    {
        if (Physics.Raycast(main.playerCam.position, main.playerCam.forward, out RaycastHit hit, main.socketManager.maxRange, main.socketManager.shootMask))
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
            
            if (hit.collider.TryGetComponent(out HeavyObject heavy))
            {
                if (!heavy.canBeGrabbed) return;
                controlledProp = heavy;
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
             
            if (hit.collider.TryGetComponent(out UnstableObject unstable))
            {
                if (!unstable.canBeGrabbed) return;
                controlledProp = unstable;
                controlledProp.ApplyTelekinesis();
                return;
            }
                    
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                var enemy = hit.collider.GetComponentInParent<Enemy>();
                if (!enemy.canBeGrabbed) return;
                if (main.currentInk < 1)return;
                controlledProp = enemy;
                controlledProp.ApplyTelekinesis();
                        
                CreateCylinder(hit.collider);
                return;
            }


        }
    }

    private Vector3 tempWorldToScreen;

    public void TelekinesisPhysics()
    {
        switch (controlledProp)
        {
            case TelekinesisObject:
                
                main.currentInk =
                    GameManager.instance.UpdatePlayerStamina(main.currentInk, main.maxInk, -holdObjectCost);
                
                var dir = offsetPosition.position - controlledProp.transform.position;
                dir.Normalize();
                if (main.playerCam.forward.y < -holdObjectYTolerance)
                {
                    CameraShake.instance.StopInfiniteShake();
                    controlledProp.ApplyTelekinesis();
                    controlledProp.isGrabbed = false;
                    controlledProp = null;
                    main.recentlyDepletedStamina = true;
                    return;
                }

                if (!controlledProp.isGrabbed)
                {
                    controlledProp.body.velocity = dir * regularTravelSpeed;
                }
                else
                {
                    controlledProp.body.velocity = dir * (regularTravelSpeed *
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
                main.currentInk =
                    GameManager.instance.UpdatePlayerStamina(main.currentInk, main.maxInk, inkAbsorbSpeed * Time.deltaTime);
                var lerpValue = Mathf.Clamp(1 - absorbInk.storedInk / absorbInk.maxInk, 0, 0.8f);
                absorbInk.transform.localScale = Vector3.Lerp(absorbInk.baseScale, Vector3.zero, lerpValue);

                if (!controlledProp.isGrabbed)
                {
                    controlledProp.isGrabbed = true;
                    CameraShake.instance.StartInfiniteShake(0);
                }

                if (absorbInk.storedInk < 0)
                {
                    main.recentlyDepletedStamina = true;
                    main.ReleaseProp(new InputAction.CallbackContext());
                    return;
                }

                return;

            case Enemy enemy:
                
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
            
            case HeavyObject heavy:
                Hold_HeavyObject(heavy);
                return;
        }

        if (main.currentInk <= 0.1f)
        {
            CameraShake.instance.StopInfiniteShake();
            controlledProp.ApplyTelekinesis();
            controlledProp.isGrabbed = false;
            controlledProp = null;
            main.recentlyDepletedStamina = true;
        }
    }


    void Hold_HeavyObject(HeavyObject heavy)
    {                
        var dir = transform.position + transform.forward * heavy.restingDistanceToPlayer - controlledProp.transform.position;
        dir.Normalize();

        var magnitude = Mathf.Clamp(heavyTravelSpeed *
                         (Vector3.Distance(controlledProp.transform.position,
                             offsetPosition.position) / grabDistanceBuffer),heavyTravelSpeed * 0.3f,heavyTravelSpeed * 2f);
        
        controlledProp.body.velocity = dir * magnitude;
        

        if (controlledProp.isGrabbed) return;
        if (grabDistanceBuffer > Vector3.Distance(controlledProp.transform.position, transform.position + transform.forward * heavy.restingDistanceToPlayer))
        {
            controlledProp.isGrabbed = true;
            CameraShake.instance.StartInfiniteShake(0);
        }
    }
    
    public void ReleaseProp()
    {
        main.recentlyDepletedStamina = false;
        if (controlledProp == null)
        {
            CameraShake.instance.ResetCoroutine();
            return;
        }
        
        ThrowTKObject(); // THOMAS
        if (main.currentInk < 1)
        {
            CameraShake.instance.StopInfiniteShake();
            controlledProp.ApplyTelekinesis();
            controlledProp.isGrabbed = false;
            controlledProp = null;
            main.recentlyDepletedStamina = true;
            return;
        }

        switch (controlledProp)
        {
            case TelekinesisObject:
                Release_TKObject();
                break;
            case Enemy:
                Release_Enemy();
                break;
            
            case AbsorbInk absorbInk:
                Release_AbsorbInk(absorbInk);
                break;
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
    }

    public void Release_TKObject()
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
            if (Physics.Raycast(main.playerCam.position, main.playerCam.forward, out RaycastHit hit, main.socketManager.maxRange,
                    ~LayerMask.GetMask("Telekinesis")))
            {
                dir = (hit.point + hit.normal * 0.5f) - offsetPosition.position;
                Debug.DrawRay(hit.point,hit.normal * 2, Color.magenta,2);
            }
            else
            {
                dir = main.playerCam.forward;
            }
                    
            dir.Normalize();
            controlledProp.body.AddForce(dir * throwForce, ForceMode.Impulse);
        }

        controlledProp.ApplyTelekinesis();
    }
    public void Release_AbsorbInk(AbsorbInk absorbInk)
    {
        absorbInk.isGrabbed = false;
        if (absorbInk.storedInk < 0)
        {
            Destroy(absorbInk.gameObject);
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
}
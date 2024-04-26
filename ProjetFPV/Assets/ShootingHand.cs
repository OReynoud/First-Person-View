using System;
using System.Collections;
using System.Collections.Generic;
using Mechanics;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class ShootingHand : MonoBehaviour
{
    public AmmoSocket currentSocket;

    public enum SocketStates
    {
        Empty,
        Loaded,
        SuperCharged
    }

    public List<AmmoSocket> sockets = new List<AmmoSocket>();

    [Foldout("Shoot")] [SerializeField]
    public float bulletSpeed;
    
    [Foldout("Shoot")] [Tooltip("Which layers will get hit by the hit scan")] [SerializeField]
    public LayerMask shootMask;

    [Foldout("Shoot")] [Tooltip("Range of the hit scan")] [SerializeField]
    public float maxRange;

    [Foldout("Shoot")] [Tooltip("Cost to convert ink into bullets")] [SerializeField]
    public float reloadCostPerBullet;
    [Foldout("Shoot")] [SerializeField] private float surplusBulletCost;
    [Foldout("Shoot")] [SerializeField] private float trailTime;
    

    [Foldout("Refs")] public Material emptySocket;
    [Foldout("Refs")] public Material loadedSocket;
    [Foldout("Refs")] public Material superChargedSocket;
    [Foldout("Refs")] [SerializeField] private PlayerBullet bulletPrefab;
    [Foldout("Refs")] [SerializeField] private LineRenderer normalTrail;
    [Foldout("Refs")] [SerializeField] private LineRenderer superTrail;
    private LineRenderer currentTrail;


    [InfoBox("Si coché, des balles super-chargées seront périodiquement et automatiquement insérés dans les cartouches vides en utilisant le surplus d'encre")]
    [BoxGroup("AutoLoad")]
    public bool autoLoadOnSurplus;


    [ShowIf("autoLoadOnSurplus")][BoxGroup("AutoLoad")] public float timeToAutoLoad = 0.3f;
    private float autoLoadTimer;

    [InfoBox("Si coché, pendant la recharge des balles super-chargées seront chargées en utilisant le surplus d'encre. La durée de la recharge pendant le surplus peut etre modifiée pour être plus courte")]
    [BoxGroup("Surplus Reload")]
    public bool reloadSuperBulletsOnSurplus;

    [ShowIf("reloadSuperBulletsOnSurplus")]
    [BoxGroup("Surplus Reload")]
    public float surplusReloadTime = 0.5f;

    private PlayerController player;


    public void Awake()
    {
        player = GetComponent<PlayerController>();
        currentSocket = sockets[0];
        foreach (var ammo in sockets)
        {
            ammo.highlightMesh.enabled = false;
            ammo.state = SocketStates.Loaded;
        }

        currentSocket.highlightMesh.enabled = true;
    }


    [HideInInspector] public bool noBullets;


    public void Update()
    {
        if (autoLoadOnSurplus && player.inSurplus)
        {
            autoLoadTimer += Time.deltaTime;
            if (autoLoadTimer >= timeToAutoLoad)
            {
                autoLoadTimer = 0;

                for (int i = sockets.Count - 1; i >= 0; i--)
                {
                    if (sockets[i].state != SocketStates.Empty) continue;

                    sockets[i].socketMesh.material = superChargedSocket;
                    decrement = (reloadCostPerBullet + surplusBulletCost);
                    sockets[i].state = SocketStates.SuperCharged;
                    player.currentInk =
                        GameManager.instance.UpdatePlayerStamina(player.currentInk, player.maxInk, -decrement);
                    noBullets = false;
                    break;
                }
            }
        }
    }

    void UpdateCurrentSocket() //Pouce, majeur, annulaire, auriculaire. Haha je suis drole
    {
        currentSocket.highlightMesh.enabled = false;
        currentSocket.socketMesh.material = emptySocket;
        currentSocket.state = SocketStates.Empty;
        noBullets = true;
        for (int i = 0; i < sockets.Count; i++)
        {
            if (sockets[i].state == SocketStates.Empty) continue;
            noBullets = false;
            currentSocket = sockets[i];
            break;
        }


        if (noBullets)
            player.RequestReload(new InputAction.CallbackContext());

        currentSocket.highlightMesh.enabled = true;
    }

    private float decrement = 0;

    public void ReloadSockets()
    {
        for (int i = sockets.Count - 1; i >= 0; i--)
        {
            if (reloadCostPerBullet > player.currentInk) break;

            if (sockets[i].state != SocketStates.Empty) continue;

            if (reloadSuperBulletsOnSurplus && player.inSurplus)
            {
                sockets[i].socketMesh.material = superChargedSocket;
                decrement = (reloadCostPerBullet + surplusBulletCost);
                sockets[i].state = SocketStates.SuperCharged;
            }
            else
            {
                sockets[i].socketMesh.material = loadedSocket;
                decrement = reloadCostPerBullet;
                sockets[i].state = SocketStates.Loaded;
            }

            player.currentInk =
                GameManager.instance.UpdatePlayerStamina(player.currentInk, player.maxInk, -decrement);
        }

        noBullets = false;
        currentSocket.highlightMesh.enabled = false;
        foreach (var ammo in sockets)
        {
            if (ammo.state == SocketStates.Empty) continue;
            currentSocket = ammo;
            currentSocket.highlightMesh.enabled = true;
            break;
        }
    }

    public void ShootWithSocket(Transform cam, Transform origin)
    {
        CameraShake.instance.ShakeOneShot(1);

        if (currentSocket.state == SocketStates.Loaded)
        {
            currentTrail = Instantiate(normalTrail);
        }
        else
        {
            currentTrail = Instantiate(superTrail);
        }

        Destroy(currentTrail.gameObject, trailTime);
        currentTrail.SetPosition(0, origin.position + origin.up * 0.5f);

        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, maxRange, shootMask))
        {

        }
        else
        {
            Debug.Log("Hit some air");
        }
        currentTrail.SetPosition(1, cam.forward * maxRange + cam.position);

        UpdateCurrentSocket();
    }
}
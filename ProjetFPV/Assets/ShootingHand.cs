using System;
using System.Collections;
using System.Collections.Generic;
using Mechanics;
using NaughtyAttributes;
using Player;
using Unity.Properties;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ShootingHand : MonoBehaviour
{
    public AmmoSocket currentSocket;
    public bool useHitScan = true;
    public ParticleSystem[] bulletParticle;

    public enum SocketStates
    {
        Empty,
        Loaded,
        SuperCharged
    }

    public List<AmmoSocket> sockets = new List<AmmoSocket>();

    [HideIf("useHitScan")] [Foldout("Shoot")] [SerializeField]
    public float bulletSpeed;

    [Foldout("Shoot")] [Tooltip("Which layers will get hit by the hit scan")] [SerializeField]
    public LayerMask shootMask;

    [Foldout("Shoot")] [Tooltip("Range of the hit scan")] [SerializeField]
    public float maxRange;

    [Foldout("Shoot")] [Tooltip("Cost to convert ink into bullets")] [SerializeField]
    public float reloadCostPerBullet;

    [ShowIf("useHitScan")] [Foldout("Shoot")] [SerializeField] 
    private float trailTime;
    [Foldout("Shoot")] [SerializeField] private float damage;
    [Foldout("Shoot")] [SerializeField] private float knockBack;

    [Foldout("Surplus")] [SerializeField] private float surplusShotDamage;
    [Foldout("Surplus")] [SerializeField] private float surplusKnockBack;
    [Foldout("Surplus")] [HideInInspector] public Vector3 hitConeSize;
    [Foldout("Surplus")] [SerializeField] private float overheatTime;
    [Foldout("Surplus")] [SerializeField][Range(1,100)] private float percentInkCost = 50;



    [Foldout("Refs")] public Material emptySocket;
    [Foldout("Refs")] public Material loadedSocket;
    [Foldout("Refs")] public Material superChargedSocket;
    [Foldout("Refs")] public GameObject overHeatFeedback;

    [Foldout("Refs")] [SerializeField] private PlayerBullet bulletPrefab;
    [Foldout("Refs")] [SerializeField] private PlayerSuperShot superShot;
    [Foldout("Refs")] [SerializeField] private LineRenderer normalTrail;
    [Foldout("Refs")] [SerializeField] private LineRenderer superTrail;
    private LineRenderer currentTrail;


    [InfoBox(
        "Si coché, des balles super-chargées seront périodiquement et automatiquement insérés dans les cartouches vides en utilisant le surplus d'encre")]
    [BoxGroup("AutoLoad")]
    public bool autoLoadOnSurplus;


    [ShowIf("autoLoadOnSurplus")] [BoxGroup("AutoLoad")]
    public float timeToAutoLoad = 0.3f;

    private float autoLoadTimer;

    [InfoBox(
        "Si coché, pendant la recharge des balles super-chargées seront chargées en utilisant le surplus d'encre. La durée de la recharge pendant le surplus peut etre modifiée pour être plus courte")]
    [BoxGroup("Surplus Reload")]
    public bool reloadSuperBulletsOnSurplus;

    [ShowIf("reloadSuperBulletsOnSurplus")] [BoxGroup("Surplus Reload")]
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


        ParticleSystem oui = new ParticleSystem();
        var main = oui.main;
        foreach (var particle in bulletParticle)
        {
            main = particle.main;
            main.startSpeed = bulletSpeed;
            particle.Stop();
        }

        hitConeSize = superShot.transform.parent.transform.localScale;
    }

    //private ParticleSystem
    [HideInInspector] public bool noBullets;

    [HideInInspector] public bool overheated;


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
                    decrement = (reloadCostPerBullet);
                    sockets[i].state = SocketStates.SuperCharged;
                    player.currentInk =
                        GameManager.instance.UpdatePlayerStamina(player.currentInk, player.maxInk, -decrement);
                    noBullets = false;
                    break;
                }
            }
        }

        if (player.inSurplus)
        {
            currentSocket.highlightMesh.enabled = false;
            currentSocket = sockets[0];
            currentSocket.state = SocketStates.SuperCharged;
            currentSocket.socketMesh.material = superChargedSocket;
            currentSocket.highlightMesh.enabled = true;
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
                decrement = (reloadCostPerBullet);
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

    private Vector3 calculatedConeDimensions;

    public void ShootWithSocket(Transform cam, Transform origin)
    {
        CameraShake.instance.ShakeOneShot(1);

        if (currentSocket.state == SocketStates.SuperCharged)
        {
            SurplusShot();
            return;
        }

        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, maxRange, shootMask))
        {
            if (useHitScan)
            {
                currentTrail = Instantiate(currentSocket.state == SocketStates.Loaded ? normalTrail : superTrail);

                Destroy(currentTrail.gameObject, trailTime);
                currentTrail.SetPosition(0, origin.position + origin.up * 0.5f);


                currentTrail.SetPosition(1, cam.forward * maxRange + cam.position);

                Debug.Log("Hit something");
                if (hit.collider.CompareTag("Head"))
                {
                    if (hit.collider.transform.parent.TryGetComponent(out Enemy enemy))
                    {
                        enemy.TakeDamage(hit.collider, cam.forward, damage, knockBack);
                        GameManager.instance.HitMark(true);
                    }
                }

                if (hit.collider.TryGetComponent(out IDestructible target))
                {
                    target.TakeDamage();
                }

                currentTrail.SetPosition(1, hit.point);

                //Coucou, Thomas est passé par là (jusqu'au prochain commentaire)
                var decal = Instantiate(GameManager.instance.inkStainDecal, hit.point + hit.normal * 0.02f,
                    Quaternion.identity, hit.transform);
                decal.transform.forward = -hit.normal;
                decal.transform.RotateAround(decal.transform.position, decal.transform.forward,
                    Random.Range(-180f, 180f));
                //Je m'en vais !
            }
            else
            {
                var bullet = Instantiate(bulletPrefab, origin.position + origin.up * 0.5f, Quaternion.identity);
                bullet.transform.LookAt(hit.point);

                bullet.rb.velocity = bullet.transform.forward * bulletSpeed;
                bullet.superShot = currentSocket.state == SocketStates.SuperCharged;
                bullet.damage = damage;
                bullet.knockBack = knockBack;

                bullet.meshRenderer.material =
                    currentSocket.state == SocketStates.SuperCharged ? superChargedSocket : loadedSocket;
                bulletParticle[0].transform.LookAt(hit.point);


                var oui = bulletParticle[^1].main;
                var ohCestRelou = bulletParticle[0].transform.eulerAngles;
                oui.startRotationX = ohCestRelou.x;
                oui.startRotationY = ohCestRelou.y;
                oui.startRotationZ = ohCestRelou.z;
            }
        }
        else
        {
            bulletParticle[0].transform.localRotation = Quaternion.identity;
            Debug.Log("Hit some air");
        }

        bulletParticle[0].Play();


        UpdateCurrentSocket();
    }

    void SurplusShot()
    {

        player.currentInk =
            GameManager.instance.UpdatePlayerStamina(player.currentInk, player.maxInk,
                -player.maxInk * percentInkCost * 0.01f);


        superShot.damage = surplusShotDamage;
        superShot.knockBack = surplusKnockBack;

        calculatedConeDimensions = hitConeSize;
        // calculatedConeDimensions += Vector3.one * hitConeSizeIncrement * incrementAmount;
        // calculatedConeDimensions +=
        //     new Vector3(hitConeAngleIncrement, hitConeAngleIncrement, 0) * incrementAmount;

        superShot.scale = calculatedConeDimensions;
        superShot.gameObject.SetActive(true);
        StartCoroutine(OverheatCoroutine(overheatTime));
        player.inSurplus = false;


        UpdateCurrentSocket();
        
        
        
        
        
        //Debug.Log("Completed supershot with " + incrementAmount + " increments");
    }

    IEnumerator OverheatCoroutine(float time)
    {
        overheated = true;
        overHeatFeedback.SetActive(true);
        yield return new WaitForSeconds(time);
        overHeatFeedback.SetActive(false);
        overheated = false;
    }
}
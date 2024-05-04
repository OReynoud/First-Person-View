using System;
using System.Collections;
using System.Collections.Generic;
using Mechanics;
using NaughtyAttributes;
using Player;
using Unity.Properties;
using UnityEngine;
using UnityEngine.InputSystem;
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

    [HideIf("useHitScan")][Foldout("Shoot")] [SerializeField]
    public float bulletSpeed;
    
    [Foldout("Shoot")] [Tooltip("Which layers will get hit by the hit scan")] [SerializeField]
    public LayerMask shootMask;

    [Foldout("Shoot")] [Tooltip("Range of the hit scan")] [SerializeField]
    public float maxRange;

    [Foldout("Shoot")] [Tooltip("Cost to convert ink into bullets")] [SerializeField]
    public float reloadCostPerBullet;
    [Foldout("Shoot")] [SerializeField] private float trailTime;
    [Foldout("Shoot")] [SerializeField] private float baseDamage;
    [Foldout("Shoot")] [SerializeField] private float baseKnockBack;

    [Foldout("Surplus")] [SerializeField] private float damageIncrement;
    [Foldout("Surplus")] [SerializeField] private float knockBackIncrement;
    [Foldout("Surplus")] [SerializeField] private float hitConeRadius;
    [Foldout("Surplus")] [SerializeField] [Range (-1,1)] private float minDotProduct;
    [Foldout("Surplus")] [SerializeField] [Range (1,100)] private float incrementPercentCost;
    
    

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


    public class Bullet
    {
        private ParticleSystem.Particle projectile;
    }


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
    }
    
    //private ParticleSystem
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

    public void ShootWithSocket(Transform cam, Transform origin)
    {
        CameraShake.instance.ShakeOneShot(1);

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
                        if (currentSocket.state == SocketStates.Loaded)
                        {
                            enemy.TakeDamage(hit.collider, false, baseDamage, baseKnockBack);
                        }
                        else
                        {
                            var surplus = player.currentInk - player.maxInk;
                            int incrementAmount = 0;
                            while (surplus > incrementPercentCost)
                            {
                                surplus -= incrementPercentCost;
                                incrementAmount++;
                            }

                            player.currentInk = player.maxInk;
                            enemy.TakeDamage(hit.collider, true, baseDamage + damageIncrement * incrementAmount, baseKnockBack + knockBackIncrement * incrementAmount);
                        }

                        GameManager.instance.HitMark(true);
                    }
                }

                if (hit.collider.TryGetComponent(out IDestructible target))
                {
                    target.TakeDamage();
                }

                currentTrail.SetPosition(1, hit.point);

                //Coucou, Thomas est passé par là (jusqu'au prochain commentaire)
                var decal = Instantiate(GameManager.instance.inkStainDecal, hit.point + hit.normal * 0.02f, Quaternion.identity, hit.transform);
                decal.transform.forward = -hit.normal;
                decal.transform.RotateAround(decal.transform.position, decal.transform.forward, Random.Range(-180f, 180f));
                //Je m'en vais !
            }
            else
            {
                if (currentSocket.state == SocketStates.Loaded)
                {
                    var bullet = Instantiate(bulletPrefab, origin.position + origin.up * 0.5f, Quaternion.identity);
                    bullet.transform.LookAt(hit.point);
                
                    bullet.rb.velocity = bullet.transform.forward * bulletSpeed;
                    bullet.superShot = currentSocket.state == SocketStates.SuperCharged;
                    bullet.damage = baseDamage;
                    bullet.knockBack = baseKnockBack;
                
                    bullet.meshRenderer.material = 
                        currentSocket.state == SocketStates.SuperCharged ? superChargedSocket : loadedSocket;
                    bulletParticle[0].transform.LookAt(hit.point);
                
                
                    var oui = bulletParticle[^1].main;
                    var ohCestRelou = bulletParticle[0].transform.eulerAngles;
                    oui.startRotationX = ohCestRelou.x;
                    oui.startRotationY = ohCestRelou.y;
                    oui.startRotationZ = ohCestRelou.z;
                }
                else
                {
                    
                }

            }
            
            
            

        }
        else
        {
            Debug.Log("Hit some air");
        }
        bulletParticle[0].Play();



        UpdateCurrentSocket();
    }



}

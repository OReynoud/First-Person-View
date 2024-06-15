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

    public readonly int InkLevel = Shader.PropertyToID("_InkLevel");
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
    [Foldout("Shoot")] [SerializeField] private float weakSpotDamage;
    [Foldout("Shoot")] [SerializeField] private float weakSpotKnockBack;
    [Foldout("Shoot")] [SerializeField] private float bodyDamage;
    [Foldout("Shoot")] [SerializeField] private float bodyKnockBack;

    // [Foldout("Surplus")] [SerializeField] private float surplusShotDamage;
    // [Foldout("Surplus")] [SerializeField] private float surplusKnockBack;
    // [Foldout("Surplus")] [HideInInspector] public Vector3 hitConeSize;
    // [Foldout("Surplus")] [SerializeField] private float overheatTime;
    // [Foldout("Surplus")] [SerializeField][Range(1,100)] private float percentInkCost = 50;



    [Foldout("Refs")] public Material emptySocket;
    [Foldout("Refs")] public Material loadedSocket;
    [Foldout("Refs")] public Material superChargedSocket;
    [Foldout("Refs")] public GameObject overHeatFeedback;

    [Foldout("Refs")] [SerializeField] private PlayerBullet bulletPrefab;
    [Foldout("Refs")] [SerializeField] private PlayerSuperShot superShot;
    [Foldout("Refs")] [SerializeField] private LineRenderer normalTrail;
    [Foldout("Refs")] [SerializeField] private LineRenderer superTrail;
    private LineRenderer currentTrail;

    
    private PlayerController player;


    public void Awake()
    {
        player = GetComponent<PlayerController>();
        currentSocket = sockets[0];
        currentSocket.socketMesh.material.SetFloat(InkLevel, 0);
        foreach (var ammo in sockets)
        {
            //ammo.highlightMesh.enabled = false;
            
            ammo.socketMesh.material.SetFloat(InkLevel, 1);
            ammo.state = SocketStates.Loaded;
        }

        //currentSocket.highlightMesh.enabled = true;


        ParticleSystem oui = new ParticleSystem();
        var main = oui.main;
        foreach (var particle in bulletParticle)
        {
            main = particle.main;
            main.startSpeed = bulletSpeed;
            particle.Stop();
        }

    }

    [HideInInspector] public bool noBullets;

    [HideInInspector] public bool overheated;


    public void Update()
    {
        
    }

    void UpdateCurrentSocket() //Pouce, majeur, annulaire, auriculaire. Haha je suis drole
    {
        //currentSocket.highlightMesh.enabled = false;
        currentSocket.state = SocketStates.Empty;
        currentSocket.socketMesh.material.SetFloat(InkLevel, 0);
        noBullets = true;
        for (int i = 0; i < sockets.Count; i++)
        {
            if (sockets[i].state == SocketStates.Empty) continue;
            noBullets = false;
            currentSocket = sockets[i];
            break;
        }


        if (noBullets)
        {
            player.animManager.ChainShootToReload();
            
            //SON
        }

        //currentSocket.highlightMesh.enabled = true;
    }

    private float decrement = 0;

    public void ReloadSockets()
    {
        for (int i = sockets.Count - 1; i >= 0; i--)
        {
            if (reloadCostPerBullet > player.currentInk) break;

            if (sockets[i].state != SocketStates.Empty) continue;


                decrement = reloadCostPerBullet;
                sockets[i].state = SocketStates.Loaded;
            

            player.currentInk =
                GameManager.instance.UpdatePlayerStamina(player.currentInk, player.maxInk, -decrement);
        }

        noBullets = false;
        //currentSocket.highlightMesh.enabled = false;
        foreach (var ammo in sockets)
        {
            if (ammo.state == SocketStates.Empty) continue;
            currentSocket = ammo;
            //currentSocket.highlightMesh.enabled = true;
            break;
        }
    }

    private Vector3 calculatedConeDimensions;

    public void ShootWithSocket(Transform cam, Transform origin)
    {
        CameraShake.instance.ShakeOneShot(1);
        player.animManager.RightHand_Shoot();

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
                        enemy.TakeDamage(hit.collider, cam.forward, weakSpotDamage, weakSpotKnockBack);
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
                var bullet = Instantiate(bulletPrefab, origin.position - origin.right * 0.5f, cam.rotation);
                bullet.transform.LookAt(hit.point);
                
                bullet.rb.velocity = bullet.transform.forward * bulletSpeed;
                
                bullet.superShot = currentSocket.state == SocketStates.SuperCharged;
                bullet.weakSpotDamage = weakSpotDamage;
                bullet.bodyDamage = bodyDamage;
                bullet.bodyKnockBack = bodyKnockBack;
                bullet.weakSpotKnockBack = weakSpotKnockBack;

                bullet.meshRenderer.material =
                    currentSocket.state == SocketStates.SuperCharged ? superChargedSocket : loadedSocket;
                // bulletParticle[1].transform.position = bullet.transform.position + dir;
                bulletParticle[2].transform.LookAt(hit.point);
            }
        }
        else
        {
            bulletParticle[0].transform.localRotation = Quaternion.identity;
            Debug.Log("Hit some air");
        }

        foreach (var particle in bulletParticle)
        {
            particle.Play();
        }


        UpdateCurrentSocket();
    }

    // void SurplusShot()
    // {
    //
    //     player.currentInk =
    //         GameManager.instance.UpdatePlayerStamina(player.currentInk, player.maxInk,
    //             -player.maxInk * percentInkCost * 0.01f);
    //
    //
    //     superShot.damage = surplusShotDamage;
    //     superShot.knockBack = surplusKnockBack;
    //
    //     calculatedConeDimensions = hitConeSize;
    //     // calculatedConeDimensions += Vector3.one * hitConeSizeIncrement * incrementAmount;
    //     // calculatedConeDimensions +=
    //     //     new Vector3(hitConeAngleIncrement, hitConeAngleIncrement, 0) * incrementAmount;
    //
    //     superShot.scale = calculatedConeDimensions;
    //     superShot.gameObject.SetActive(true);
    //     StartCoroutine(OverheatCoroutine(overheatTime));
    //     player.inSurplus = false;
    //
    //
    //     UpdateCurrentSocket();
    //     
    //     
    //     
    //     
    //     
    //     //Debug.Log("Completed supershot with " + incrementAmount + " increments");
    // }
    //
    // IEnumerator OverheatCoroutine(float time)
    // {
    //     overheated = true;
    //     overHeatFeedback.SetActive(true);
    //     yield return new WaitForSeconds(time);
    //     overHeatFeedback.SetActive(false);
    //     overheated = false;
    // }
}
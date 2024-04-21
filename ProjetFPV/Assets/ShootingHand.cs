using System;
using System.Collections;
using System.Collections.Generic;
using Mechanics;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class ShootingHand : MonoBehaviour
{
    public AmmoSocket currentSocket;
    public AmmoSocket nextSocket;
    public enum SocketStates
    {
        Empty,
        Loaded,
        SuperCharged
    }
    public List<AmmoSocket> sockets = new List<AmmoSocket>();
    public Material emptySocket;
    public Material loadedSocket;
    public Material superChargedSocket;
    
    [Foldout("Shoot")] [Tooltip("Which layers will get hit by the hit scan")] [SerializeField]
    public LayerMask shootMask;
    
    [Foldout("Shoot")] [Tooltip("Range of the hit scan")] [SerializeField]
    public float maxRange;
    [Foldout("Shoot")] [Tooltip("Cost to convert ink into bullets")] [SerializeField]
    private float reloadCostPerBullet;
    
    [SerializeField] private GameObject inkStainDecal;
    
    [Foldout("Refs")] public LineRenderer shootTrail;
    private LineRenderer currentTrail;



    [InfoBox("Si coché, des balles super-chargées seront périodiquement et automatiquement insérés dans les cartouches vides en utilisant le surplus d'encre")]
    public bool autoLoadOnSurplus;
    
    [ShowIf("autoLoadOnSurplus")]  [SerializeField] private float surplusBulletCost;

    [ShowIf("autoLoadOnSurplus")] public float timeToAutoLoad = 0.3f;
    
    [InfoBox("Si coché, le joueur peut recharger manuellement en étant en surplus d'encre pour remplir autant de balles super-chargées que le surplus d'encre le permet")]
    public bool reloadSuperBulletsOnSurplus;
    [ShowIf("reloadSuperBulletsOnSurplus")] public float surplusReloadTime = 0.5f;
    
    [InfoBox("Si coché, le joueur peut se servir de la molette de la souris pour choisir de quelle cartouche il va tirer")]
    public bool canChooseSocketToFire;

    public void Awake()
    {
        currentSocket = sockets[0];
        nextSocket = sockets[1];
        foreach (var ammo in sockets)
        {
            ammo.highlightMesh.enabled = false;
        }
        currentSocket.highlightMesh.enabled = true;
    }

    public void UpdateCurrentSocket() //Pouce, majeur, annulaire, auriculaire. Haha je suis drole
    {
        
        
        currentSocket.highlightMesh.enabled = false;
        currentSocket.socketMesh.material = emptySocket;
        currentSocket.state = SocketStates.Empty;
        
        currentSocket = nextSocket;

        for (int i = sockets.IndexOf(currentSocket); i < sockets.Count; i++)
        {
            if (sockets[i].state == SocketStates.Empty) continue;
            nextSocket = sockets[i];

        }
        currentSocket.highlightMesh.enabled = true;
    }

    public void ReloadSockets()
    {
        var player = PlayerController.instance;
        UpdateCurrentSocket();

        for (int i = sockets.Count - 1; i >= 0; i--)
        {
            if (reloadCostPerBullet > player.currentInk) break;
            
            if (sockets[i].state != SocketStates.Empty) continue;

            if (PlayerController.instance.inSurplus)
            {
                sockets[i].socketMesh.material = loadedSocket;
                sockets[i].state = SocketStates.Loaded;
            }
            else
            {
                sockets[i].socketMesh.material = superChargedSocket;
                sockets[i].state = SocketStates.SuperCharged;
            }
            player.currentInk = 
                GameManager.instance.UpdatePlayerStamina(player.currentInk, player.maxInk, -reloadCostPerBullet);
        }

    }

    [SerializeField] private float trailTime;
    public void ShootWithSocket(Transform cam, Transform origin)
    {
        
        CameraShake.instance.ShakeOneShot(1);
        
        currentTrail = Instantiate(shootTrail);
        Destroy(currentTrail.gameObject, trailTime);
        currentTrail.SetPosition(0, origin.position + origin.up * 0.5f);

        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, maxRange, shootMask))
        {
            Debug.Log("Hit something");
            if (hit.collider.CompareTag("Head"))
            {
                if (hit.collider.transform.parent.TryGetComponent(out Enemy enemy))
                {
                    if (currentSocket.state == SocketStates.Loaded)
                    {
                        enemy.TakeDamage(hit.collider, false);
                    }
                    else
                    {
                        enemy.TakeDamage(hit.collider, true);
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
            var decal = Instantiate(inkStainDecal, hit.point + hit.normal * 0.02f, Quaternion.identity, hit.transform);
            decal.transform.forward = -hit.normal;
            decal.transform.RotateAround(decal.transform.position, decal.transform.forward, Random.Range(-180f, 180f));
            //Je m'en vais !
        }
        else
        {
            Debug.Log("Hit some air");
            currentTrail.SetPosition(1, cam.forward * maxRange + cam.position);
        }
        
        UpdateCurrentSocket();
    }

}

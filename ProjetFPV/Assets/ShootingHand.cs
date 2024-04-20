using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class ShootingHand : MonoBehaviour
{
    public AmmoSocket currentSocket;
    public enum SocketStates
    {
        Empty,
        Loaded,
        SuperCharged
    }
    public AmmoSocket[] sockets;
    public Material emptySocket;
    public Material loadedSocket;
    public Material superChargedSocket;



    [InfoBox("Si coché, des balles super-chargées seront périodiquement et automatiquement insérés dans les cartouches vides en utilisant le surplus d'encre")]
    public bool autoLoadOnSurplus;

    [ShowIf("autoLoadOnSurplus")] public float timeToAutoLoad = 0.3f;
    
    [InfoBox("Si coché, le joueur peut recharger manuellement en étant en surplus d'encre pour remplir autant de balles super-chargées que le surplus d'encre le permet")]
    public bool reloadSuperBulletsOnSurplus;
    [ShowIf("reloadSuperBulletsOnSurplus")] public float surplusReloadTime = 0.5f;
    
    [InfoBox("Si coché, le joueur peut se servir de la molette de la souris pour choisir de quelle cartouche il va tirer")]
    public bool canChooseSocketToFire;

    public void Awake()
    {
        currentSocket = sockets[0];
        foreach (var ammo in sockets)
        {
            ammo.highlightMesh.enabled = false;
        }
        currentSocket.highlightMesh.enabled = true;
    }

    public void UpdateCurrentSocket(int index) //Pouce, majeur, annulaire, auriculaire. Haha je suis drole
    {
        
        currentSocket.highlightMesh.enabled = false;
        currentSocket.socketMesh.material = emptySocket;
        currentSocket.state = SocketStates.Empty;
        if (index == sockets.Length) return;
        
        currentSocket = sockets[index];
        
        currentSocket.highlightMesh.enabled = true;
    }

    public void ReloadSockets(int index)
    {
        UpdateCurrentSocket(index);
        for (int i = index; i < sockets.Length; i++)
        {
            sockets[i].socketMesh.material = loadedSocket;
            sockets[i].state = SocketStates.Loaded;
        }

    }

}

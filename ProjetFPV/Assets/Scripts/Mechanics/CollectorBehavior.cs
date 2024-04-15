using System;
using System.Collections;
using System.Collections.Generic;
using Mechanics;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CollectorBehavior : Enemy
{        
    [InfoBox("Collector Behavior")]
    public NavMeshAgent agent;
    public CollectorBullet bulletPrefab;
    [InfoBox("Cercle vert = Zone de patrouille;  Cercle rouge = Zone d'aggro", EInfoBoxType.Warning) ]
    [SerializeField]
    private float pathUpdateFrequency;
    public enum States
    {
        Roam,
        Repositioning,
        Shoot,
        Stunned,
        Paralysed
    }

    [Foldout("Shoot State")] [SerializeField]
    private int numberOfBullets;

    [Foldout("Shoot State")] [SerializeField]
    private float bulletSpeed;
    [Foldout("Shoot State")] [SerializeField]
    private float timeBetweenBullets;

    [Foldout("Shoot State")] [SerializeField]
    private Transform[] bulletSpawnPos;

    public States currentState;
    public Animation transitionState;
    private bool repositioning;
    [BoxGroup("Debug")] [SerializeField] private float bulletTimer;
    [BoxGroup("Debug")] [SerializeField] private float bulletsLeft;
    [BoxGroup("Debug")] [SerializeField] private bool spawning;
    
    

    public Transform test;
    // Update is called once per frame
    public override void Start()
    {
        bulletsLeft = numberOfBullets;
        
    }
    void Update()
    {
        agent.SetDestination(test.position);
        switch (currentState)
        {
            case States.Roam:
                break;
            case States.Repositioning:
                break;
            case States.Shoot:
                ShootMethod();
                break;
            case States.Stunned:
                break;
            case States.Paralysed:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ShootMethod()
    {
        if (bulletsLeft <= 0)
        {
            currentState = States.Repositioning;
            bulletsLeft = numberOfBullets;
        }
        if (bulletTimer < timeBetweenBullets)
        {
            bulletTimer += Time.deltaTime;
            return;
        }

        bulletsLeft--;
        bulletTimer = 0;
        var rand = Random.Range(0, bulletSpawnPos.Length);
        var dir = PlayerController.instance.transform.position - bulletSpawnPos[rand].position;
        var bullet = Instantiate(bulletPrefab, bulletSpawnPos[rand].position, Quaternion.LookRotation(dir.normalized));
        bullet.rb.velocity = dir * bulletSpeed;
    }
}

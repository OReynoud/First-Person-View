using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mechanics;
using NaughtyAttributes;
using UnityEngine;
[RequireComponent(typeof(TelekinesisObject))]
public class TNT : MonoBehaviour, IDestructible
{
    [SerializeField] private float health = 1;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionForce = 3;
    [SerializeField] private float damageToPlayer = 3;
    [SerializeField] private float damageToEnemy = 5;
    [Range(0,1)][SerializeField] private float damageFallOff = 0.2f;
    [SerializeField] private LayerMask mask;
    
    [SerializeField] private GameObject barrelMesh;
    [SerializeField] private ParticleSystem[] VFX_Explosion;
    private TelekinesisObject tk;
    private bool soundPlayed;

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public void TakeDamage()
    {
        health--;
        if (health <= 0)
        {
            OnDestroyEvent();
        }
    }

    private void Awake()
    {
        tk = GetComponent<TelekinesisObject>();

        var main = VFX_Explosion[1].main;
        var baseSize = main.startSize.constant;
        main = VFX_Explosion[1].main;
        main.startSize = explosionRadius * 2;

        float sizeIncreaseFactor = explosionRadius * 2 / baseSize;
        for (int i = 2; i < VFX_Explosion.Length; i++)
        {
            main = VFX_Explosion[i].main;
            baseSize = main.startSize.constant;
            main.startSize =  baseSize * sizeIncreaseFactor;
        }
    }

    public void OnDestroyEvent()
    {
        if (health <= -1)return;
        health--;
        GetComponent<Collider>().enabled = false;
        
        if (TryGetComponent(out TelekinesisObject tk))
        {
            tk.body.isKinematic = true;
            tk.body.useGravity = false;
        }
        var colliders = Physics.OverlapSphere(transform.position, explosionRadius, mask, QueryTriggerInteraction.Ignore);

        foreach (var col in colliders)
        {
            var damageModifier = Mathf.Lerp(1, damageFallOff,
                Vector3.Distance(col.transform.position, transform.position) / explosionRadius);
            Vector3 dir = col.transform.position - transform.position;
            
            if (col.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                var enemy = col.GetComponentInParent<Enemy>();
                enemy.TakeDamage(explosionForce,dir.normalized, col.ClosestPointOnBounds(transform.position),damageToEnemy * damageFallOff, col);
            }

            if (col.transform.TryGetComponent(out TNT tnt))
            {
                if (tnt != this)
                {
                    transform.DOMove(transform.position, 0.08f).OnComplete((() =>
                    {
                        tnt.OnDestroyEvent();
                    }));
                }
            }

            if (col.gameObject.CompareTag(Ex.Tag_Player))
            {
                if (col.enabled)
                {
                    
                    Debug.Log("Taking damage");
                    PlayerController.instance.TakeDamage(damageModifier * damageToPlayer);
                }
            }
        }

        barrelMesh.SetActive(false);
        VFX_Explosion[0].Play();
        //SON

        if (!soundPlayed)
        {
            AudioManager.instance.PlaySound(1, 0, gameObject, 0.05f, false);
            soundPlayed = true;
        }
        
        
        Destroy(gameObject,VFX_Explosion[0].main.duration + 1f);
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (!tk.thrown)return;
        Debug.Log("COLLIDE");
        
        health = 0;
        OnDestroyEvent();
        
    }
}


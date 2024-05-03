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
    [SerializeField] private GameObject explosionMesh;
    private TelekinesisObject tk;

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
            OnDestroy();
        }
    }

    private void Awake()
    {
        tk = GetComponent<TelekinesisObject>();
    }

    public void OnDestroy()
    {
        if (health <= -1)return;
        health--;
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
            
            if (col.transform.TryGetComponent(out Enemy enemy))
            {
                enemy.TakeDamage(explosionForce,dir.normalized, col.ClosestPointOnBounds(transform.position),damageToEnemy * damageFallOff, col);
            }

            if (col.transform.TryGetComponent(out TNT tnt))
            {
                if (tnt != this)
                {
                    transform.DOMove(transform.position, 0.08f).OnComplete((() =>
                    {
                        tnt.OnDestroy();
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

        //Moche en attendant le bon VFX
        GetComponent<MeshRenderer>().enabled = false;
        transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        //
        
        explosionMesh.SetActive(true);
        explosionMesh.transform.localScale = new Vector3(1/transform.localScale.x ,1/transform.localScale.y,1/transform.localScale.z) * explosionForce;
        Destroy(gameObject,1f);
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (!tk.thrown)return;
        Debug.Log("COLLIDE");
        
        health = 0;
        OnDestroy();
        
    }
}


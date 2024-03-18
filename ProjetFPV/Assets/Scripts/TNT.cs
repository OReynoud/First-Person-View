using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT : MonoBehaviour, IDestructible
{
    [SerializeField] private float health = 1;
    [SerializeField] private float explosionRadius;
    [SerializeField] private float explosionForce = 3;
    [SerializeField] private float damageToEnemy = 5;
    [SerializeField] private float damageToPlayer = 3;
    [Range(0,1)][SerializeField] private float damageFallOff = 0.2f;
    [SerializeField] private LayerMask mask;
    [SerializeField] private GameObject explosionMesh;

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

    public void OnDestroy()
    {
        
        var colliders = Physics.OverlapSphere(transform.position, explosionRadius, mask, QueryTriggerInteraction.Ignore);

        foreach (var col in colliders)
        {
            var damageModifier = Mathf.Lerp(1, damageFallOff,
                Vector3.Distance(col.transform.position, transform.position) / explosionRadius);
            Vector3 dir = col.transform.position - transform.position;
            
            if (col.transform.parent.TryGetComponent(out Enemy enemy))
            {
                enemy.TakeDamage(Mathf.RoundToInt(damageToEnemy * damageModifier), explosionForce,dir.normalized, col.ClosestPointOnBounds(transform.position));
            }

            if (col.gameObject.CompareTag(Ex.Tag_Player))
            {
                if (!col.enabled)return;
                Debug.Log("Taking damage");
                PlayerController.instance.TakeDamage(damageModifier * damageToPlayer);
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
}


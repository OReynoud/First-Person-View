using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT : MonoBehaviour, IDestructible
{
    [SerializeField] private float explosionRadius;
    [SerializeField] private float damageToEnemy = 5;
    [SerializeField] private float damageToPlayer = 3;
    [SerializeField] private float damageFallOff = 0.2f;
    [SerializeField] private LayerMask mask;

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public void OnDestroy()
    {
        
        var colliders = Physics.OverlapSphere(transform.position, explosionRadius, mask, QueryTriggerInteraction.UseGlobal);

        foreach (var col in colliders)
        {
            var damageModifier = Mathf.Lerp(1, damageFallOff,
                Vector3.Distance(col.transform.position, transform.position) / explosionRadius);
            Vector3 dir = col.transform.position - transform.position;
            
            if (col.transform.parent.TryGetComponent(out Enemy enemy))
            {
                enemy.TakeDamage(Mathf.RoundToInt(damageToEnemy * damageModifier), 1,dir.normalized, col.ClosestPointOnBounds(transform.position));
            }

            if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Debug.Log("Taking damage");
            }
        }
        
        Destroy(gameObject);
    }
}

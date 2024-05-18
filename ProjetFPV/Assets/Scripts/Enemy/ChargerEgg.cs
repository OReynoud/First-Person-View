using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mechanics;
using UnityEngine;

public class ChargerEgg : MonoBehaviour
{
    public ChargerBehavior toSpawn;
    public CollectorBehavior parent;
    public float speed;
    public Transform destination;
    
    
    
    private ChargerBehavior spawnedEnemy;
    
    // Start is called before the first frame update

    public void LayEgg()
    {
        transform.DOMove(destination.position, Vector3.Distance(transform.position,destination.position)/speed).SetEase(Ease.Linear).OnComplete((
            () =>
            {
                spawnedEnemy = Instantiate(toSpawn,destination.position,Quaternion.identity);
                spawnedEnemy.collectorSpawn = true;
                spawnedEnemy.spawnPositions = parent.spawnEnemyPos;
                parent.children.Add(spawnedEnemy);
                Destroy(gameObject);
            }));
    }
}
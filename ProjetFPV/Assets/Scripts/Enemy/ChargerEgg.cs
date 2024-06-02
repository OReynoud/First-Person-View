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
        var eggSound = AudioManager.instance.PlaySound(7, 6, gameObject, 0f, true);
        
        transform.DOMove(destination.position, Vector3.Distance(transform.position,destination.position)/speed).SetEase(Ease.Linear).OnComplete((
            () =>
            {
                spawnedEnemy = Instantiate(toSpawn,destination.position,Quaternion.identity);
                spawnedEnemy.collectorSpawn = true;
                spawnedEnemy.parentEnemy = parent;
                spawnedEnemy.spawnPositions = parent.spawnEnemyPos;
                parent.children.Add(spawnedEnemy);
                Destroy(eggSound);
                Destroy(gameObject);
            }));
    }
}

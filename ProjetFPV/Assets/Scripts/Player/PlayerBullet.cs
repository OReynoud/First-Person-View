using System;
using System.Collections;
using System.Collections.Generic;
using Mechanics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerBullet : MonoBehaviour
{
    public bool superShot;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Hit something");
        if (other.collider.CompareTag("Head"))
        {
            if (other.transform.parent.TryGetComponent(out Enemy enemy))
            {
                
                enemy.TakeDamage(other.collider, superShot);
                

                GameManager.instance.HitMark(true);
            }
        }

        if (other.gameObject.TryGetComponent(out IDestructible target))
        {
            target.TakeDamage();
        }


        //Coucou, Thomas est passé par là (jusqu'au prochain commentaire)
        var decal = Instantiate(GameManager.instance.inkStainDecal, other.GetContact(0).point + other.GetContact(0).normal * 0.02f, Quaternion.identity, other.transform);
        decal.transform.forward = -other.GetContact(0).normal;
        decal.transform.RotateAround(decal.transform.position, decal.transform.forward, Random.Range(-180f, 180f));
        //Je m'en vais !
    }
}

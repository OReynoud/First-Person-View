using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : ControllableProp
{
    public float maxHealth;
    public float currentHealth;
    public float headShotMultiplier;
    public float bodyShotMultiplier;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, bool headHit, float knockBackValue, Vector3 knockBackDir)
    {
        var totalDmg = 0f;
        if (headHit)
        {
            totalDmg = damage * headShotMultiplier;
            currentHealth -= totalDmg;
        }
        else
        {
            totalDmg = damage * bodyShotMultiplier;
            currentHealth -= totalDmg;
        }

        if (currentHealth < 0)Die();
        if (isGrabbed)return;
        
        body.AddForce(knockBackDir * knockBackValue * totalDmg);
    }

    private void Die()
    {
        GameManager.instance.HitMark(true);
        Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

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

    public void TakeDamage(int damage, bool headHit)
    {
        if (headHit)
        {
            currentHealth -= damage * headShotMultiplier;
        }
        else
        {
            currentHealth -= damage * bodyShotMultiplier;
        }

        if (currentHealth < 0)Die();
    }

    private void Die()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

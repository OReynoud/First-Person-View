using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class Enemy : ControllableProp
{
    public float maxHealth;
    public float currentHealth;
    public float headShotMultiplier;
    public float bodyShotMultiplier;
    public bool isImmobile = true;

    [HideIf("isImmobile")] public List<Transform> waypoints;
    [HideIf("isImmobile")] public float translationSpeed = 0.1f;

    private int currentIndex = 1;

    private int previousIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        currentIndex = 1;
        previousIndex = 0;
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

        if (currentHealth < 0) Die();
        if (isGrabbed) return;

        body.AddForce(knockBackDir * knockBackValue * totalDmg);
    }

    public void TakeDamage(int damage, float knockBackValue, Vector3 knockBackDir, Vector3 pointOfForce)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            Die();
        }


        if (isGrabbed) return;

        body.constraints = RigidbodyConstraints.None;
        body.useGravity = true;
        body.AddForceAtPosition(knockBackDir * knockBackValue * damage,pointOfForce, ForceMode.Impulse);
        body.AddForce(knockBackDir * knockBackValue * damage, ForceMode.Impulse);
    }

    private void Die()
    {
        GameManager.instance.HitMark(true);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (isImmobile) return;
        MoveBetweenWaypoints();
    }

    private float lerp;

    void MoveBetweenWaypoints()
    {
        lerp += Time.deltaTime * translationSpeed;
        transform.position = Vector3.Lerp(waypoints[previousIndex].position, waypoints[currentIndex].position, lerp);
        if (lerp >= 1)
        {
            lerp = 0;
            previousIndex = currentIndex;
            currentIndex++;
            if (currentIndex >= waypoints.Count)
            {
                currentIndex = 0;
            }
        }
    }
}
using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerHealth : MonoBehaviour
{
    [Tooltip("Cocher si le perso doit commencer avec toute sa vie")] [SerializeField] private bool fullHealthOnStart;
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;
    
    [Tooltip("Temps avant de régen après avoir subi des dégâts")] 
    [SerializeField] private float timeBeforeRegen; 
    
    [Tooltip("PV régénérés chaque seconde")] 
    [SerializeField] private float regenPerSecond; //Nombre de PV soignés chaque seconde
    
    [Tooltip("Intensité de la vignette selon les PV perdus")] 
    [SerializeField] private AnimationCurve vignetteIntensity; //Intensité de la vignette

    [SerializeField] private Volume volume;
    
    private float t;

    private void OnValidate() //Eviter que currentHealth ne dépasse maxHealth
    {
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    void Start()
    {
        if (fullHealthOnStart)
        {
            currentHealth = maxHealth;
        }
    }

    void Update()
    {
        t -= Time.deltaTime;

        var lostHealth = (maxHealth - currentHealth) / maxHealth;

        //volume.weight = Mathf.Lerp(volume.weight, vignetteIntensity.Evaluate(lostHealth), .01f);
        
        if (t <= 0 && currentHealth < maxHealth) //La régén ne s'applique qu'après que timeBeforeRegen soit passé
        {
            currentHealth += regenPerSecond * Time.deltaTime;

            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
        }
    }
    
    public void TakeDamage(int damages)
    {
        CameraShake.instance.ShakeOneShot(2);
        currentHealth -= damages;
        t = timeBeforeRegen;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Joueur mort");
    }
}

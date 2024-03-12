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
        
        //volume.weight = Math.Max((maxHealth - currentHealth) / maxHealth, 1f); //Set l'intensité du global volume selon les dégâts subis (max 1f)
        
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

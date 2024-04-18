using UnityEngine;

namespace Mechanics
{
    public class HealPack : MonoBehaviour, ICanInteract
    {
        public void Interact(Vector3 dir)
        {
            if (PlayerController.instance.currentHealPackAmount >= PlayerController.instance.healPackCapacity)return;
            
            PlayerController.instance.currentHealPackAmount++;
            GameManager.instance.UpdateHealPackUI();
            Destroy(gameObject);
        }

        public void ShowContext()
        {
            GameManager.instance.interactText.text = "[E] Prendre le Pack de soins";
        }
    }
}
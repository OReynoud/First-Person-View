using UnityEngine;

namespace Mechanics
{
    public class HealPack : MonoBehaviour, ICanInteract
    {
        public void Interact(Vector3 dir)
        {
            if (PlayerController.instance.currentHealPackAmount >= PlayerController.instance.healPackCapacity)return;
            PlayerController.instance.animManager.RightHand_PickUp();
            PlayerController.instance.currentHealPackAmount++;
            PlayerController.instance.tkManager.UpdateHealPackVisual();
            //SON
            Destroy(gameObject);
        }

        public void ShowContext()
        {
            GameManager.instance.interactText.text = "[E] Pick up";
        }
    }
}

using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Mechanics
{
    public class HealPack : MonoBehaviour, ICanInteract
    {
        private Animator anim;
        public GameObject healingCapsule;

        public float activationDistance = 3f;
        private bool used;

        private void Start()
        {
            anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!used && Vector3.Distance(PlayerController.instance.transform.position, transform.position) < activationDistance)
            {
                anim.SetBool("Open",true);
            }
            else
            {
                anim.SetBool("Open",false);
            }
        }

        public void Interact(Vector3 dir)
        {
            if (PlayerController.instance.currentHealPackAmount >= PlayerController.instance.healPackCapacity)return;
            PlayerController.instance.animManager.RightHand_PickUp();
            PlayerController.instance.currentHealPackAmount++;
            PlayerController.instance.tkManager.UpdateHealPackVisual();
            //SON
            used = true;
            healingCapsule.SetActive(false);
        }

        public void ShowContext()
        {
            GameManager.instance.interactText.text = used ? "" : "[E] Pick up";
        }
    }
}

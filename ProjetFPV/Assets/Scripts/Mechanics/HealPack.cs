using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Mechanics
{
    public class HealPack : MonoBehaviour, ICanInteract
    {
        private Animator anim;
        public GameObject healingCapsule;
        public GameObject light;

        public float activationDistance = 3f;
        private bool used;

        private bool previousState;
        private bool currentState;

        private void Start()
        {
            anim = GetComponent<Animator>();
        }

        private void Update()
        {
            previousState = currentState;
            
            if (!used && Vector3.Distance(PlayerController.instance.transform.position, transform.position) < activationDistance)
            {
                currentState = true;
                anim.SetBool("Open",true);
            }
            else
            {
                currentState = false;
                anim.SetBool("Open",false);
            }

            if (previousState != currentState)
            {
                AudioManager.instance.PlaySound(3, currentState ? 21 : 22, gameObject, 0.1f, false);
            }
        }

        public void Interact(Vector3 dir)
        {
            if (PlayerController.instance.currentHealPackAmount >= PlayerController.instance.healPackCapacity)return;
            PlayerController.instance.animManager.RightHand_PickUp();
            PlayerController.instance.currentHealPackAmount++;
            PlayerController.instance.tkManager.UpdateHealPackVisual();
            AudioManager.instance.PlaySound(3, 8, gameObject, 0.1f, false);
            used = true;
            healingCapsule.SetActive(false);
            light.SetActive(false);
        }

        public void ShowContext()
        {
            GameManager.instance.interactText.text = used ? "" : "[E] Take medicine";
        }
    }
}

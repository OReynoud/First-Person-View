using DG.Tweening;
using Mechanics;
using UnityEngine;

public class Door : MonoBehaviour, ICanInteract
{
    [SerializeField] private bool _isLocked;

    [HideInInspector] public bool lockBroken;
    
    public BoxCollider col;
    [SerializeField] private GameObject leftDoor;
    [SerializeField] private GameObject rightDoor;
    [SerializeField] private float duration;

    void Start()
    {
        lockBroken = !_isLocked;
        col = GetComponent<BoxCollider>();
    }

    public void Interact(Vector3 dir)
    {
        if (!lockBroken) return;

        PlayerController.instance.animManager.RightHand_PickUp();
        AudioManager.instance.PlaySound(1, 3, gameObject, 0.1f, false);

        Debug.Log(transform.rotation.y + "|" + transform.localRotation.y);
        
        leftDoor.transform.DORotate(new Vector3(0, transform.eulerAngles.y - 85f, 0), duration);
        rightDoor.transform.DORotate(new Vector3(0,  + transform.eulerAngles.y + 85f, 0), duration);
        col.enabled = false;
    }

    public void ShowContext()
    {
        GameManager.instance.interactText.text = lockBroken ? "[E] Ouvrir" : "";
    }
    
    private void OnValidate()
    {
        transform.GetChild(0).gameObject.SetActive(_isLocked);
    }
}

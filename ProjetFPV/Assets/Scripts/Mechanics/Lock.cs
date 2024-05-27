using UnityEngine;

public class Lock : MonoBehaviour, IDestructible
{
    private Door door;
    
    void Start()
    {
        door = transform.parent.parent.GetComponent<Door>();
    }
    
    public void TakeDamage()
    {
       OnDestroyEvent();
    }

    public void OnDestroyEvent()
    {
        door.lockBroken = true;
        
        AudioManager.instance.PlaySound(1, 5, gameObject.transform.parent.transform.parent.gameObject, 0.1f, false);
        
        Destroy(transform.parent.gameObject);
    }
    


}

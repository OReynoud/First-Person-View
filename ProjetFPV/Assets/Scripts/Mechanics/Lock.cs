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
       OnDestroy();
    }

    public void OnDestroy()
    {
        door.lockBroken = true;
        
        Destroy(transform.parent.gameObject);
    }
    


}

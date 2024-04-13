using UnityEngine;

public class GymBullet : MonoBehaviour
{
    public int bulletDamage;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().TakeDamage(bulletDamage);
            Destroy(gameObject);
        }
    }
}

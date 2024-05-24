using UnityEngine;

public class GymEnemyShoot : MonoBehaviour
{
    [Tooltip("Angle du tir")] [SerializeField] [Range(0f, 360f)] private float angle;
    [Tooltip("Cooldown entre deux tirs")] [SerializeField] private float timeBetweenShot;
    [Tooltip("Puissance de la balle (commencer à partir de 1000)")] [SerializeField] private float bulletForce;
    [Tooltip("Prefab de la balle")] [SerializeField] private GameObject bullet;
    
    private float t;
    private Vector3 direction;
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        direction = transform.TransformDirection(Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward) * 20;
        Gizmos.DrawRay(transform.position, direction);
    }

    void Start()
    {
        direction = transform.TransformDirection(Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward) * 20;
    }
    void Update()
    {
        t -= Time.deltaTime;

        if (t <= 0)
        {
            Shoot();
            
            t = timeBetweenShot;
        }
    }

    void Shoot()
    {
        GameObject spawnedBullet = Instantiate(bullet, transform.position + direction/10, Quaternion.identity);
        spawnedBullet.GetComponent<Rigidbody>().AddForce(direction/20 * bulletForce);
        Destroy(spawnedBullet, 2f);
    }
}

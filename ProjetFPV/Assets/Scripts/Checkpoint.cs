using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var savePosition = other.transform.position;
            var inkLevel = PlayerController.instance.GetInk();
            var healKits = PlayerController.instance.GetHealKits();

            PlayerPrefs.SetFloat("SavePosX", savePosition.x);
            PlayerPrefs.SetFloat("SavePosY", savePosition.y);
            PlayerPrefs.SetFloat("SavePosZ", savePosition.z);
            PlayerPrefs.SetFloat("SaveInkLevel", inkLevel);
            PlayerPrefs.SetInt("SaveHealKits", healKits);
            
            Destroy(gameObject);
        }
    }
}

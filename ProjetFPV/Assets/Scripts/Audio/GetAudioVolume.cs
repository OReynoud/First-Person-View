using UnityEngine;

public class GetAudioVolume : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private int category;
    [SerializeField] private int sound;
    [Tooltip("Permet de réduire le volume de cet objet par rapport au son de base. 0 = pas de son ; 1 = son de base")] [Range(0f,1f)] [SerializeField] private float attenuation;

    void Start()
    {
        audioSource.volume = attenuation * AudioManager.instance.GetVolume(category, sound);
    }
}

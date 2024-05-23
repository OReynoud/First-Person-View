using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public List<SoundsSO> categories;

    [SerializeField] AudioSource music1;
    [SerializeField] AudioSource music2;
    private int currentMusicSource;

    [SerializeField] private GameObject prefabAudioSource;
    
    private float GetVolume(int category, int sound) // Appeler cette fonction avec l'index affiché dans le tableau de sound design
    {
        return categories[category].sounds[sound].volume;
    }

    /// <param name="category">La catégorie du son, référencée dans le tableur (de 0 à 8).</param>
    /// <param name="sound">Le son joué dans la catégorie, référencé dans le tableur.</param>
    /// <param name="position">La position de l'objet qui émet le son.</param>
    /// <param name="randomPitchIntensity">L'intensité du pitch randomisé. 0 = pitch du son originel.</param>
    public void PlaySound(int category, int sound, Vector3 position, float randomPitchIntensity)
    {
        var newAudioSourceObject = Instantiate(prefabAudioSource, position, Quaternion.identity);
        var newAudioSource = newAudioSourceObject.GetComponent<AudioSource>();
        newAudioSource.volume = GetVolume(category, sound);
        newAudioSource.pitch = Random.Range(-randomPitchIntensity, randomPitchIntensity);
        newAudioSource.PlayOneShot(categories[category].sounds[sound].sounds[Random.Range(0, categories[category].sounds[sound].sounds.Count)]);
    }
}


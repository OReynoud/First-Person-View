using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    public List<SoundsSO> categories;

    [SerializeField] AudioSource music1;
    [SerializeField] AudioSource music2;
    private int currentMusicSource;
    private Coroutine currentCoroutine;

    [SerializeField] private GameObject prefabAudioSource;

    [SerializeField] private AudioMixerSnapshot defaultSnapshot;
    [SerializeField] private AudioMixerSnapshot muffleSnapshot;
    [SerializeField] [Range(0f, 1f)] private float snapshotTransitionTime;
    
    private float GetVolume(int category, int sound) // Appeler cette fonction avec l'index affiché dans le tableau de sound design
    {
        return categories[category].sounds[sound].volume;
    }

    /// <param name="category">La catégorie du son, référencée dans le tableur (de 0 à 8).</param>
    /// <param name="sound">Le son joué dans la catégorie, référencé dans le tableur.</param>
    /// <param name="position">La position de l'objet qui émet le son.</param>
    /// <param name="randomPitchIntensity">L'intensité du pitch randomisé. 0 = pitch du son originel.</param>
    /// <param name="loop">Détermine si le son doit loop. Dans ce cas, le gameobject doit être détruit manuellement.</param>
    public GameObject PlaySound(int category, int sound, GameObject go, float randomPitchIntensity, bool loop)
    {
        var newAudioSourceObject = Instantiate(prefabAudioSource, go.transform.position, Quaternion.identity, go.transform);
        var newAudioSource = newAudioSourceObject.GetComponent<AudioSource>();
        newAudioSource.clip = categories[category].sounds[sound].sounds[Random.Range(0, categories[category].sounds[sound].sounds.Count)];
        newAudioSource.volume = GetVolume(category, sound);
        newAudioSource.pitch = 1 - Random.Range(-randomPitchIntensity, randomPitchIntensity);
        newAudioSourceObject.SetActive(true);
        if (loop)
        {
            newAudioSource.loop = true;
        }
        else
        {
            Destroy(newAudioSourceObject, categories[category].sounds[sound].sounds[Random.Range(0, categories[category].sounds[sound].sounds.Count)].length + 0.3f);
        }
        
        return newAudioSourceObject;
    }

    /// <param name="category">La catégorie de la musique, référencée dans le tableur (de 0 à 8).</param>
    /// <param name="sound">La musique jouée dans la catégorie, référencée dans le tableur.</param>
    /// <param name="transitionDuration">Le temps de transition (en secondes) entre les deux musiques.</param>
    public void SwapMusics(int category, int sound, float transitionDuration)
    {
        if (currentCoroutine is not null)
        {
            StopCoroutine(currentCoroutine);
        }
        
        switch (currentMusicSource)
        {
            case 0:
                music2.clip = categories[category].sounds[sound].sounds[Random.Range(0, categories[category].sounds[sound].sounds.Count)];
                currentCoroutine = StartCoroutine(SwapMusicsCoroutine(music1, music2, transitionDuration, GetVolume(category, sound)));
                currentMusicSource = 1;
                break;
            case 1:
                music1.clip = categories[category].sounds[sound].sounds[Random.Range(0, categories[category].sounds[sound].sounds.Count)];
                currentCoroutine = StartCoroutine(SwapMusicsCoroutine(music2, music1, transitionDuration, GetVolume(category, sound)));
                currentMusicSource = 0;
                break;
        }
    }

    private IEnumerator SwapMusicsCoroutine(AudioSource from, AudioSource to, float duration, float volume)
    {
        to.gameObject.SetActive(true);
        var currentVolume = from.volume;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            to.volume = (t / duration) * volume;
            from.volume = (1 - (t / duration)) * currentVolume;

            yield return null;
        }

        from.gameObject.SetActive(false);
    }

    public void MuffleSound()
    {
        muffleSnapshot.TransitionTo(snapshotTransitionTime);
    }

    public void UnMuffleSound()
    {
        defaultSnapshot.TransitionTo(snapshotTransitionTime);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            PlaySound(1, 2, transform.gameObject, 0.1f, false);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            SwapMusics(2, 1, 4f);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            SwapMusics(2, 0, 4f);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            MuffleSound();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            UnMuffleSound();
        }
    }
}


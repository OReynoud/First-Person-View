using UnityEngine;

public class AudioAdvertising : MonoBehaviour
{
    private float t;
    private int previousAd;

    [SerializeField] private float timeBetweenAds;
    [SerializeField] private AudioSubtitles subtitles;

    void Update()
    {
        t -= Time.deltaTime;

        if (t < 0)
        {
            PlayAd();
        }
    }

    void PlayAd()
    {
        var r = 0;
        
        while (r == previousAd)
        {
            r = Random.Range(0, 10);
        }

        previousAd = r;
        var audioObject = AudioManager.instance.PlaySound(9, r, gameObject, 0f, false);
        t = timeBetweenAds + audioObject.GetComponent<AudioSource>().clip.length;
        subtitles.StartTimer(r);
    }
}

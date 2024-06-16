using UnityEngine;

public class AudioAdvertising : MonoBehaviour
{
    private float t;
    private int previousAd;

    [SerializeField] private float timeBetweenAds;
    [SerializeField] private AudioSubtitles subtitles;
    private int r = 0;

    [SerializeField] private int minAd;
    [SerializeField] private int maxAd;

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
        while (r == previousAd)
        {
            r = Random.Range(minAd, maxAd);
        }

        previousAd = r;
        var audioObject = AudioManager.instance.PlaySound(9, r, gameObject, 0f, false);
        t = timeBetweenAds + audioObject.GetComponent<AudioSource>().clip.length;
        subtitles.StartTimer(r - minAd);
    }
}

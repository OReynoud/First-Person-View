using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public CanvasGroup optionsCanva;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider subtitlesSlider;
    [SerializeField] private TextMeshProUGUI subtitles;
    private Coroutine coroutine;

    void Start()
    {
        if (!PlayerPrefs.HasKey("Sensitivity"))
        {
            PlayerPrefs.SetInt("Sensitivity", 50);
        }
        if (!PlayerPrefs.HasKey("MasterVolume"))
        {
            PlayerPrefs.SetInt("MasterVolume", 0);
        }
        if (!PlayerPrefs.HasKey("MusicVolume"))
        {
            PlayerPrefs.SetInt("MusicVolume", 0);
        }
        if (!PlayerPrefs.HasKey("SFXVolume"))
        {
            PlayerPrefs.SetInt("SFXVolume", 0);
        }
        if (!PlayerPrefs.HasKey("SubtitlesSize"))
        {
            PlayerPrefs.SetInt("SubtitlesSize", 50);
        }

        sensitivitySlider.value = PlayerPrefs.GetInt("Sensitivity");
        masterSlider.value = PlayerPrefs.GetInt("MasterVolume");
        musicSlider.value = PlayerPrefs.GetInt("MusicVolume");
        sfxSlider.value = PlayerPrefs.GetInt("SFXVolume");
        subtitlesSlider.value = PlayerPrefs.GetInt("SubtitlesSize");

        if (subtitles != null)
        {
            subtitles.fontSize = PlayerPrefs.GetInt("SubtitlesSize");
        }
    }
    
    public void OpenOptions()
    {
        AudioManager.instance.PlayUISound(0, 0, 0f);
        optionsCanva.gameObject.SetActive(true);
        
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(Fade(false, 0.5f, optionsCanva));
    }

    public void CloseOptions()
    {
        AudioManager.instance.PlayUISound(0, 1, 0f);
        PlayerPrefs.SetInt("Sensitivity", (int)sensitivitySlider.value);
        PlayerPrefs.SetInt("MasterVolume", (int)masterSlider.value);
        PlayerPrefs.SetInt("MusicVolume", (int)musicSlider.value);
        PlayerPrefs.SetInt("SFXVolume", (int)sfxSlider.value);
        PlayerPrefs.SetInt("SubtitlesSize", (int)subtitlesSlider.value);
        
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        coroutine = StartCoroutine(Fade(true, 0.5f, optionsCanva));
    }
    
    public IEnumerator Fade(bool fadeOut, float timer, CanvasGroup target)
    {
        var time = 0f;
        while (time < timer)
        {
            time += Time.unscaledDeltaTime;
            if (fadeOut)
            {
                target.alpha = Mathf.Lerp(1, 0, time / timer);
            }
            else
            {
                target.alpha = Mathf.Lerp(0, 1, time  / timer);
            }
            yield return null;
        }

        target.gameObject.SetActive(target.alpha > 0.9f);
    }

    public void ToggleFullScreen()
    {
        AudioManager.instance.PlayUISound(0, 2, 0.05f);
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void Sensitivity()
    {
        if (PlayerController.instance == null) return;
        
        PlayerController.instance.sensitivity = sensitivitySlider.value / 50f;
    }
    
    public void MasterAudio()
    {
        mixer.SetFloat("masterVolume", masterSlider.value);
    }
    
    public void MusicAudio()
    {
        mixer.SetFloat("musicVolume", musicSlider.value);
    }
    
    public void SFXAudio()
    {
        mixer.SetFloat("sfxVolume", sfxSlider.value);
    }

    public void ChangeSubtitlesSize()
    {
        if (subtitles == null) return;
        
        subtitles.fontSize = subtitlesSlider.value;
    }
}

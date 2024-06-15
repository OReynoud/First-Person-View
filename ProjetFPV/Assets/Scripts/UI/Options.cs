using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider subtitlesSlider;
    [SerializeField] private TextMeshProUGUI demoSubtitles;
    [SerializeField] private Toggle subtitlesOff;
    [SerializeField] private Toggle fullScreenToggle;

    [SerializeField] private TextMeshProUGUI sensitivityText;
    [SerializeField] private TextMeshProUGUI masterText;
    [SerializeField] private TextMeshProUGUI musicText;
    [SerializeField] private TextMeshProUGUI sfxText;
    [SerializeField] private TextMeshProUGUI subtitlesText;

    [SerializeField] private GameObject inGameSubtitles;

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
            PlayerPrefs.SetInt("SubtitlesSize", 80);
        }
        if (!PlayerPrefs.HasKey("SubtitlesOff"))
        {
            PlayerPrefs.SetInt("SubtitlesOff", 0);
        }
        if (!PlayerPrefs.HasKey("FullScreen"))
        {
            PlayerPrefs.SetInt("FullScreen", 1);
        }

        sensitivitySlider.value = PlayerPrefs.GetInt("Sensitivity");
        masterSlider.value = PlayerPrefs.GetInt("MasterVolume");
        musicSlider.value = PlayerPrefs.GetInt("MusicVolume");
        sfxSlider.value = PlayerPrefs.GetInt("SFXVolume");
        subtitlesSlider.value = PlayerPrefs.GetInt("SubtitlesSize");
        subtitlesOff.isOn = PlayerPrefs.GetInt("SubtitlesOff") == 1;
        fullScreenToggle.isOn = PlayerPrefs.GetInt("FullScreen") == 1;

        sensitivityText.text = GiveNumber(sensitivitySlider);
        masterText.text = GiveNumber(masterSlider);
        musicText.text = GiveNumber(musicSlider);
        sfxText.text = GiveNumber(sfxSlider);
        subtitlesText.text = GiveNumber(subtitlesSlider);
        
        demoSubtitles.fontSize = PlayerPrefs.GetInt("SubtitlesSize");
        
        demoSubtitles.gameObject.SetActive(PlayerPrefs.GetInt("SubtitlesOff") == 0);
        
        inGameSubtitles.SetActive(PlayerPrefs.GetInt("SubtitlesOff") == 0);
    }

    string GiveNumber(Slider slider)
    {
        return ((int)(100 - ((slider.maxValue - slider.value) / (slider.maxValue - slider.minValue) * 100f))).ToString();
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
        sensitivityText.text = GiveNumber(sensitivitySlider);
        
        if (PlayerController.instance == null) return;
        
        PlayerController.instance.sensitivity = sensitivitySlider.value / 50f;
    }
    
    public void MasterAudio()
    {
        mixer.SetFloat("masterVolume", masterSlider.value);
        masterText.text = GiveNumber(masterSlider);
    }
    
    public void MusicAudio()
    {
        mixer.SetFloat("musicVolume", musicSlider.value);
        musicText.text = GiveNumber(musicSlider);
    }
    
    public void SFXAudio()
    {
        mixer.SetFloat("sfxVolume", sfxSlider.value);
        sfxText.text = GiveNumber(sfxSlider);
    }

    public void ChangeSubtitlesSize()
    {
        demoSubtitles.fontSize = subtitlesSlider.value;
        subtitlesText.text = GiveNumber(subtitlesSlider);
    }

    public void DisableSubtitles()
    {
        AudioManager.instance.PlayUISound(0, 2, 0.05f);
        demoSubtitles.gameObject.SetActive(!demoSubtitles.gameObject.activeInHierarchy);

        if (inGameSubtitles == null) return;
        inGameSubtitles.SetActive(!inGameSubtitles.gameObject.activeInHierarchy);
    }

    private void OnDisable()
    {
        PlayerPrefs.SetInt("Sensitivity", (int)sensitivitySlider.value);
        PlayerPrefs.SetInt("MasterVolume", (int)masterSlider.value);
        PlayerPrefs.SetInt("MusicVolume", (int)musicSlider.value);
        PlayerPrefs.SetInt("SFXVolume", (int)sfxSlider.value);
        PlayerPrefs.SetInt("SubtitlesSize", (int)subtitlesSlider.value);
        PlayerPrefs.SetInt("SubtitlesOff", subtitlesOff.isOn ? 1 : 0);
        PlayerPrefs.SetInt("FullScreen", fullScreenToggle.isOn ? 1 : 0);
    }
}

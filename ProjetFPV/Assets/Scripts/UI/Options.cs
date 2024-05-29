using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public CanvasGroup optionsCanva;
    [SerializeField] private InGamePause inGamePauseScript;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

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

        sensitivitySlider.value = PlayerPrefs.GetInt("Sensitivity");
        masterSlider.value = PlayerPrefs.GetInt("MasterVolume");
        musicSlider.value = PlayerPrefs.GetInt("MusicVolume");
        sfxSlider.value = PlayerPrefs.GetInt("SFXVolume");
    }
    
    public void OpenOptions()
    {
        optionsCanva.gameObject.SetActive(true);
        optionsCanva.DOFade(1f, 0.5f);
    }

    public void CloseOptions()
    {
        PlayerPrefs.SetInt("Sensitivity", (int)sensitivitySlider.value);
        PlayerPrefs.SetInt("MasterVolume", (int)masterSlider.value);
        PlayerPrefs.SetInt("MusicVolume", (int)musicSlider.value);
        PlayerPrefs.SetInt("SFXVolume", (int)sfxSlider.value);
        
        optionsCanva.DOFade(0f, 0.2f).OnComplete(()=>optionsCanva.gameObject.SetActive(false));
    }

    public void ToggleFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void Sensitivity()
    {
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
}
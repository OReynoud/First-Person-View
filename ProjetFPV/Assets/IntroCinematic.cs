using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroCinematic : MonoBehaviour
{
    [SerializeField] private CanvasGroup blackScreen;
    [SerializeField] private CanvasGroup menuCanva;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject radio;
    [SerializeField] private Volume oldVolume;
    [SerializeField] private Volume newVolume;
    [SerializeField] private GameObject camera;
    [SerializeField] private Light[] lights;
    [SerializeField] private CameraShake camShake;
    [SerializeField] private ParticleSystem rain;
    [SerializeField] private AudioSource rainSound;
    [SerializeField] private AudioSubtitles subtitlesScript;
    private bool canLoadTheScene;

    [SerializeField] private RectTransform[] blackLines;

    void Start()
    {
        StartCoroutine(LoadAsyncScene());
    }
    
    IEnumerator LoadAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Habillage_02");
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f && canLoadTheScene)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }
    
    public void FadeToBlack(float time)
    {
        blackScreen.DOFade(1f, time).OnComplete(() => canLoadTheScene = true);
    }

    public void StartIntroEasy()
    {
        PlayerPrefs.SetInt("isReloadingSave", 0);
        PlayerPrefs.SetInt("difficulty", 1);
        StartCoroutine(Intro());
    }

    public void StartIntroHard()
    {
        PlayerPrefs.SetInt("isReloadingSave", 0);
        PlayerPrefs.SetInt("difficulty", 0);
        StartCoroutine(Intro());
    }

    private IEnumerator Intro()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;

        yield return new WaitForSeconds(0.6f);
        
        var target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        target.transform.position = camera.transform.position;
        target.transform.rotation = camera.transform.rotation;
        target.transform.LookAt(radio.transform);
        target.SetActive(false);
        
        var t = 0f;
        var startRot = camera.transform.rotation;
        
        menu.SetActive(false);
        menuCanva.DOFade(0f, 2f);

        StartCoroutine(Fade());
        StartCoroutine(LightsOut());

        foreach (var line in blackLines)
        {
            line.DOAnchorPosY(0, 3f);
        }
        
        yield return new WaitForSeconds(0.2f);
        
        AudioManager.instance.PlaySound(4, 0, radio, 0f, false);
        StartCoroutine(Sub());
        
        while (t < 60f)
        {
            t += Time.deltaTime;
            
            oldVolume.weight = Mathf.Lerp(1, 0, t / 50f);
            newVolume.weight = Mathf.Lerp(0, 1, t / 50f);
            camera.transform.rotation = Quaternion.Lerp(startRot, target.transform.rotation, t / 30f);
            camera.transform.DOMove(camera.transform.position + camera.transform.forward * (Time.deltaTime * 0.035f), Time.deltaTime);
            
            yield return null;
        }
    }

    private IEnumerator Sub()
    {
        yield return new WaitForSeconds(0.2f);
        
        subtitlesScript.StartTimer(0);
    }

    private IEnumerator Fade()
    {
        yield return new WaitForSeconds(50f);
        lights[2].intensity = 0;
        yield return new WaitForSeconds(3f);
        FadeToBlack(4f);
    }

    private IEnumerator LightsOut()
    {
        yield return new WaitForSeconds(34.5f);
        
        camShake.ShakeOneShot(0);
        yield return new WaitForSeconds(0.5f);
        
        StartCoroutine(Rain());
        
        for (int i = 0; i < 12; i++)
        {
            var t = 0f;

            var currentLight = lights[0].intensity;
            var targetLight = 0f;
            var radioLight = 0f;
            var wallLight = 0f;
            
            targetLight = currentLight >= 1f ? Random.Range(8f, 12f) : Random.Range(30f, 36f);
            radioLight = currentLight >= 1f ? Random.Range(0.2f, 0.6f) : Random.Range(1f, 1f);
            wallLight = currentLight >= 1f ? Random.Range(0.4f, 1.8f) : Random.Range(2f, 2.5f);

            var time = Random.Range(0.03f, 0.08f);

            while (t<time)
            {
                t += Time.deltaTime;

                foreach (var light in lights)
                {
                   light.intensity = Mathf.Lerp(currentLight, targetLight, t / time);
                   light.intensity = Mathf.Lerp(currentLight, radioLight, t / time);
                   light.intensity = Mathf.Lerp(currentLight, wallLight, t / time);
                }
            
                yield return null;
            }
            
            yield return new WaitForSeconds(Random.Range(0.02f, 0.08f));
        }
    }

    private IEnumerator Rain()
    {
        rainSound.enabled = true;
        var rainEmission = rain.emission;
        var x = 0f;
        
        yield return new WaitForSeconds(3f);

        var t = 0f;

        while (t < 10f)
        {
            t += Time.deltaTime;

            x = Mathf.Lerp(0, 1000, t / 10f);
            rainSound.volume = Mathf.Lerp(0f, 0.15f, t / 10f);
            rainEmission.rateOverTime = x;
            
            yield return null;
        }
    }
}

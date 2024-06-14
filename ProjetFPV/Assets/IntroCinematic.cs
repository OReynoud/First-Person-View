using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

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

    public void FadeToBlack()
    {
        blackScreen.DOFade(1f, 4f).OnComplete(()=>SceneManager.LoadScene("Habillage_02"));
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
        
        yield return new WaitForSeconds(0.2f);

        AudioManager.instance.PlaySound(4, 0, radio, 0f, false);

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

    private IEnumerator Fade()
    {
        yield return new WaitForSeconds(53f);
        
        FadeToBlack();
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
            
            targetLight = currentLight >= 1f ? Random.Range(8f, 12f) : Random.Range(30f, 36f);

            var time = Random.Range(0.03f, 0.08f);

            while (t<time)
            {
                t += Time.deltaTime;

                foreach (var light in lights)
                {
                   light.intensity = Mathf.Lerp(currentLight, targetLight, t / time);
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

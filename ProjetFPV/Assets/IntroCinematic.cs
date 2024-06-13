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
    private GameObject camera;
    [SerializeField] private Light[] lights;

    void Start()
    {
        camera = Camera.main.gameObject;
    }

    public void FadeToBlack()
    {
        blackScreen.DOFade(1f, 5f).OnComplete(()=>SceneManager.LoadScene("Habillage_02"));
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
        menuCanva.DOFade(0f, 3f);

        StartCoroutine(Fade());
        StartCoroutine(LightsOut());
        
        yield return new WaitForSeconds(0.2f);

        AudioManager.instance.PlaySound(4, 0, camera, 0f, false);

        while (t < 60f)
        {
            t += Time.deltaTime;
            
            oldVolume.weight = Mathf.Lerp(1, 0, t / 50f);
            newVolume.weight = Mathf.Lerp(0, 1, t / 50f);
            camera.transform.rotation = Quaternion.Lerp(startRot, target.transform.rotation, t / 40f);
            camera.transform.DOMove(camera.transform.position + camera.transform.forward * (Time.deltaTime * 0.03f), Time.deltaTime);
            
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
        yield return new WaitForSeconds(35f);

        var t = 0f;
        
        while (t < 0.05f)
        {
            t += Time.deltaTime;

            foreach (var light in lights)
            {
                light.intensity = Mathf.Lerp(1.5f, 0.4f, t / 0.05f);
            }
            
            yield return null;
        }

        t = 0f;

        yield return new WaitForSeconds(0.1f);
        
        while (t < 0.08f)
        {
            t += Time.deltaTime;

            foreach (var light in lights)
            {
                light.intensity = Mathf.Lerp(0.4f, 1.8f, t / 0.08f);
            }
            
            yield return null;
        }

        t = 0f;

        yield return new WaitForSeconds(0.05f);
        
        while (t < 0.05f)
        {
            t += Time.deltaTime;

            foreach (var light in lights)
            {
                light.intensity = Mathf.Lerp(1.8f, 0.2f, t / 0.05f);
            }
            
            yield return null;
        }

        t = 0f;

        yield return new WaitForSeconds(0.1f);
        
        while (t < 0.02f)
        {
            t += Time.deltaTime;

            foreach (var light in lights)
            {
                light.intensity = Mathf.Lerp(0.2f, 1.2f, t / 0.02f);
            }
            
            yield return null;
        }
        
        t = 0f;

        yield return new WaitForSeconds(0.05f);
        
        while (t < 0.02f)
        {
            t += Time.deltaTime;

            foreach (var light in lights)
            {
                light.intensity = Mathf.Lerp(1.2f, 0.4f, t / 0.02f);
            }
            
            yield return null;
        }
        
        t = 0f;

        yield return new WaitForSeconds(0.04f);
        
        while (t < 0.04f)
        {
            t += Time.deltaTime;

            foreach (var light in lights)
            {
                light.intensity = Mathf.Lerp(0.4f, 1f, t / 0.04f);
            }
            
            yield return null;
        }
    }
}

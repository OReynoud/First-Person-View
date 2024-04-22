using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PH_Intro : MonoBehaviour
{
    public Image blackScreen;
    public TextMeshProUGUI subtitlesUI;
    public GameObject camera;
    public Transform startPos;
    public Transform endPos;
    public AudioSource audioSource;
    public string[] subtitles;
    public AudioClip[] audioClips;
    
    
    void Start()
    {
        blackScreen.DOFade(0f, 0.1f);
        subtitlesUI.text = "";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine(IntroCoroutine());
            StartCoroutine(SubtitleCoroutine());
        }
    }
    
    IEnumerator IntroCoroutine()
    {
        blackScreen.DOFade(1f, 0f);
        camera.transform.position = startPos.position;
        camera.transform.rotation = startPos.rotation;
        
        yield return new WaitForSeconds(5f);
        
        camera.transform.DOMove(endPos.position, 15f).SetEase(Ease.InQuad);
        blackScreen.DOFade(0f, 5f);
        
        yield return null;
    }

    IEnumerator SubtitleCoroutine()
    {
        yield return new WaitForSeconds(1f);
        
        audioSource.PlayOneShot(audioClips[0]);
        
        yield return new WaitForSeconds(.2f);
        
        subtitlesUI.text = subtitles[0];
        
        yield return new WaitForSeconds(3f);
        subtitlesUI.text = subtitles[1];
        
        yield return new WaitForSeconds(2.5f);
        subtitlesUI.text = subtitles[2];

        yield return null;
    }
}

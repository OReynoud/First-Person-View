using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CinematicManager : Singleton<CinematicManager>
{
    [SerializeField] private Camera introCamera;
    [SerializeField] private CanvasGroup playerUI;
    [SerializeField] private Image topLine;
    [SerializeField] private Image bottomLine;
    [SerializeField] private Image fullScreenBlack;

    public void StartCinematic()
    {
        StartCoroutine(StartCinematicCoroutine());
    }
    
    private IEnumerator StartCinematicCoroutine()
    {
        topLine.rectTransform.DOAnchorPosY(0, 2f);
        bottomLine.rectTransform.DOAnchorPosY(0, 2f);
        playerUI.DOFade(0f, 1f);

        yield return new WaitForSeconds(5f);
        
        topLine.rectTransform.DOAnchorPosY(topLine.rectTransform.rect.height, 2f);
        bottomLine.rectTransform.DOAnchorPosY(-bottomLine.rectTransform.rect.height, 2f);
        playerUI.DOFade(1f, 1f);
    }

    public void StartFullScreen(float speed)
    {
        playerUI.DOFade(0f, speed / 2f);
        fullScreenBlack.DOFade(1f, speed);
    }

    public void EndFullScreen(float speed)
    {
        playerUI.DOFade(1f, speed / 2f);
        fullScreenBlack.DOFade(0f, speed);
    }

    public void StartIntroCinematic()
    {
        StartCoroutine(IntroCoroutine());
    }

    private IEnumerator IntroCoroutine()
    {
        StartFullScreen(0f);
        StartCinematic();
        yield return new WaitForSeconds(2f);
        EndFullScreen(3f);
        introCamera.transform.DORotate(new Vector3(34, -18.8f, 0), 25f).SetEase(Ease.InQuad);
        introCamera.transform.DOMove(introCamera.transform.position - introCamera.transform.forward * 0.5f, 25f).SetEase(Ease.InQuad);
        yield return new WaitForSeconds(25f);
        StartFullScreen(0.05f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            StartIntroCinematic();
        }
    }
}

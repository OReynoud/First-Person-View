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
    [SerializeField] private GameObject arms;

    public void StartCinematic()
    {
        StartCoroutine(StartCinematicCoroutine());
    }
    
    private IEnumerator StartCinematicCoroutine()
    {
        var leftArm = arms.transform.GetChild(2);
        var rightArm = arms.transform.GetChild(3);
        
        var posLeft = leftArm.localPosition;
        var posRight = rightArm.localPosition;
        
        topLine.rectTransform.DOAnchorPosY(0, 2f);
        bottomLine.rectTransform.DOAnchorPosY(0, 2f);
        playerUI.DOFade(0f, 1f);
        leftArm.transform.DOLocalMove(posLeft + 0.6f * arms.transform.right - 0.8f * leftArm.transform.up, 2f).SetEase(Ease.InOutQuad);
        rightArm.transform.DOLocalMove(posRight - 0.6f * arms.transform.right - 0.8f * rightArm.transform.up, 2f).SetEase(Ease.InOutQuad);

        yield return new WaitForSeconds(5f);
        
        topLine.rectTransform.DOAnchorPosY(topLine.rectTransform.rect.height, 2f);
        bottomLine.rectTransform.DOAnchorPosY(-bottomLine.rectTransform.rect.height, 2f);
        playerUI.DOFade(1f, 1f);
        arms.transform.GetChild(2).transform.DOLocalMove(posLeft, 2f).SetEase(Ease.InOutQuad);
        arms.transform.GetChild(3).transform.DOLocalMove(posRight, 2f).SetEase(Ease.InOutQuad);
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

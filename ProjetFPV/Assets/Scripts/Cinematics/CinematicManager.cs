using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CinematicManager : Singleton<CinematicManager>
{
    [SerializeField] private CanvasGroup playerUI;
    [SerializeField] private Image topLine;
    [SerializeField] private Image bottomLine;
    [SerializeField] private Image fullScreenBlack;

    public void StartCinematic()
    {
        topLine.rectTransform.DOAnchorPosY(0, 2f);
        bottomLine.rectTransform.DOAnchorPosY(0, 2f);
        playerUI.DOFade(0f, 1f);
    }
    
    public void EndCinematic()
    {
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
}

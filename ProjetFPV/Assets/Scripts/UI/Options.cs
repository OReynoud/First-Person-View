using DG.Tweening;
using UnityEngine;

public class Options : MonoBehaviour
{
    [SerializeField] private CanvasGroup optionsCanva;
    [SerializeField] private InGamePause inGamePauseScript;
    
    public void OpenOptions()
    {
        optionsCanva.DOFade(1f, 1f);
    }

    public void CloseOptions()
    {
        optionsCanva.DOFade(0f, 1f);
    }
}

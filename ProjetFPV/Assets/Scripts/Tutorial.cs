using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Tutorial : Singleton<Tutorial>
{
    [SerializeField] private CanvasGroup tutorialCanva;
    [SerializeField] private TextMeshProUGUI tutorialText;
    
    public void DisplayTutorial(string tutoText)
    {
        tutorialText.text = tutoText;
        tutorialCanva.DOFade(1f, 0.5f);
    }

    public void HideTutorial()
    {
        tutorialCanva.DOFade(0f, 0.3f).OnComplete(() => tutorialText.text = "");
    }
}

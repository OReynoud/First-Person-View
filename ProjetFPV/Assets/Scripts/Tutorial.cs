using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Tutorial : Singleton<Tutorial>
{
    [SerializeField] private CanvasGroup tutorialCanva;
    [SerializeField] private TextMeshProUGUI tutorialText;
    private Tween tween;

    void Start()
    {
        if (PlayerPrefs.GetInt("isReloadingSave") == 0)
        {
            tutorialText.text = "Use ZQSD to move";
            tween = tutorialCanva.DOFade(1f, 0.5f);
        }
    }
    
    public void DisplayTutorial(string tutoText)
    {
        tween?.Kill();

        tutorialText.text = tutoText;
        tween = tutorialCanva.DOFade(1f, 0.5f);
    }

    public void HideTutorial()
    {
        tween?.Kill();
        
        tween = tutorialCanva.DOFade(0f, 0.3f).OnComplete(() => tutorialText.text = "");
    }
}

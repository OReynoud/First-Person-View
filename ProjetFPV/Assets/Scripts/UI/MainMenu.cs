using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Options optionsScript;
    [SerializeField] private Button continueGame;
    [SerializeField] private CanvasGroup pauseCanva;
    [SerializeField] private CanvasGroup difficultyCanva;
    [SerializeField] private CanvasGroup optionsCanva;
    [SerializeField] private CanvasGroup creditsCanva;
    private Coroutine coroutine;
    private float t;
    
    [SerializeField] private RectTransform blackArea;
    private Vector2 baseAnchorPos;

    [SerializeField] private GameObject camera;
    [SerializeField] private GameObject baseTarget;
    [SerializeField] private GameObject newGameTarget;
    [SerializeField] private GameObject optionsTarget;
    [SerializeField] private GameObject creditsTarget;

    [SerializeField] private IntroCinematic introScript;

    private bool cinematicStarted;
    [SerializeField] private float cinematicCutTimer;
    [SerializeField] private Image skipCircle;
    private float cT;
    private bool countingCutCinematic;
    private int selectedDifficulty;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        
        baseAnchorPos = blackArea.anchoredPosition;
        
        if (continueGame == null) return;

        if (!PlayerPrefs.HasKey("SavePosX"))
        {
            continueGame.interactable = false;
            continueGame.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.grey;
        }
        else
        {
            continueGame.interactable = true;
            continueGame.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
        }
    }

    void Update()
    {
        if (countingCutCinematic)
        {
            skipCircle.fillAmount = cT / cinematicCutTimer;
            
            cT += Time.deltaTime;
            if (cT >= cinematicCutTimer)
            {
                SkipIntro();
            }
        }
        
        if (t < 0) return;

        t -= Time.unscaledDeltaTime;
    }

    void SkipIntro()
    {
        switch (selectedDifficulty)
        {
            case 0:
                PlayerPrefs.SetInt("isReloadingSave", 0);
                PlayerPrefs.SetInt("difficulty", 0);
                break;
            case 1:
                PlayerPrefs.SetInt("isReloadingSave", 0);
                PlayerPrefs.SetInt("difficulty", 1);
                break;
        }
        
        introScript.FadeToBlack(1f);
    }
    
    public void Escape(InputAction.CallbackContext obj)
    {
        if (cinematicStarted)
        {
            if (obj.started)
            {
                countingCutCinematic = true;
            }
            else if (obj.canceled)
            {
                countingCutCinematic = false;
                cT = 0f;
            }
            return;
            
        }
        
        if (!obj.started || t > 0f) return;
        
        t = 0.5f;

        if (optionsCanva.alpha >= 1)
        {
            CloseOptions();
        }
        else if (difficultyCanva.alpha >= 1)
        {
            CloseDifficulty();
        }
        else if (creditsCanva.alpha >= 1)
        {
            CloseCredits();
        }
    }

    public void OpenDifficulty()
    {
        ExtendBlackArea();

        LookAtNewGame();
        
        difficultyCanva.gameObject.SetActive(true);
        difficultyCanva.DOFade(1f, 0.8f);
    }
    
    public void CloseDifficulty()
    {
        ReduceBlackArea();
        
        difficultyCanva.DOFade(0f, 0.1f).OnComplete(() => difficultyCanva.gameObject.SetActive(false));
    }

    public void OpenOptions()
    {
        LookAtOptions();
        
        ExtendBlackArea();

        optionsCanva.gameObject.SetActive(true);
        optionsCanva.DOFade(1f, 0.8f);
    }

    public void CloseOptions()
    {
        ReduceBlackArea();
        
        optionsCanva.DOFade(0f, 0.1f).OnComplete(() => optionsCanva.gameObject.SetActive(false));
    }

    public void Credits()
    {
        LookAtCredits();
        
        ExtendBlackArea();
        
        creditsCanva.gameObject.SetActive(true);
        creditsCanva.DOFade(1f, 0.8f);
    }

    public void CloseCredits()
    {
        ReduceBlackArea();
        
        creditsCanva.DOFade(0f, 0.1f).OnComplete(() => creditsCanva.gameObject.SetActive(false));
    }

    void ExtendBlackArea()
    {
        pauseCanva.DOFade(0f, 0.1f);
        
        blackArea.DOAnchorPosX(baseAnchorPos.x + 800, 0.8f);
    }

    void ReduceBlackArea()
    {
        LookAtBase();
        
        pauseCanva.DOFade(1f, 0.5f);
        
        blackArea.DOAnchorPosX(baseAnchorPos.x, 0.8f);
    }
    
    public void Quit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    public void ClickSound()
    {
        AudioManager.instance.PlayUISound(0, 2, 0.05f);
    }

    public void ContinueGame()
    {
        PlayerPrefs.SetInt("isReloadingSave", 1);

        SceneManager.LoadScene("Habillage_02");
    }
    
    #region LookAt
    
    void LookAtNewGame()
    {
        camera.transform.DOMove(newGameTarget.transform.position, 0.8f).SetEase(Ease.OutCubic);
        camera.transform.DORotate(newGameTarget.transform.eulerAngles, 0.8f).SetEase(Ease.OutCubic);
    }

    void LookAtOptions()
    {
        camera.transform.DOMove(optionsTarget.transform.position, 0.8f).SetEase(Ease.OutCubic);
        camera.transform.DORotate(optionsTarget.transform.eulerAngles, 0.8f).SetEase(Ease.OutCubic);
    }

    void LookAtCredits()
    {
        camera.transform.DOMove(creditsTarget.transform.position, 0.8f).SetEase(Ease.OutCubic);
        camera.transform.DORotate(creditsTarget.transform.eulerAngles, 0.8f).SetEase(Ease.OutCubic);
    }

    void LookAtBase()
    {
        camera.transform.DOMove(baseTarget.transform.position, 0.8f).SetEase(Ease.OutCubic);
        camera.transform.DORotate(baseTarget.transform.eulerAngles, 0.8f).SetEase(Ease.OutCubic);
    }
    
    #endregion
    
    #region Difficulties

    public void StartGameEasy()
    {
        selectedDifficulty = 1;
        cinematicStarted = true;
        LookAtBase();
        introScript.StartIntroEasy();
    }

    public void StartGameHard()
    {
        selectedDifficulty = 0;
        cinematicStarted = true;
        LookAtBase();
        introScript.StartIntroHard();
    }
    
    #endregion
}

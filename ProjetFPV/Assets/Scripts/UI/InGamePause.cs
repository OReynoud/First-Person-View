using DG.Tweening;
using Mechanics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGamePause : MonoBehaviour
{
    [SerializeField] private GameObject collectibleCamera;
    [SerializeField] private RectTransform blackArea;
    [SerializeField] private Image blackScreen;
    private Vector2 baseAnchorPos;
    [SerializeField] private CanvasGroup pauseCanva;
    [SerializeField] private CanvasGroup optionsCanva;
    private float t;

    void Start()
    {
        DOTween.Init();
        DOTween.defaultTimeScaleIndependent = true;
        baseAnchorPos = blackArea.anchoredPosition;
    }

    void Update()
    {
        if (t < 0) return;

        t -= Time.unscaledDeltaTime;
    }
    
    public void Escape(InputAction.CallbackContext obj)
    {
        if (!obj.started || t > 0f) return;

        if (collectibleCamera != null && collectibleCamera.activeInHierarchy) return;
        
        t = 1f;

        if (optionsCanva.alpha >= 1)
        {
            CloseOptions();
        }
        else if (pauseCanva.alpha >= 1)
        {
            Resume();
        }
        else
        {
            OpenPause();
        }
    }

    void OpenPause()
    {
        if (PlayerController.instance.isControled) return;
        
        ExtendBlackArea(1400);
        
        AudioManager.instance.MuffleSound();

        AudioManager.instance.PlayUISound(0, 0, 0f);

        pauseCanva.gameObject.SetActive(true);
        pauseCanva.DOFade(1f, 0.8f);
        
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Time.timeScale = 0;

        GameManager.instance.HideUI();
        PlayerController.instance.ImmobilizePlayer();
        PlayerController.instance.LockCam();
    }

    public void Resume()
    {
        AudioManager.instance.UnMuffleSound();
        
        ReduceBlackArea();

        pauseCanva.DOFade(0f, 0.1f).OnComplete(() => pauseCanva.gameObject.SetActive(false));
        
        AudioManager.instance.PlayUISound(0, 1, 0f);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameManager.instance.ShowUI();
        PlayerController.instance.ImmobilizePlayer();
        PlayerController.instance.LockCam();
    }
    
    void ExtendBlackArea(int value)
    {
        t = 1f;
        
        blackScreen.DOFade(0.8f, 0.5f);
        blackArea.DOAnchorPosX(baseAnchorPos.x + value, 0.8f);
    }
    
    void SmallReduceBlackArea()
    {
        t = 1f;
        
        blackArea.DOAnchorPosX(baseAnchorPos.x + 1400, 0.8f).OnComplete(() => pauseCanva.interactable = true);

        pauseCanva.gameObject.SetActive(true);
        pauseCanva.DOFade(1f, 0.8f);
    }
    
    void ReduceBlackArea()
    {
        t = 1f;
        
        blackScreen.DOFade(0f, 0.5f);
        blackArea.DOAnchorPosX(baseAnchorPos.x, 0.8f).OnComplete(() => Time.timeScale = 1f);
    }
    
    public void OpenOptions()
    {
        pauseCanva.interactable = false;
        
        ExtendBlackArea(2200);

        pauseCanva.DOFade(0f, 0.1f);
        optionsCanva.gameObject.SetActive(true);
        optionsCanva.DOFade(1f, 0.8f);
    }
    
    void CloseOptions()
    {
        SmallReduceBlackArea();
        
        optionsCanva.DOFade(0f, 0.1f).OnComplete(() => optionsCanva.gameObject.SetActive(false));
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
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
}
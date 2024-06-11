using System.Collections;
using DG.Tweening;
using Mechanics;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGamePause : MonoBehaviour
{
    [SerializeField] private CanvasGroup pauseCanva;
    [SerializeField] private Options optionsScript;
    private PlayerInput inputs;
    private InputActionMap currentControls;
    [SerializeField] private Button continueGame;
    [SerializeField] private CanvasGroup difficultyCanva;

    void Start()
    {
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

    public void Escape(InputAction.CallbackContext obj)
    {
        if (!obj.started) return;

        if (optionsScript.optionsCanva.gameObject.activeInHierarchy)
        {
            optionsScript.CloseOptions();
        }
        else if (difficultyCanva != null && difficultyCanva.alpha >= 1)
        {
            CloseDifficulty();
        }
        else if (pauseCanva == null)
        {
            return;
        }
        else if (pauseCanva.gameObject.activeInHierarchy)
        {
            Resume();
        }
        else
        {
            OpenPause();
        }
    }

    public void OpenPause()
    {
        if (PlayerController.instance.isControled) return;

        AudioManager.instance.MuffleSound();

        AudioManager.instance.PlayUISound(0, 0, 0f);

        pauseCanva.gameObject.SetActive(true);
        StartCoroutine(Fade(false, 1, pauseCanva));
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Time.timeScale = 0;

        GameManager.instance.HideUI();
        PlayerController.instance.ImmobilizePlayer();
        PlayerController.instance.LockCam();
    }

    public void OpenDifficulty()
    {
        difficultyCanva.gameObject.SetActive(true);
        difficultyCanva.DOFade(1f, 0.5f);
    }
    
    public void CloseDifficulty()
    {
        difficultyCanva.DOFade(0f, 0.5f).OnComplete(() => difficultyCanva.gameObject.SetActive(false));
    }

    public void Resume()
    {
        AudioManager.instance.UnMuffleSound();
        Time.timeScale = 1;

        AudioManager.instance.PlayUISound(0, 1, 0f);

        //pauseCanva.DOFade(0f, 0.2f).OnComplete(() => pauseCanva.gameObject.SetActive(false));
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        StartCoroutine(Fade(true, 0.2f, pauseCanva));

        GameManager.instance.ShowUI();
        PlayerController.instance.ImmobilizePlayer();
        PlayerController.instance.LockCam();
    }


    public IEnumerator Fade(bool fadeOut, float timer, CanvasGroup target)
    {
        var time = 0f;
        while (time < timer)
        {
            time += Time.unscaledDeltaTime;
            if (fadeOut)
            {
                target.alpha = Mathf.Lerp(1, 0, time / timer);
            }
            else
            {
                target.alpha = Mathf.Lerp(0, 1, time  / timer);
            }
            yield return null;
        }

        pauseCanva.gameObject.SetActive(target.alpha > 0.9f);
    }

    public void OpenOptions()
    {
        optionsScript.OpenOptions();
    }

    public void CloseOptions()
    {
        optionsScript.CloseOptions();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ClickSound()
    {
        AudioManager.instance.PlayUISound(0, 2, 0.05f);
    }

    public void NewGameEasy()
    {
        PlayerPrefs.SetInt("isReloadingSave", 0);
        PlayerPrefs.SetInt("difficulty", 1);

        SceneManager.LoadScene("LevelDesign");
    }

    public void NewGameHard()
    {
        PlayerPrefs.SetInt("isReloadingSave", 0);
        PlayerPrefs.SetInt("difficulty", 0);

        SceneManager.LoadScene("LevelDesign");
    }

    public void ContinueGame()
    {
        PlayerPrefs.SetInt("isReloadingSave", 1);

        SceneManager.LoadScene("LevelDesign");
    }
}
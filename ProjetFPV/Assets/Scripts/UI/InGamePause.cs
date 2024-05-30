using DG.Tweening;
using Mechanics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InGamePause : MonoBehaviour
{
    [SerializeField] private CanvasGroup pauseCanva;
    [SerializeField] private Options optionsScript;
    private PlayerInput inputs;
    private InputActionMap currentControls;

    public void Escape(InputAction.CallbackContext obj)
    {
        if (!obj.started) return;
        
        if (optionsScript.optionsCanva.gameObject.activeInHierarchy)
        {
            optionsScript.CloseOptions();
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
        pauseCanva.DOFade(1f, 0.5f);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        
        GameManager.instance.HideUI();
        PlayerController.instance.ImmobilizePlayer();
        PlayerController.instance.LockCam();
    }

    public void Resume()
    {
        AudioManager.instance.UnMuffleSound();
        
        AudioManager.instance.PlayUISound(0, 1, 0f);
        
        pauseCanva.DOFade(0f, 0.2f).OnComplete(()=>pauseCanva.gameObject.SetActive(false));
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        GameManager.instance.ShowUI();
        PlayerController.instance.ImmobilizePlayer();
        PlayerController.instance.LockCam();
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
}

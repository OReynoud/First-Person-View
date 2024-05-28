using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGamePause : MonoBehaviour
{
    [SerializeField] private CanvasGroup pauseCanva;
    [SerializeField] private Options optionsScript;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            OpenPause();
        }
    }
    
    public void OpenPause()
    {
        pauseCanva.DOFade(1f, 1f);
        // Stop le temps
        // Activer le curseur
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void Resume()
    {
        pauseCanva.DOFade(0f, 1f);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Application.Quit();
    }
}

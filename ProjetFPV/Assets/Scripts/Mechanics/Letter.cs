using System;
using DG.Tweening;
using Mechanics;
using TMPro;
using UnityEngine;

public class Letter : MonoBehaviour, ICanInteract
{
    [SerializeField] private GameObject collectibleCamera;
    [TextArea] [SerializeField] private string translatedText;
    [SerializeField] private CanvasGroup collectibleCanva;
    [SerializeField] private TextMeshProUGUI translatedTextUI;
    private bool isDisplayed;
    private GameObject uiObject;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        collectibleCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        uiObject = transform.GetChild(1).gameObject;
    }

    public void Interact(Vector3 dir)
    {
        if(isDisplayed)
        {
            CloseLetterInFullScreen();
        }
        else
        {
            OpenLetterInFullScreen();
        }

        isDisplayed = !isDisplayed;
    }

    public void ShowContext()
    {
        GameManager.instance.interactText.text = isDisplayed ? "" : "[E] Lire";
    }

    public float rotationSpeed = 10.0f;

    void Update()
    {
        // Obtenir la position de la souris
        Vector3 mousePosition = Input.mousePosition;

        // Convertir la position de la souris en coordonnées du monde
        Vector3 objectScreenPosition = cam.WorldToScreenPoint(uiObject.transform.position);

        // Calculer l'angle de rotation en fonction de la position de la souris sur l'axe X uniquement
        float angleX = (mousePosition.x - objectScreenPosition.x) / Screen.width * rotationSpeed;

        // Appliquer la rotation à l'objet autour de l'axe X uniquement
        uiObject.transform.Rotate(Vector3.right, -angleX * Time.deltaTime, Space.World);
    }

    void OpenLetterInFullScreen()
    {
        // Retirer les contrôles du joueur
        
        Cursor.lockState = CursorLockMode.Confined;
        
        AudioManager.instance.PlaySound(3, 11, gameObject, 0.1f, false);
        collectibleCamera.transform.position = transform.GetChild(2).position;
        collectibleCamera.transform.rotation = transform.GetChild(2).rotation;
        translatedTextUI.text = translatedText + "\n\n [E] Fermer";
        collectibleCanva.DOFade(1f, 0.5f);
        collectibleCamera.SetActive(true);
    }

    void CloseLetterInFullScreen()
    {
        // Remettre les contrôles du joueur
        
        Cursor.lockState = CursorLockMode.Locked;
        
        AudioManager.instance.PlaySound(3, 11, gameObject, 0.1f, false);
        collectibleCanva.DOFade(0f, 0.5f);
        collectibleCamera.SetActive(false);
    }
}

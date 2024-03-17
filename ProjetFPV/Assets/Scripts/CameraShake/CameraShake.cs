using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class CameraShake : MonoBehaviour
{
    private GameObject cam;
    private int currentIndex;
    private Coroutine currentCoroutine;
    private Vector3 originalPos;
    private Quaternion originalRot; 
    
    public List<Shakes> shakesPresets;

    void Start()
    {
        cam = Camera.main.gameObject;
    }

    void Update() //TEMPORAIRE
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ShakeOneShot(0);
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartInfiniteShake(0);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
             StopInfiniteShake();
        }
    }

    public void ShakeOneShot(int index)
    {
        if (shakesPresets[index].moveForce >= shakesPresets[currentIndex].moveForce)
        {
            Debug.Log("Lancement d'un shake One shot");
            
            StartCoroutine(ShakeCoroutine(index, false));
        }   
    }

    public void StartInfiniteShake(int index)
    {
        if (shakesPresets[index].moveForce >= shakesPresets[currentIndex].moveForce)
        {
            Debug.Log("Lancement d'un shake Infini");
            
            currentCoroutine = StartCoroutine(ShakeCoroutine(index, true));
        }  
    }

    public void StopInfiniteShake()
    {
        if (currentCoroutine is not null)
        {
            Debug.Log("Lancement d'un stop Couroutine");
            
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
            StartCoroutine(StopShaking(currentIndex));
        }
    }
    
    IEnumerator ShakeCoroutine(int index, bool infinite)
    {
        Debug.Log("Début shake");
        
        currentIndex = index;
        
        originalPos = cam.transform.localPosition;
        originalRot = cam.transform.localRotation;
        float elapsed = 0f;

        float duration = infinite ? 1000 : shakesPresets[index].duration;

        #region FadeIn and Shake
        
        while (elapsed < duration)
        {
            float mF = shakesPresets[index].moveForce / 100 * Math.Min(elapsed / shakesPresets[index].fadeIn, 1f);
            float rF = shakesPresets[index].rotationForce / 100 * Math.Min(elapsed / shakesPresets[index].fadeIn, 1f);

            Debug.Log("Move Force : " + mF + " || Rotate Force : " + rF);

            float x = Random.Range(-1f, 1f) * mF;
            float y = Random.Range(-1f, 1f) * mF;

            float rotX = Random.Range(-1f, 1f) * rF;
            float rotY = Random.Range(-1f, 1f) * rF;
            float rotZ = Random.Range(-1f, 1f) * rF;

            cam.transform.localPosition = new Vector3(x, y, originalPos.z);
            cam.transform.RotateAround(cam.transform.position, Vector3.up, rotX);
            cam.transform.RotateAround(cam.transform.position, Vector3.forward, rotY);
            cam.transform.RotateAround(cam.transform.position, Vector3.right, rotZ);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(StopShaking(index));

        #endregion
    }

    IEnumerator StopShaking(int index)
    {
        Debug.Log("Stop shake");
        
        float elapsed = 0f;

        #region FadeOut
        
        while (elapsed < shakesPresets[index].fadeOut)
        {
            float mF = shakesPresets[index].moveForce / 100 * ((shakesPresets[index].fadeOut - elapsed) / shakesPresets[index].fadeOut);
            float rF = shakesPresets[index].rotationForce / 100 * ((shakesPresets[index].fadeOut - elapsed) / shakesPresets[index].fadeOut);
            
            Debug.Log("Move Force : " + mF + " || Rotate Force : " + rF);

            float x = Random.Range(-1f, 1f) * mF;
            float y = Random.Range(-1f, 1f) * mF;

            float rotX = Random.Range(-1f, 1f) * rF;
            float rotY = Random.Range(-1f, 1f) * rF;
            float rotZ = Random.Range(-1f, 1f) * rF;

            cam.transform.localPosition = new Vector3(x, y, originalPos.z);
            cam.transform.RotateAround(originalPos, Vector3.up, rotX);
            cam.transform.RotateAround(originalPos, Vector3.forward, rotY);
            cam.transform.RotateAround(originalPos, Vector3.right, rotZ);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        cam.transform.localPosition = originalPos;
        cam.transform.localRotation = originalRot;
        currentIndex = 0;

        #endregion
    }
}

[Serializable]
public class Shakes
{
    public string name;
    public float moveForce;
    public float rotationForce;
    public float duration;
    public float fadeIn;
    public float fadeOut;
}

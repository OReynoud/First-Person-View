using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class CameraShake : MonoBehaviour
{
    private GameObject cam;
    private float currentShake = -1f;
    private Coroutine currentCoroutine;
    
    public List<Shakes> shakesPresets;

    void Start()
    {
        cam = Camera.main.gameObject;
    }

    void Update() //DEBUG
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
        if (shakesPresets[index].moveForce >= currentShake)
        {
            StartCoroutine(ShakeCoroutine(index, false));
        }   
    }

    public void StartInfiniteShake(int index)
    {
        if (index >= currentShake)
        {
            currentCoroutine = StartCoroutine(ShakeCoroutine(index, true));
        }  
    }

    public void StopInfiniteShake()
    {
        if (currentCoroutine is not null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
            currentShake = -1f;
        }
    }
    
    IEnumerator ShakeCoroutine(int index, bool infinite)
    {
        Vector3 originalPos = cam.transform.localPosition;
        Quaternion originalRot = cam.transform.localRotation;
        float elapsed = 0f;

        float duration = infinite ? 1000 : shakesPresets[index].duration;

        #region FadeIn and Shake
        
        while (elapsed < duration)
        {
            float mF = shakesPresets[index].moveForce / 100 * Math.Min(elapsed / shakesPresets[index].fadeIn, 1f);
            float rF = shakesPresets[index].rotationForce / 100 * Math.Min(elapsed / shakesPresets[index].fadeIn, 1f);
            
            currentShake = mF;

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

        #endregion
        
        elapsed = 0f;

        #region FadeOut
        
        while (elapsed < shakesPresets[index].fadeOut)
        {
            float mF = shakesPresets[index].moveForce / 100 * ((shakesPresets[index].fadeOut - elapsed) / shakesPresets[index].fadeOut);
            float rF = shakesPresets[index].rotationForce / 100 * ((shakesPresets[index].fadeOut - elapsed) / shakesPresets[index].fadeOut);
            
            currentShake = mF;

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
        
        #endregion

        cam.transform.localPosition = originalPos;
        cam.transform.localRotation = originalRot;
        currentShake = -1f;
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

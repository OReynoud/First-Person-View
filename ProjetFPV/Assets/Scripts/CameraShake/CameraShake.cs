using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShake : Singleton<CameraShake>
{
    private GameObject cam;
    private int currentIndex;
    private Coroutine infCoroutine;
    private Coroutine oneShotCoroutine;
    private Vector3 originalPos;
    private Quaternion originalRot; 
    
    public List<Shakes> shakesPresets;

    public int index;
    
    
    //Yo, Oscar est passer par la. J'ai surtout modif de la logique pour faire en sorte que chaque coroutine qui est en cours est stocké, puis nullifié quand il est terminé
    public override void Awake()
    {
        base.Awake();
        cam = Camera.main.gameObject;
    }

    void Update() //TEMPORAIRE
    {
        // if (Input.GetKeyDown(KeyCode.K))
        // {
        //     ShakeOneShot(index);
        // }
        // if (Input.GetKeyDown(KeyCode.L))
        // {
        //     StartInfiniteShake(index);
        // }
        if (Input.GetKeyDown(KeyCode.Semicolon))
        {
             StopInfiniteShake();
        }
    }

    public void ShakeOneShot(int index)
    {
        if (oneShotCoroutine != null) //Annule la coroutine en cours (si yen a une) et reset la cam
        {
            ResetCoroutine();
        }
        
        oneShotCoroutine = StartCoroutine(ShakeCoroutine(index, false)); //On stocke cette coroutine aussi, comme ca si jamais yen a un autre qui est trigger, on peut tej celui en cours
        
        //Pas besoin de ce check de ce que jai pu comprendre
        //if (shakesPresets[index].moveForce >= shakesPresets[currentIndex].moveForce) { }   
    }

    public void StartInfiniteShake(int index)
    {      
        if (infCoroutine != null)
        {
            StopCoroutine(infCoroutine);
            infCoroutine = null;
            cam.transform.localPosition = originalPos;
            cam.transform.localRotation = originalRot;
        }
        
        infCoroutine = StartCoroutine(ShakeCoroutine(index, true));
        
        //Pas besoin de ce check de ce que jai pu comprendre
        //if (shakesPresets[index].moveForce >= shakesPresets[currentIndex].moveForce) { }  
    }

    public void StopInfiniteShake()
    {
        if (infCoroutine is not null)
        {
            StopCoroutine(infCoroutine);
            infCoroutine = null;
            StartCoroutine(StopShaking(currentIndex, infCoroutine));
        }
    }
    
    IEnumerator ShakeCoroutine(int index, bool infinite)
    {
        currentIndex = index;
        
        originalPos = cam.transform.localPosition;
        originalRot = cam.transform.localRotation;
        float elapsed = 0f;

        float duration = infinite ? 1000 : shakesPresets[index].duration;
        float fadeIn = shakesPresets[index].fadeIn;
        if (fadeIn <= 0) //Pour eviter de diviser par zero, on met une valeur proche de zero
        {
            fadeIn = 0.00001f;
        }
        while (elapsed < duration)
        {
            if (Time.timeScale >= 0.1f)
            {
                float mF = shakesPresets[index].moveForce / 100 * Math.Min(elapsed / fadeIn, 1f);
                float rF = shakesPresets[index].rotationForce / 100 * Math.Min(elapsed / fadeIn, 1f);

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
            }

            yield return null;
        }

        StartCoroutine(StopShaking(index, infinite ? infCoroutine : oneShotCoroutine));
    }

    IEnumerator StopShaking(int index, Coroutine routine) // On track le type de coroutine pour savoir lequel on nullifie
    {
        float elapsed = 0f;
        
        while (elapsed < shakesPresets[index].fadeOut)
        {
            if (Time.timeScale >= 0.1f)
            {
                float mF = shakesPresets[index].moveForce / 100 *
                           ((shakesPresets[index].fadeOut - elapsed) / shakesPresets[index].fadeOut);
                float rF = shakesPresets[index].rotationForce / 100 *
                           ((shakesPresets[index].fadeOut - elapsed) / shakesPresets[index].fadeOut);

                float x = Random.Range(-1f, 1f) * mF;
                float y = Random.Range(-1f, 1f) * mF;

                float rotX = Random.Range(-1f, 1f) * rF;
                float rotY = Random.Range(-1f, 1f) * rF;
                float rotZ = Random.Range(-1f, 1f) * rF;

                cam.transform.localPosition = new Vector3(x, y, originalPos.z);
                cam.transform.RotateAround(originalPos, Vector3.up, rotX);
                cam.transform.RotateAround(originalPos, Vector3.forward, rotY);
                cam.transform.RotateAround(originalPos, Vector3.right, rotZ);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
        
        cam.transform.localPosition = originalPos;
        cam.transform.localRotation = originalRot;
        currentIndex = 0;
        NullifyCurrentCoroutine(routine); 
    }

    void NullifyCurrentCoroutine(Coroutine routine)
    {
        routine = null;
    }

    public void ResetCoroutine()
    {
        if (oneShotCoroutine != null) StopCoroutine(oneShotCoroutine);
        oneShotCoroutine = null;
        cam.transform.localPosition = originalPos;
        cam.transform.localRotation = originalRot;
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

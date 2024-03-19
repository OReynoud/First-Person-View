using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private CanvasGroup bodyHitMarker;
    [SerializeField] private CanvasGroup headHitMarker;
    [SerializeField] private Image staminaBar;
    [SerializeField] private List<TextMeshProUGUI> ammoText;
    
    
    [SerializeField] private float hitMarkerFadeTime;
    [SerializeField] private float timeForEnemyToRespawn;

    private Coroutine bodyHit;
    private Coroutine headHit;
    
    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        bodyHitMarker.alpha = 0;
        headHitMarker.alpha = 0;
    }

    public void Update()
    {
        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        ammoText[0].text = PlayerController.instance.currentAmmo.ToString();
        ammoText[1].text = PlayerController.instance.magSize.ToString();
        ammoText[2].text = PlayerController.instance.inventoryAmmo.ToString();
    }

    public void HitMark(bool headshot)
    {
        if (headshot)
        {
            if (headHit != null)StopCoroutine(headHit);
            headHit = StartCoroutine(FadeHitMark(true));
        }
        else
        {
            if (bodyHit != null)StopCoroutine(bodyHit);
            headHit = StartCoroutine(FadeHitMark(false));
        }
    }

    public float UpdatePlayerStamina(float current, float max, float increment)
    {
        current += increment;
        
        if (current > max) current = max;
        if (current < 0) current = 0;
        
        staminaBar.fillAmount = current / max;
        return current;
    }
    
    // Update is called once per frame
    private IEnumerator FadeHitMark(bool headshot)
    {
        var timer = Time.time + hitMarkerFadeTime;
        if (headshot)
        {
            headHitMarker.alpha = 1;
            while (Time.time < timer)
            {
                headHitMarker.alpha = (timer - Time.time)/hitMarkerFadeTime;
                yield return null;
            }
            
            headHitMarker.alpha = 0;
        }
        else
        {
            bodyHitMarker.alpha = 1;
            while (Time.time < timer)
            {
                bodyHitMarker.alpha = (timer - Time.time)/hitMarkerFadeTime;
                yield return null;
            }

            bodyHitMarker.alpha = 0;
        }
    }

    public void Respawn(Enemy enemy)
    {
        StartCoroutine(RespawnCoroutine(enemy));
    }
    private IEnumerator RespawnCoroutine(Enemy enemy)
    {
        yield return new WaitForSeconds(timeForEnemyToRespawn);
        enemy.gameObject.SetActive(true);
        enemy.Start();
        if (enemy.isImmobile) yield break;
        
        try
        {
            enemy.transform.position = enemy.respawnPoint.position;
        }
        catch (Exception e)
        {
            Debug.LogError("T'as oublié de mettre un respawn point en réference");
       
        }
    }
}

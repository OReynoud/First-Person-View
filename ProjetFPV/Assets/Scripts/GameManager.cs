using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] private CanvasGroup bodyHitMarker;
    [SerializeField] private CanvasGroup headHitMarker;
    [SerializeField] private Image staminaBar;
    
    [SerializeField] private float hitMarkerFadeTime;

    private Coroutine bodyHit;
    private Coroutine headHit;
    
    // Start is called before the first frame update
    public override void Awake()
    {
        base.Awake();
        bodyHitMarker.alpha = 0;
        headHitMarker.alpha = 0;
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
}

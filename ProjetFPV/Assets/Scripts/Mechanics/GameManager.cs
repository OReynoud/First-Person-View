using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mechanics
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private CanvasGroup playerUI;
        [SerializeField] private CanvasGroup bodyHitMarker;
        [SerializeField] private CanvasGroup headHitMarker;
        [SerializeField] private CanvasGroup crosshairTK;
        [SerializeField] private Image crosshairBase;
        [SerializeField] private Image baseInkBar;
        [SerializeField] private Image surplusInkBar;
        [SerializeField] private Image[] segments;
        [SerializeField] private TextMeshProUGUI healPackText;
        [SerializeField] public TextMeshProUGUI interactText;
        [SerializeField] public CanvasGroup gameOver;
        [SerializeField] public GameObject inkStainDecal;
        [SerializeField] public ParticleSystem[] VFX_EnemyHit;
        private Coroutine coroutine;
        

        public AbsorbInk inkStainPrefab;
    
    
        [SerializeField] private float hitMarkerFadeTime;
        [SerializeField] private float timeForEnemyToRespawn;

        private Coroutine bodyHit;
        private Coroutine headHit;
    
        // Start is called before the first frame update
        public override void Awake()
        {
            base.Awake();
            // bodyHitMarker.alpha = 0;
            // headHitMarker.alpha = 0;
            Ex.DefaultMask = LayerMask.GetMask("Default");

            // var temp = FindObjectsOfType<GameObject>();
            // foreach (var oui in temp)
            // {
            //     if (oui.activeInHierarchy)
            //     {
            //         if (oui.TryGetComponent(out NavMeshSurface blabla))
            //         {
            //             Debug.Log("Found Navmesh at " + oui, oui);
            //         }
            //
            //     }
            // }

        }

        void Start()
        {
            UpdateHealPackUI();
            gameOver.DOFade(0f, 2f);
        }
        
        public void UpdateHealPackUI()
        {
            healPackText.text = "x " + PlayerController.instance.currentHealPackAmount;
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
                bodyHit = StartCoroutine(FadeHitMark(false));
            }
        }

        public void UsingTK()
        {
            //crosshairTK.alpha = PlayerController.instance.tkManager.controlledProp ? 1 : 0;
        }

        public Coroutine killMarker;
        public void OnKillEnemy()
        {
            if (killMarker != null)StopCoroutine(killMarker);
            killMarker = StartCoroutine(OnKillHitMark());
        }

        public float UpdatePlayerStamina(float current, float max, float increment)
        {
            current += increment;
        
            if (current < 0) current = 0;
            if (current >= max)
            {
                current = max;
            }
            else
            {
                segments[0].color = Color.black;
            }
            
            baseInkBar.fillAmount = current / max;
            return current;
        }

        private float timer;
        // Update is called once per frame
        private IEnumerator FadeHitMark(bool headshot)
        {
            timer = Time.time + hitMarkerFadeTime;
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

        private float timer2;
        private IEnumerator OnKillHitMark()
        {
            
            timer2 = Time.time + hitMarkerFadeTime;
            crosshairBase.color = Color.red;
            while (Time.time < timer2)
            {
                 crosshairBase.color = Color.Lerp(Color.white,Color.red, (timer2 - Time.time)/hitMarkerFadeTime);
                yield return null;
            }
            
            crosshairBase.color = Color.white;
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
        
            try
            {
                enemy.transform.position = enemy.respawnPoint.position;
            }
            catch (Exception e)
            {
                Debug.LogError("T'as oublié de mettre un respawn point en réference");
       
            }
        }
        
        public void PlayerDeath()
        {
            Debug.Log(Time.timeScale);
            PlayerController.instance.isControled = true;
            PlayerController.instance.enabled = false;
            gameOver.DOFade(1f, 1f);
            StartCoroutine(GameOverCoroutine());
        }

        private IEnumerator GameOverCoroutine()
        {
            yield return new WaitForSeconds(1.5f);
            Reload();
        }

        public void Reload()
        {
            PlayerController.instance.isControled = false;
            PlayerPrefs.SetInt("isReloadingSave", 1);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void VFX_EnemyHitMethod(Vector3 position)
        {
            VFX_EnemyHit[0].transform.position = position;
            VFX_EnemyHit[0].transform.LookAt(PlayerController.instance.transform.position);
            VFX_EnemyHit[0].Play();
        }

        public void HideUI()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(Fade(true, 1f, playerUI));
        }

        public void ShowUI()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(Fade(false, 0.2f, playerUI));
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

            target.gameObject.SetActive(target.alpha > 0.9f);
        }
    }
}

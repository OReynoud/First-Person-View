using System;
using System.Collections;
using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mechanics
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private CanvasGroup playerUI;
        [SerializeField] private CanvasGroup bodyHitMarker;
        [SerializeField] private CanvasGroup headHitMarker;
        [SerializeField] private Image crosshairBase;
        [SerializeField] public TextMeshProUGUI interactText;
        [SerializeField] public CanvasGroup gameOver;
        [SerializeField] public GameObject inkStainDecal;
        [SerializeField] public ParticleSystem[] VFX_EnemyHit;
        
        [SerializeField] private ParticleSystem VFX_HeadShot;
        private Coroutine coroutine;
        

        public AbsorbInk inkStainPrefab;
    
    
        [SerializeField] private float hitMarkerFadeTime;
        [SerializeField] private float timeForEnemyToRespawn;

        private Coroutine bodyHit;
        private Coroutine headHit;

        public bool canStartEndingCinematic;
        public bool ending;

        [SerializeField] private AudioMixerSnapshot cutMusic;
        [SerializeField] private AudioSource heartBeat;
        [SerializeField] private RectTransform credits;
        [SerializeField] private CanvasGroup endingBlackScreen;
    
        // Start is called before the first frame update
        public override void Awake()
        {
            base.Awake();
            bodyHitMarker.alpha = 0;
            headHitMarker.alpha = 0;
            Ex.DefaultMask = LayerMask.GetMask("Default");



        }

        void Start()
        {
            gameOver.DOFade(0f, 2f);
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
        }

        public Coroutine killMarker;
        public void OnKillEnemy()
        {
            if (killMarker != null)StopCoroutine(killMarker);
            killMarker = StartCoroutine(OnKillHitMark());
        }

        private float percent;
        public float UpdatePlayerStamina(float current, float max, float increment)
        {
            current += increment;
        
            if (current < 0) current = 0;
            if (!ending)
            {
                if (current >= max)
                {
                    current = max;
                }
            }
            else
            {
                if (current >= inkOverFlowLimit)
                {
                    current = inkOverFlowLimit;
                }
            }

            PlayerController.instance.tkManager.leftHandModule.materials[1].SetFloat(
                "_InkLevel",
                Mathf.Lerp(TelekinesisModule.zeroInkFill, TelekinesisModule.fullInkFill, current / max));
            percent = Mathf.Round((current / max) * 100);
            PlayerController.instance.tkManager.moduleText.text = percent.ToString();
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
            if (credits.gameObject.activeInHierarchy) return;
            
            PlayerController.instance.isControled = true;
            PlayerController.instance.enabled = false;
            gameOver.DOFade(1f, 1f);

            if (canStartEndingCinematic)
            {
                StartCoroutine(EndingCredits());
            }
            else
            {
                StartCoroutine(GameOverCoroutine());
            }
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

        public void VFX_HeadshotMethod(Vector3 position)
        {
            VFX_HeadShot.transform.position = position;
            VFX_HeadShot.transform.LookAt(PlayerController.instance.transform.position);
            VFX_HeadShot.Play();
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



        
        //Outro Variables
        [Foldout("Ending Cinematic")] [SerializeField]
        private float timeToDealDamage;
        [Foldout("Ending Cinematic")] [SerializeField]
        private float timeTickDecrease;
        
        [Foldout("Ending Cinematic")] [SerializeField]
        private float damageAmount;
        [Foldout("Ending Cinematic")] [SerializeField]
        private float absorbSpeed = 25;
        [Foldout("Ending Cinematic")] [SerializeField]
        private float inkOverFlowLimit;
        
        
        //Start the Outro
        public IEnumerator StartEndingCinematic()
        {
            PlayerController.instance.ImmobilizePlayer();
            ending = true;
            StartCoroutine(PlayerController.instance.ResetDamageShader(false));

            var currentRot = PlayerController.instance.playerCam.rotation;
            PlayerController.instance.playerCam.LookAt(PlayerController.instance.tkManager.controlledProp.transform.position + Vector3.up * 50f);
            var targetRot = PlayerController.instance.playerCam.rotation;
            PlayerController.instance.playerCam.rotation = currentRot;

            var t = 0f;

            while (t <= 3f)
            {
                t += Time.deltaTime;
                PlayerController.instance.playerCam.rotation = Quaternion.Lerp(currentRot, targetRot, t / 3f);
                yield return null;
            }
            PlayerController.instance.rotationX = 0;
            PlayerController.instance.tkManager.inkAbsorbSpeed = absorbSpeed;

            cutMusic.TransitionTo(8f);
            
            StartCoroutine(TakeDamageOverTime());
            
            
            yield return null;
        }

        IEnumerator TakeDamageOverTime()
        {
            yield return new WaitForSeconds(timeToDealDamage);
            timeToDealDamage -= timeTickDecrease;
            gameOver.alpha = 1 - (PlayerController.instance.currentHealth / PlayerController.instance.maxHealth);
            PlayerController.instance.TakeDamage(damageAmount);
            StartCoroutine(TakeDamageOverTime());
        }

        IEnumerator EndingCredits()
        {
            credits.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(1f);
            StartCoroutine(StopHeartBeat());
            
            yield return new WaitForSeconds(3f);

            StartCoroutine(BlackScreen());
            StartCoroutine(LoadMainMenu());
            StartCoroutine(SoundOutro());

            var t = 0f;
            Vector2 basePos = credits.anchoredPosition;

            while (t < 70f)
            {
                t += Time.unscaledDeltaTime;

                credits.anchoredPosition = Vector2.Lerp(basePos, new Vector2(basePos.x, 0), t / 70f);
                
                yield return null;
            }
        }

        IEnumerator StopHeartBeat()
        {
            Debug.Log("oui");
            
            var t = 0f;
            var vol = heartBeat.volume;
            
            while (t < 4f)
            {
                t += Time.unscaledDeltaTime;

                heartBeat.volume = Mathf.Lerp(vol, 0f, t / 4f);
                
                yield return null;
            }
        }

        IEnumerator BlackScreen()
        {
            yield return new WaitForSeconds(70);
            endingBlackScreen.DOFade(1f, 3f);
        }

        IEnumerator LoadMainMenu()
        {
            yield return new WaitForSeconds(80);
            SceneManager.LoadScene("MainMenu");
        }

        IEnumerator SoundOutro()
        {
            yield return new WaitForSeconds(0.5f);

            AudioManager.instance.PlayUISound(5, 3, 0f);
        }
    }
}

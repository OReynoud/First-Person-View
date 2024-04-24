using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Mechanics
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private CanvasGroup bodyHitMarker;
        [SerializeField] private CanvasGroup headHitMarker;
        [SerializeField] private CanvasGroup crosshairTK;
        [SerializeField] private Image crosshairBase;
        [SerializeField] private Image baseInkBar;
        [SerializeField] private Image surplusInkBar;
        [SerializeField] private Image[] segments;
        [SerializeField] private TextMeshProUGUI healPackText;
        [SerializeField] public TextMeshProUGUI interactText;
        [SerializeField] public GameObject gameOver;
        

        public AbsorbInk inkStainPrefab;
    
    
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
        }

        public void Update()
        {
            UsingTK();
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
                headHit = StartCoroutine(FadeHitMark(false));
            }
        }

        public void UsingTK()
        {
            crosshairTK.alpha = PlayerController.instance.controlledProp ? 1 : 0;
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
            if (current > max * 2)
            {
                current = max * 2;
                segments[0].color = Color.yellow;
            }
            else
            {
                
                segments[0].color = Color.black;
            }
            
            baseInkBar.fillAmount = current / max;
            surplusInkBar.fillAmount = (current - max) / max;
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
            PlayerController.instance.enabled = false;
            Debug.Log("Je suis mort");
            gameOver.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        public void Reload()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}

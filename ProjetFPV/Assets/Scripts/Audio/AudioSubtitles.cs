using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioSubtitles : MonoBehaviour
{
    private bool hasToCount;
    [HideInInspector] public bool hasToDisplay; // Call this from Audio is sound is too low, call it again when it's high enough

    [SerializeField] private TextMeshProUGUI subtitlesUI;
    [SerializeField] private float subtitlesMaxDistance;
    
    private float timer;
    private int currentIndex;
    private int currentList;
    [SerializeField] private List<SubtitlesList> subtitles;

    private Transform player;
    private bool temp;
    private bool mainMenu;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            mainMenu = true;
            return;
        }
        
        player = PlayerController.instance.transform;
    }
    
    void Update()
    {
        if (!hasToCount) return;
        
        timer += Time.unscaledDeltaTime;

        if (currentIndex >= subtitles[currentList].subtitlesList.Count)
        {
            hasToCount = false;
            
            if (hasToDisplay)
            {
                HideSubtitles(false);
            }
        }
        
        else if (timer >= subtitles[currentList].subtitlesList[currentIndex].timeCode)
        {
            DisplaySubtitles(subtitles[currentList].subtitlesList[currentIndex].sub);
            
            currentIndex++;
        }

        if (mainMenu) return;
        
        temp = hasToDisplay;
        hasToDisplay = Vector3.Distance(transform.position, player.position) <= subtitlesMaxDistance;
        if (temp != hasToDisplay && temp)
        {
            HideSubtitles(true);
        }
    }

    void DisplaySubtitles(string sub)
    {
        if (!hasToDisplay) return;
        subtitlesUI.text = sub;
    }

    void HideSubtitles(bool continueToCount)
    {
        hasToCount = continueToCount;
        
        subtitlesUI.text = "";
    }

    // Call this from Audio Script (if multiple audio, specify index
    public void StartTimer(int list)
    {
        hasToCount = true;

        if (!mainMenu)
        {
            if (Vector3.Distance(transform.position, player.position) <= subtitlesMaxDistance)
            {
                hasToDisplay = true;
            }
        }
        else
        {
            hasToDisplay = true;
        }
        
        currentList = list;
        currentIndex = 0;
        timer = 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, subtitlesMaxDistance);
    }

    [Serializable]
    class SubtitlesList
    {
        public string name;
        public List<Subtitles> subtitlesList;
    }

    [Serializable]
    class Subtitles
    {
        public float timeCode;
        [TextArea]
        public string sub;
    }
}

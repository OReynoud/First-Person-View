using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioSubtitles : MonoBehaviour
{
    private bool hasToCount;

    [SerializeField] private TextMeshProUGUI subtitlesUI;
    [SerializeField] private float subtitlesMaxDistance;
    
    private float timer;
    private int currentIndex;
    private int currentList;
    [SerializeField] private List<SubtitlesList> subtitles;

    private Transform player;
    private bool mainMenu;

    private string subToDisplay;

    private bool tooFar;
    
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
        if (mainMenu || Vector3.Distance(transform.position, player.position) <= subtitlesMaxDistance)
        {
            tooFar = false;
            DisplaySubtitles(subToDisplay);
        }
        else if (!tooFar)
        {
            DisplaySubtitles("");

            tooFar = true;
        }
        
        if (!hasToCount) return;
        
        timer += Time.deltaTime;

        if (currentIndex >= subtitles[currentList].subtitlesList.Count)
        {
            DisplayEnd();
            return;
        }
        
        if (timer >= subtitles[currentList].subtitlesList[currentIndex].timeCode)
        {
            subToDisplay = subtitles[currentList].subtitlesList[currentIndex].sub;
            
            
            currentIndex++;
        }
    }

    void DisplaySubtitles(string sub)
    {
        subtitlesUI.text = sub;
    }

    void DisplayEnd()
    {
        Debug.LogFormat("From {0}, DisplayEnd", this);
        subToDisplay = "";
        hasToCount = false;
    }

    // Call this from Audio Script (if multiple audio, specify index
    public void StartTimer(int list)
    {
        Debug.LogFormat("From {0}, StartTimer", this);
        
        hasToCount = true;
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

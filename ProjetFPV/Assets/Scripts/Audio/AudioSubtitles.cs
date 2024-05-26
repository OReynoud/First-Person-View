using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AudioSubtitles : MonoBehaviour
{
    private bool hasToCount;
    [HideInInspector] public bool hasToDisplay; // Call this from Audio is sound is too low, call it again when it's high enough

    [SerializeField] private TextMeshProUGUI subtitlesUI;
    
    public float timer;
    private int currentIndex;
    private int currentList;
    [SerializeField] private List<SubtitlesList> subtitles;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            hasToCount = true;
            hasToDisplay = true;
        }
        
        if (!hasToCount) return;
        
        subtitlesUI.gameObject.SetActive(hasToDisplay);
        
        timer += Time.deltaTime;

        if (currentIndex >= subtitles[currentList].subtitlesList.Count)
        {
            HideSubtitles();
            return;
        }
        
        if (timer >= subtitles[currentList].subtitlesList[currentIndex].timeCode)
        {
            DisplaySubtitles(subtitles[currentList].subtitlesList[currentIndex].sub);
            
            currentIndex++;
        }
    }

    void DisplaySubtitles(string sub)
    {
        subtitlesUI.text = sub;
    }

    void HideSubtitles()
    {
        hasToCount = false;
        subtitlesUI.text = "";
    }

    // Call this from Audio Script (if multiple audio, specify index
    public void StartTimer(int list)
    {
        hasToCount = true;
        hasToDisplay = true;
        
        currentList = list;
        currentIndex = 0;
        timer = 0;
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

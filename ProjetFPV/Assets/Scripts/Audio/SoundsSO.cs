using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundSO", menuName = "ScriptableObjects/NewSoundSO" + "", order = 1)]
public class SoundsSO : ScriptableObject
{
    public List<Sound> sounds;
}

[Serializable]
public class Sound
{
    public string name;
    public AudioClip sound;
    [Range(0f, 1f)] public float volume;
}
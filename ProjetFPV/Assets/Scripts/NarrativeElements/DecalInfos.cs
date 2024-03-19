using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalInfos : MonoBehaviour
{
    [SerializeField] private string description;

    public string ReturnDescription()
    {
        return description;
    }
}

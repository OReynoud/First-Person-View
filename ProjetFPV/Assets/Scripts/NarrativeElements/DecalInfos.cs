using System;
using UnityEngine;

public class DecalInfos : MonoBehaviour
{
    [Tooltip("Description à afficher dans la UI")]
    [SerializeField] private string description;
   
    [Tooltip("Distance à laquelle le texte est lisible (gizmo jaune)")]
    [SerializeField] private float maxDistance;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, maxDistance);
    }

    public void ReturnDescription(out float distance, out string desc)
    {
        distance = maxDistance;
        desc = description;
    }
}

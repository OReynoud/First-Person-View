using System;
using TMPro;
using UnityEngine;

public class DecalTranslation : MonoBehaviour
{
    private RaycastHit hit;
    [SerializeField] private float translationDistance;

    [SerializeField] private TextMeshProUGUI descriptionText;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * translationDistance);
    }

    void Update()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit, translationDistance))
        {
            if (hit.transform.gameObject.CompareTag("Decal"))
            {
                descriptionText.text = hit.transform.gameObject.GetComponent<DecalInfos>().ReturnDescription();
            }
            else
            {
                descriptionText.text = "";
            }
        }
    }
}

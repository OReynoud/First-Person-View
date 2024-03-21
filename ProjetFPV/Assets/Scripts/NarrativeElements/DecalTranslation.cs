using System;
using TMPro;
using UnityEngine;

public class DecalTranslation : MonoBehaviour
{
    private RaycastHit hit;
    private bool hasAText;
    
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
                var target = hit.transform.gameObject.GetComponent<DecalInfos>();
                float dist = 0;
                string desc = "";

                target.ReturnDescription(out dist, out desc);

                if (Vector3.Distance(transform.position, hit.transform.position) <= dist)
                {
                    descriptionText.text = desc;
                    hasAText = true;
                }
                else
                {
                    descriptionText.text = "";
                    hasAText = false;
                }
            }
            else if (hasAText)
            {
                descriptionText.text = "";
                hasAText = false;
            }
        }
    }
}

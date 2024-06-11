using System.Collections.Generic;
using UnityEngine;

public class DisableGameObjects : MonoBehaviour
{
    public List<GameObject> gameObjectsToDisable;
    public GameObject twinSafetyTrigger;

    void Start()
    {
        if (twinSafetyTrigger == null) return;

        var comp = twinSafetyTrigger.AddComponent<DisableGameObjects>();
        comp.gameObjectsToDisable = gameObjectsToDisable;
        comp.twinSafetyTrigger = gameObject;
        twinSafetyTrigger.SetActive(false);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (var obj in gameObjectsToDisable)
        {
            obj.SetActive(!obj.activeInHierarchy);
        }

        twinSafetyTrigger.SetActive(true);
        gameObject.SetActive(false);
    }
}

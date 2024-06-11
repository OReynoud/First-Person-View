using System.Collections;
using UnityEngine;

public class TutorialStepTrigger : MonoBehaviour
{
    [Tooltip("Si coché, cache le tutoriel précédent. Sinon, affiche le tutoriel")] [SerializeField] private bool hideTutorial;
    [SerializeField] private string tutorialText;
    [SerializeField] private float timeBeforeDisplay;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (hideTutorial)
        {
            Tutorial.instance.HideTutorial();
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(WaitBeforeDisplay());
        }
    }

    private IEnumerator WaitBeforeDisplay()
    {
        GetComponent<BoxCollider>().enabled = false;
        
        yield return new WaitForSeconds(timeBeforeDisplay);
        
        Tutorial.instance.DisplayTutorial(tutorialText);

        Destroy(gameObject);
    }
}

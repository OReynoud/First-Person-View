using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Mechanics
{
    public class ClimbWall : MonoBehaviour, ICanInteract
    {
        public BoxCollider col;
        public float timeToClimb;

        float TopOfCollider()
        {
            return col.bounds.center.y + col.bounds.extents.y;
        }
        private void Start()
        {
            player = PlayerController.instance;
        }

        private PlayerController player;
        public float amplitude = 0.02f;
        public float frequency = 0.05f;

        public void Interact(Vector3 dir)
        {
            
            player.ImmobilizePlayer();
            
            float angle = new Vector2(dir.z, dir.x).GetAngleFromVector();
            Debug.Log(angle);
            var rotationValue = new Vector3(0, angle, 0);
            player.playerCam.DORotate(rotationValue, 0.2f);
            player.transform.DORotate(rotationValue, 0.2f).OnComplete(() =>
            {
                StartCoroutine(Climb());
            });
        }

        IEnumerator Climb()
        {
            //var oui = player.transform.rotate;
            float t = 0;
            var startHeight = player.transform.position.y; 
            var endHeight = TopOfCollider();
            while (t < timeToClimb)
            {
                t += Time.deltaTime;
                Vector3 pos = player.transform.position;
                pos.x += Mathf.Cos(t * frequency) * amplitude;
                pos.y = Mathf.Lerp(startHeight, endHeight, t / timeToClimb);
                player.transform.position = pos;
                yield return null;
            }

            var posToJump = col.ClosestPoint(player.transform.position) + player.transform.forward;
            player.transform.DOJump(posToJump, 1, 1, 0.5f).OnComplete(
                () =>
                {
                    player.ImmobilizePlayer();
                });

        }
    }
}

using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Mechanics
{
    public class ClimbWall : MonoBehaviour, ICanInteract
    {
        public BoxCollider col;
        public float timeToClimb = 2;

        float TopOfCollider()
        {
            return col.bounds.center.y + col.bounds.extents.y;
        }

        private void Start()
        {
            player = PlayerController.instance;
            col = GetComponent<BoxCollider>();
        }

        private PlayerController player;
        public float amplitude = 0.01f;
        public float frequency = 7f;

        public void Interact(Vector3 dir)
        {
            if (player.transform.position.y > transform.position.y +0.5f)return;
            player.ImmobilizePlayer();

            //SON
            AudioManager.instance.PlaySound(3, 17, player.gameObject, 0.1f, false);
            
            float angle = new Vector2(dir.z, dir.x).GetAngleFromVector();
            var rotationValue = new Vector3(0, angle, 0);
            Vector2 wallPoint = new Vector2(
                col.ClosestPoint(player.transform.position).x,
                col.ClosestPoint(player.transform.position).z);
            
            Vector2 playerPoint = new Vector2(
                player.transform.position.x, 
                player.transform.position.z);
            
            float dist = Vector2.Distance(wallPoint, playerPoint);
            player.transform.DOMove(
                Vector3.Lerp(
                new Vector3(playerPoint.x, player.transform.position.y,playerPoint.y),
                new Vector3(wallPoint.x,player.transform.position.y,wallPoint.y),
                (dist-1f) / dist),
                0.2f);
            player.rotationX = rotationValue.x;
            player.playerCam.DORotate(rotationValue, 0.2f);
            player.transform.DORotate(rotationValue, 0.2f).OnComplete(() =>
            {
                StartCoroutine(Climb());
                // player.transform.rotation = Quaternion.Euler(rotationValue);
                //
                // player.playerCam.rotation = Quaternion.Euler(rotationValue);
            });
        }

        public void ShowContext()
        {
            if (player.transform.position.y > transform.position.y + 0.5f)
            {
                GameManager.instance.interactText.enabled = false;
                return;
            }
            
            GameManager.instance.interactText.text = "[E] Climb";
        }

        IEnumerator Climb()
        {
            float t = 0;
            var startHeight = player.transform.position.y;
            var endHeight = TopOfCollider();
            while (t < timeToClimb)
            {
                t += Time.deltaTime;
                Vector3 pos = player.transform.position;
                pos.x += Mathf.Cos(t * frequency) * amplitude * player.transform.right.x;
                pos.z += Mathf.Cos(t * frequency) * amplitude * player.transform.right.z;
                pos.y = Mathf.Lerp(startHeight, endHeight, t / timeToClimb);
                player.transform.position = pos;
                yield return null;
            }

            var posToJump = col.ClosestPoint(player.transform.position) + player.transform.forward +
                            Vector3.up * (player.standingCollider.height * 0.5f);
            player.transform.DOJump(posToJump, 1, 1, 0.5f).OnComplete(
                () =>
                {
                    player.ImmobilizePlayer(); 
                    
                });
        }
    }
}
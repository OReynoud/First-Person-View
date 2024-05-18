using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Mechanics;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnstableObject : ControllableProp
{
    public float timeBasculement = 2;

    private bool toppled;
    private float timer;
    public int trajectoryPredictPoints;
    public Transform trajectoryDir;

    public float groundLevel = 0;
    [Range(0,5f)]public float playerPush = 0.1f;
    
    [Range(0, 360)] public float throwDir;
    public float throwVelocity;

    public bool testOnStart;
    
    // Start is called before the first frame update
    
    //
    private const float g = 9.81f;

    private float distanceToGround;

    
    // FOR Y VELOCITY, TO DETERMINE HOW LONG IT WILL TAKE FOR THE OBJECT TO HIT THE GROUND
    // v^2 = u^2 + 2as
    // v = u + at
    // at = v - u
    // t = (v - u) / a
    
    //WHERE v = speedBasculement  AND s IS THE X COORDINATE
    
    private float totalTime;
    private float v;
    float localTime;
    float yPos; 
    float xPos;
    Vector3 aimedPos;

    private void OnDrawGizmosSelected()
    { 
        if(EditorApplication.isPlaying ) return;
        Handles.color = Color.magenta;
        Handles.ArrowHandleCap(
            0,
            transform.position,
            Quaternion.AngleAxis(throwDir,Vector3.up),
            3,
            EventType.Repaint);
        distanceToGround = transform.position.y - groundLevel;

        trajectoryDir.rotation = Quaternion.AngleAxis(throwDir, Vector3.up);
        v = Mathf.Sqrt(2 * g * distanceToGround);
        totalTime = v / g;
        Gizmos.color = Color.blue;
        for (int i = 0; i < trajectoryPredictPoints; i++)
        {
            localTime = totalTime * i / trajectoryPredictPoints;
            yPos = (g * localTime * localTime) * 0.5f;
            xPos = throwVelocity * localTime;
            aimedPos = transform.position + trajectoryDir.forward * xPos;
            aimedPos = new Vector3(aimedPos.x,
                aimedPos.y - yPos, 
                aimedPos.z);
            Gizmos.DrawSphere(aimedPos, 0.1f);
        }

    }

    private async void Start()
    {
        await Task.Delay(100);
        if (testOnStart)
        {
            Topple();
        }
    }


    private bool hitPlayer;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            var enemy = other.collider.GetComponentInParent<Enemy>();
            enemy.ApplyStun();
        }
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StartCoroutine(DelayedDestroy());
        }
    }

    private void OnCollisionStay(Collision other)
    {
        Debug.Log("Poussez");
        if (other.gameObject.TryGetComponent(out PlayerController player))
        {
            var dir = player.transform.position - transform.position;
            dir = new Vector3(dir.x, 0, dir.z);
            //player.transform.position += dir.normalized * playerPush;
            player.rb.AddForce(dir.normalized * playerPush, ForceMode.Impulse);
            hitPlayer = true;
            player.canMove = false;
        }
    }

    private void OnCollisionExit(Collision other)
    {    
        if (other.gameObject.TryGetComponent(out PlayerController player))
        {
            var dir = player.transform.position - transform.position;
            dir = new Vector3(dir.x, 0, dir.z);
            player.rb.AddForce(dir.normalized * playerPush, ForceMode.VelocityChange);
            hitPlayer = false;
            player.canMove = true;
        }
    }

    public override void ApplyTelekinesis()
    {
        isGrabbed = !isGrabbed;

        if (isGrabbed)
        {
            StartShakeAnim();
        }
        else
        {
            shake.Kill();
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (toppled)
        {
            canBeGrabbed = false;
            return;
        }
        
        if (isGrabbed)
        {
            timer += Time.deltaTime;
            if (timer >= timeBasculement)
            {
                toppled = true;
                Topple();
            }
        }
        else
        {
            shake.Kill();
            timer = 0;
        }
    }

    public Tween shake;
    void StartShakeAnim()
    {
        shake = transform.DOShakePosition(0.1f, 0.2f, 100).OnComplete(StartShakeAnim);
    }

    void Topple()
    {
        PlayerController.instance.ReleaseProp(new InputAction.CallbackContext());
        body.AddForce(trajectoryDir.forward * throwVelocity,ForceMode.VelocityChange);
        canBeGrabbed = false;
    }

    private IEnumerator DelayedDestroy()
    {
        body.mass = 100;
        while (body.velocity.magnitude > 0.001f || hitPlayer)
        {
            body.AddForce(Vector3.down,ForceMode.VelocityChange);
            yield return new WaitForSeconds(0.1f);
        }
        Destroy(this);
    }

    private void OnDestroy()
    {
        body.isKinematic = true;
        PlayerController.instance.canMove = true;
    }
}

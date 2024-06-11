using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimHandsController : MonoBehaviour
{
    // Grab, Release
    [SerializeField] private Animation leftHand;
    
    [SerializeField] private Animation rightHand;


    public bool holding;
    public bool reloading;
    public bool walking;
    public bool shooting;
    public bool pickUp;
    private float walkAnimDuration;
    private float walkAnimClock;
    
    // Start is called before the first frame update
    void Start()
    {
        
        walkAnimDuration = leftHand.GetClip("A_WalkLeft").length;
    }

    // Update is called once per frame
    void Update()
    {
        if (reloading) RightHand_ReloadLoop();

        if (walking)
        {
            walkAnimClock += Time.deltaTime;
            if (walkAnimClock > walkAnimDuration)
            {
                walkAnimClock = 0;
            }
        }
        else
        {
            if (walkAnimClock > 0)
            {
                walkAnimClock -= Time.deltaTime;
            }
        }

        if (!holding)
        {
            if (walking)
            {
                leftHand.Stop();
                leftHand.GetClip("A_WalkLeft").SampleAnimation(leftHand.gameObject,walkAnimClock);
            }
            else if (!leftHand.isPlaying)
            {
                
                leftHand.Play("A_IdleLeftNew");
            }
        }
        if (!reloading && !shooting && !pickUp)
        {
            if (walking)
            {
                rightHand.Stop();
                rightHand.GetClip("A_WalkRightBis").SampleAnimation(rightHand.gameObject,walkAnimClock);
            }
            else if (!rightHand.isPlaying)
            {
                rightHand.Play("A_IdleRightNew");
            }
        }
    }


    public void CheckWalk()
    {
        // if (walking != walkChecker)
        // {
        //     if (walking)
        //     {
        //         StartWalk();
        //     }
        //     else
        //     {
        //         StopWalk();
        //     }
        // }
        //
        // walkChecker = walking;
    }
 

    public void LeftHand_Grab()
    {
        holding = true;
        leftHand.Stop();
        leftHand.Play("A_GrabLeft");
        LeftHand_Hold();

    }
    public void LeftHand_Reset()
    {
        leftHand.Play("A_ResetNew");
        if (holdRoutine is not null)
        {
            StopCoroutine(holdRoutine);
        }
        holdRoutine = StartCoroutine(LeftHand_StopHold(leftHand.GetClip("A_ResetNew").length));
    }
    public void LeftHand_Release()
    {
        leftHand.Play("A_ReleaseNew");
        
        if (holdRoutine is not null)
        {
            StopCoroutine(holdRoutine);
        }
        holdRoutine = StartCoroutine(LeftHand_StopHold(leftHand.GetClip("A_ReleaseNew").length));
    }

    private Tween shake;
    public void LeftHand_Hold()
    {
        if (leftHand.isPlaying)
        {
            shake = leftHand.transform.DOShakeScale(0.05f,0,0).OnComplete(() =>
            {
                if (holding)
                {
                    LeftHand_Hold();
                }
            });
            return;
        }
        shake = leftHand.transform.DOShakePosition(0.1f, 0.05f, 50).OnComplete(() =>
        {
            if (holding)
            {
                LeftHand_Hold();
            }
        });
    }

    private Coroutine holdRoutine;
     IEnumerator LeftHand_StopHold(float duration)
    {
        shake.Kill(true);
        yield return new WaitForSeconds(duration);
        holding = false;
    }

    public void RightHand_ReloadStart()
    {
        rightHand.Play("A_ReloadStartNew");
        reloading = true;
    }
    
    public void RightHand_ReloadLoop()
    {
        if (rightHand.isPlaying) return;
        
        rightHand.Play("A_ReloadLoopNew");
    }
    
    public void RightHand_ReloadEnd()
    {
        rightHand.Stop();
        rightHand.Play("A_ReloadEndNew");
        
        if (reloadRoutine is not null)
        {
            StopCoroutine(reloadRoutine);
        }

        reloadRoutine = StartCoroutine(HandleReload());
    }

    private Coroutine reloadRoutine;

    public IEnumerator HandleReload()
    {
        yield return new WaitForSeconds(rightHand.GetClip("A_ReloadEndNew").length);
        reloading = false;
    }
    public void RightHand_Shoot()
    {
        rightHand.Stop();
        rightHand.Play("A_ShootNew");
        if (shootCoroutine is not null)
        {
            StopCoroutine(shootCoroutine);
        }

        shootCoroutine = StartCoroutine(HandleShooting());
    }


    private Coroutine shootCoroutine;
    IEnumerator HandleShooting()
    {
        shooting = true;
        yield return new WaitForSeconds(rightHand.GetClip("A_ShootNew").length);
        shooting = false;
    }
    public void RightHand_PickUp()
    {
        rightHand.Stop();
        rightHand.Play("A_GrabNew");
        if (pickUpCoroutine is not null)
        {
            StopCoroutine(pickUpCoroutine);
        }

        pickUpCoroutine = StartCoroutine(HandlePickUp());
    }

    private Coroutine pickUpCoroutine;
    IEnumerator HandlePickUp()
    {
        pickUp = true;
        yield return new WaitForSeconds(rightHand.GetClip("A_GrabNew").length);
        pickUp = false;
    }
    public async void ChainShootToReload()
    {
        while (rightHand.isPlaying)
        {
            await Task.Delay(10);
            if (!Application.isPlaying)return;
        }

        if (!Application.isPlaying)return;
        PlayerController.instance.RequestReload(new InputAction.CallbackContext());
    }
}

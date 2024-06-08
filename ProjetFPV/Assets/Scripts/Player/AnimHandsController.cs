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
    
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (reloading) RightHand_ReloadLoop();
        if (!leftHand.isPlaying && !holding)
        {
            
            leftHand.Play("A_IdleLeftNew");
        }
        if (!rightHand.isPlaying && !reloading)
        {
            
            rightHand.Play("A_IdleRightNew");
        }
    }

    public void LeftHand_Grab()
    {
        leftHand.Play("A_GrabNew");
        holding = true;
        LeftHand_Hold();

    }
    public void LeftHand_Reset()
    {
        leftHand.Play("A_ResetNew");
        LeftHand_StopHold();
    }
    public void LeftHand_Release()
    {
        leftHand.Play("A_ReleaseNew");
        LeftHand_StopHold();
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
    
    public void LeftHand_StopHold()
    {
        holding = false;
        shake.Kill(true);
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
        reloading = false;
        rightHand.Stop();
        rightHand.Play("A_ReloadEndNew");
    }
    
    public void RightHand_Shoot()
    {
        rightHand.Stop();
        rightHand.Play("A_ShootNew");
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

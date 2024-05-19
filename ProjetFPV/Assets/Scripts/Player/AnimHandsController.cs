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
    
    
    
    //LEFT HAND ANIMS
    //GRAB = 0.20 - 1.18
    //RELEASE = 2.03 - 2.27
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (reloading) RightHand_ReloadLoop();
        
    }

    public void LeftHand_Grab()
    {
        leftHand.Play("A_Grab");
        holding = true;
        LeftHand_Hold();

    }
    public void LeftHand_Release()
    {
        leftHand.Play("A_ReleaseBis");
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
        rightHand.Play("A_ReloadStart");
        reloading = true;
    }
    
    public void RightHand_ReloadLoop()
    {
        if (rightHand.isPlaying) return;
        
        rightHand.Play("A_ReloadLoop");
    }
    
    public void RightHand_ReloadEnd()
    {
        reloading = false;
        rightHand.Stop();
        rightHand.Play("A_ReloadEnd");
    }
    
    public void RightHand_Shoot()
    {
        rightHand.Stop();
        rightHand.Play("A_Shoot");
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchState : BaseState
{
    public float pSpeed = 2f;
    
    public override void enterState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerConfig pPlayerConfig, PlayerInfo pInfo)
    {
        pAnimator.SetBool("IsCrouching", true);
        pInfo.SetSpeed(pSpeed);
        Debug.Log("In the courch state");
    }

    public override void exitState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        Debug.Log("Exiting the crouch state");
        pInfo.ResetSpeed();
        pAnimator.SetBool("IsCrouching", false);
    }

    public override void updatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pInfo)
    {
        
    }

    public override void updateState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        pAnimator.SetFloat("CrouchInput", pInfo.GetMoveInput().x);
    }
}

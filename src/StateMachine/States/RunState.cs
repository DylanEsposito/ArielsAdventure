using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunState : BaseState
{
    private float moveSpeed = 5f;
    public override void enterState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerConfig pPlayerConfig, PlayerInfo pInfo)
    {
        //Debug.Log("Is running");
        pInfo.ResetSpeed();
        moveSpeed = pInfo.GetMoveSpeed();
        pAnimator.SetBool("IsRunning", true);
    }

    public override void updateState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        Run(pRigidbody, pAnimator, pInfo);
        //ManageAnimation(pStateManager);
    }

    public override void exitState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        pAnimator.SetBool("IsRunning", false);
    }

    public override void updatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pInfo)
    {

    }

    void Run(Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        Vector2 playerVelocity;
        Vector2 normalizedVector = pInfo.GetMoveInput().normalized;
        //Old model where player velocity was set every frame
        //Debug.Log("Move speed is " + moveSpeed);
        playerVelocity = new Vector2(normalizedVector.x * moveSpeed, pRigidbody.velocity.y);
        pRigidbody.velocity = playerVelocity;

        //Now set animator dependent on if we're actually moving
        if (pRigidbody.velocity.x > 0.15f || pRigidbody.velocity.x < -0.15f)
        {
            if (pInfo.IsOnGround())
            {
                pAnimator.SetBool("IsRunning", true);
            }
            else
            {
                pAnimator.SetBool("IsRunning", false);
            }
        }
        else
        {
            pAnimator.SetBool("IsRunning", false);
        }
    }

    public override PlayerState GetStateType()
    {
        return PlayerState.Running;
    }
}

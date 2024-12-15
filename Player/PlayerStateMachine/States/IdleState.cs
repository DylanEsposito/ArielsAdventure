using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : BaseState
{
    //Should probably get rid of this,not necessary

    public override void enterState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerConfig pPlayerConfig, PlayerInfo pInfo)
    {
        Debug.Log("Entering idle state");
        pAnimator.SetBool("IsJumping", false);
        pAnimator.SetBool("IsRunning", false);
        pRigidbody.velocity = new Vector2(0f, pRigidbody.velocity.y);
    }

    public override void updateState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        //Debug.Log("IdleState::updateState In the idle state");
        //pAnimator.SetBool("IsRunning", false);
        pRigidbody.velocity = new Vector2(0f, pRigidbody.velocity.y);
    }

    public override void exitState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        Debug.Log("Exiting idle state");
    }

    public override void updatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pInfo)
    {
        if (pRigidbody.velocity.y < -8)
        {
            pRigidbody.velocity = new Vector2(pRigidbody.velocity.x, -8);
        }
    }

    public void IdleAnimations()
    {

    }
}

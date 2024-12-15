using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallState : BaseState
{
    private float moveSpeed = 4f;
    private float mGravityModifier = 1f;
    Vector2 playerVelocity = new Vector2(0, 0);

    public override void enterState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerConfig pPlayerConfig, PlayerInfo pInfo)
    {
        pRigidbody.gravityScale = 2f;
        moveSpeed = pPlayerConfig.GetInAirSpeed();
        mGravityModifier = pPlayerConfig.GetGravityModifer();

        pAnimator.SetBool("IsFalling", true);
        pRigidbody.velocity = new Vector2(pRigidbody.velocity.x, pRigidbody.velocity.y);
    }

    public override void updateState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        if (!pInfo.IsAlive()) return;
        InAir(pRigidbody, pInfo);
    }

    public override void updatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pInfo)
    {
        //Limit fall speed to a certain amount
        if (pRigidbody.velocity.y < -8f)
        {
            pRigidbody.velocity = new Vector2(pRigidbody.velocity.x, -8);
        }

        //Need to cap speed if we're launched
        if (pRigidbody.velocity.y > 15f)
        {
            pRigidbody.velocity = new Vector2(pRigidbody.velocity.x, 15f);
        }
    }

    public override void exitState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        pAnimator.SetBool("IsFalling", false);
        pInfo.ResetSpeed();
    }

    void InAir(Rigidbody2D pRigidbody, PlayerInfo pInfo)
    {
        playerVelocity = new Vector2(pInfo.GetMoveInput().x * moveSpeed, pRigidbody.velocity.y);
        pRigidbody.velocity = playerVelocity;
    }
}

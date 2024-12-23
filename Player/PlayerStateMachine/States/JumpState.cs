using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : MoveSubState
{
    private float mGravityModifier = 1f;
    private float inAirGravityScale;
    private float moveSpeed = 4f;
    private float jumpTimer = 0.3f;
    private float minCancelTimer = 0.3f;
    bool timerHasBeentHit = false;
    bool cancelRegistered = false;
    Vector2 playerVelocity = new Vector2(0, 0);

    public override void enterState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerConfig pPlayerConfig, PlayerInfo pInfo)
    {
        Debug.Log("Entering jump state");
        jumpTimer = pPlayerConfig.GetJumpTimer();
        moveSpeed = pPlayerConfig.GetInAirSpeed();
        minCancelTimer = pPlayerConfig.GetMinCancelTimer();
        pRigidbody.gravityScale = pPlayerConfig.GetGravityScale();
        mGravityModifier = pPlayerConfig.GetGravityModifer();

        cancelRegistered = false;
        timerHasBeentHit = false;
        
        //AudioManagement.instance.OnPlayerJump();

        pAnimator.SetBool("IsJumping", true);
        pAnimator.SetFloat("JumpInput", 0);
        pRigidbody.velocity = new Vector2(pRigidbody.velocity.x, pPlayerConfig.GetJumpSpeed());
    }

    public override void updateState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        if (cancelRegistered)
        {
            pAnimator.SetFloat("JumpInput", 1);
        }
        UpdateTimer();
    }

    public override void exitState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        pAnimator.SetBool("IsJumping", false);
        pAnimator.SetFloat("JumpInput", 0);
        pInfo.SetContextCancel(false);
        cancelRegistered = false;
        timerHasBeentHit = false;
    }

    public override void updatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pInfo)
    {
        //Here cancel has not been hit, meaning we reached the apex of the jump and are beginning to fall
        if (pRigidbody.velocity.y < 0 && !pInfo.isContextCanceled())
        {
            pRigidbody.velocity += Vector2.up * Physics2D.gravity.y * (mGravityModifier) * Time.fixedDeltaTime;
            pInfo.SetContextCancel(true);
            cancelRegistered = true;
        }

        //We have begun to fall, continue iterating on it
        if (cancelRegistered) 
        {
            pRigidbody.velocity += Vector2.up * Physics2D.gravity.y * (mGravityModifier) * Time.fixedDeltaTime;
        }

        //Cancel has been hit after minimum time, halt the jump and start falling
        if (jumpTimer > 0 && jumpTimer < minCancelTimer && pInfo.isContextCanceled() && !cancelRegistered)
        {
            
            pRigidbody.velocity = new Vector2(pRigidbody.velocity.x, 0);
            cancelRegistered = true;
        }

        //Limit fall speed to a certain amount
        if (pRigidbody.velocity.y < -10f) {
            pRigidbody.velocity = new Vector2(pRigidbody.velocity.x, -10f);
        }

        //Finally we need to update the x direction of the player according to the move speed
        playerVelocity = new Vector2(pInfo.GetMoveInput().x * moveSpeed, pRigidbody.velocity.y);
        pRigidbody.velocity = playerVelocity;
    }

    void UpdateTimer()
    {
        //TODO - Should remove this jumptimer check
        if (jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;
        }
    }

    public override PlayerState GetStateType()
    {
        return PlayerState.Jumping;
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallJumpState : BaseState
{
    private float originalGravity = 0f;
    private Vector2 modifiedDirection = Vector2.zero;
    private float mGravityModifier = 1f;
    private bool cancelHasBeenHit = false;
    private float moveSpeed = 4f;
    private float jumpTimer = 0.3f;
    private float minCancelTimer = 0.3f;
    private float wallJumpGravity = 1.5f;
    //Use to prevent player from taking control too early
    private float wallJumpBuffer = 0.2f;

    public override void enterState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerConfig pPlayerConfig, PlayerInfo pInfo)
    {
        jumpTimer = pPlayerConfig.GetJumpTimer();
        moveSpeed = pPlayerConfig.GetInAirSpeed();
        minCancelTimer = pPlayerConfig.GetMinCancelTimer();
        modifiedDirection = Vector2.zero;
        pAnimator.SetBool("IsJumping", true);
        Debug.Log("In wall jump state");
        pRigidbody.gravityScale = pPlayerConfig.GetGravityScale();
        mGravityModifier = pPlayerConfig.GetGravityModifer();
        wallJumpBuffer = pPlayerConfig.GetWallJumpBuffer();
        
        pRigidbody.velocity = Vector2.zero;
        //pRigidbody.AddForce(new Vector2(pRigidbody.velocity.x * 25f, pRigidbody.velocity.y), ForceMode2D.Force);
        Debug.Log("Wall jump speed is " + pInfo.GetWallJumpSpeed());
        if (pInfo.wallJumpLeft)
        {
            modifiedDirection = new Vector2(pInfo.GetWallJumpSpeed(), pPlayerConfig.GetJumpSpeed());
        }
        else if(pInfo.wallJumpRight)
        {
            modifiedDirection = new Vector2(-pInfo.GetWallJumpSpeed(), pPlayerConfig.GetJumpSpeed());
        }

        pRigidbody.velocity = new Vector2(modifiedDirection.x, pPlayerConfig.GetJumpSpeed());
    }

    public override void exitState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        Debug.Log("Exiting wall jump");
        pInfo.ResetSpeed();
        pRigidbody.gravityScale = originalGravity;
        pRigidbody.velocity = new Vector2(0f, pRigidbody.velocity.y);
        pAnimator.SetBool("IsJumping", false);
        pInfo.wallJumpRight = false;
        pInfo.wallJumpLeft = false;
        pInfo.SetContextCancel(false);
    }

    public override void updatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pInfo)
    {
        if (pRigidbody.velocity.y < 0 && !pInfo.isContextCanceled())
        {
            pRigidbody.velocity += Vector2.up * Physics2D.gravity.y * (mGravityModifier) * Time.fixedDeltaTime;
            pInfo.SetContextCancel(true);
        }
        else if (cancelHasBeenHit)
        {
            pRigidbody.velocity += Vector2.up * Physics2D.gravity.y * (mGravityModifier) * Time.fixedDeltaTime;
        }
        else if (jumpTimer > 0 && jumpTimer < minCancelTimer && pInfo.isContextCanceled() && !cancelHasBeenHit)
        {
            pRigidbody.velocity = new Vector2(pInfo.GetMoveInput().x * moveSpeed, 0);
            cancelHasBeenHit = true;
        }
    }

    public override void updateState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        if(wallJumpBuffer < 0)
        {
            Debug.Log("Wall jump buffer is less than zero");
            InAir(pRigidbody, pInfo);
        }
        if (cancelHasBeenHit)
        {
            pAnimator.SetFloat("JumpInput", 1);
        }
        wallJumpBuffer -= Time.deltaTime;
    }

    void InAir(Rigidbody2D pRigidbody, PlayerInfo pInfo)
    {
        if (pInfo.GetMoveInput().x > 0.05f || pInfo.GetMoveInput().x < -0.05f)
        {
            pRigidbody.velocity = new Vector2(pInfo.GetMoveInput().x * moveSpeed, pRigidbody.velocity.y);
        }
        
    }
}

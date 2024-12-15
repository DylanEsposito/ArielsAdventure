using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//PyerInfo
//Use: Store player related values that are continuously updated during playtime, this revolves around movement, what state the player is in
//EX: coyoteTimeCounter will be updated very frame the player is off the ground but is reset to coyoteTimer 
public class PlayerInfo
{
    public Vector2 moveInput;
    public Vector2 lastMoveInput;

    //Player states
    private bool isAlive = true;
    private bool isGrounded = false;
    private bool isDashing = false;
    private bool canDash = false;

    private float gravityScaleAtStart;
    private float speedAtStart;

    private float climbTime = 0;
    private float currentClimbTime = 1f;
    private float jumpTime = 0;
    private float coyoteTimeCounter;

    private bool isJumping = false;
    private bool contextCanceled = false;
    private bool jumpContextCanceled = false;

    //Climbing variables
    public bool climbLeft = false;
    public bool climbRight = false;

    //Wall slide variables
    public bool wallSlideLeft = false;
    public bool wallSlideRight = false;

    //Wall jump variables
    public bool wallJumpLeft = false;
    public bool wallJumpRight = false;

    //Wate related variables
    private bool isSwimming = false;
    private bool waterHit = false;

    private float moveSpeed = 4.5f;
    private float climbExitTime = 0.1f;
    private bool movementLock = false;

    private Transform wallCheck;

    public PlayerInfo(Transform pWallCheck, float pMoveSpeed)
    {
        wallCheck = pWallCheck;
        moveSpeed = pMoveSpeed;
        speedAtStart = moveSpeed;
    }

    public void SetIsGrounded(bool pState)
    {
        isGrounded = pState;
    }

    public bool IsOnGround()
    {
        return isGrounded;
    }

    public bool IsAlive()
    {
        return isAlive;
    }

    public Vector2 GetMoveInput()
    {
        return moveInput;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public bool isContextCanceled()
    {
        return jumpContextCanceled;
    }

    public void SetContextCancel(bool pState)
    {
        jumpContextCanceled = pState;
    }

    public bool GetClimbLeft()
    {
        return climbLeft;
    }

    public bool GetClimbRight()
    {
        return climbRight;
    }

    public void SetSpeed(float pSpeed)
    {
        moveSpeed = pSpeed;
    }

    public void ResetSpeed()
    {
        moveSpeed = speedAtStart;
    }

    public void SetSwimming(bool pState)
    {
        isSwimming = false;
    }

    public Transform GetWallCheck()
    {
        return wallCheck;
    }

    float wallJumpSpeed = 5f;

    public void SetWallJumpSpeed(float pSpeed)
    {
        wallJumpSpeed = pSpeed;
    }

    public float GetWallJumpSpeed()
    {
        return wallJumpSpeed;
    }

    public void SetCanDash(bool pState)
    {
        canDash = pState;
    }

    public bool CanWeDash()
    {
        return canDash;
    }

    public void SetClimbTime(float pFloat)
    {
        currentClimbTime = pFloat;
    }

    public float GetClimbTime()
    {
        return currentClimbTime;
    }

    public bool MovementLocked()
    {
        return movementLock;
    }

    public void SetMovementLock(bool pState)
    {
        movementLock = pState;
    }
}

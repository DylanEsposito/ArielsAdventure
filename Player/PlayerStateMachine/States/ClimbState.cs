using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbState : BaseState
{
    private Vector2 rightOffset;
    private Vector2 leftOffset;
    private float collisionRadius;
    private LayerMask wallMask;
    private float gravityScaleAtStart = 1;
    private bool climbLeft = false;
    private bool climbRight = false;
    private bool wallRight, wallLeft = false;
    private float climbSpeed = 3f;

    private float remainingClimbTime = 1f;

    Vector2 wallExitKick = new Vector2(25f, 5f);
    float yVelocity = 0f;
    bool sentAlert = false;
    public override void enterState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerConfig pPlayerConfig, PlayerInfo pInfo)
    {
        Debug.Log("DYLAN Entered climb state");
        pInfo.SetMovementLock(false);
        sentAlert = false;
        rightOffset = pPlayerConfig.rightOffset;
        leftOffset = pPlayerConfig.leftOffset;
        collisionRadius = pPlayerConfig.collisionRadius;
        climbRight = pInfo.GetClimbRight();
        climbLeft = pInfo.GetClimbLeft();
        wallMask = pPlayerConfig.GetClimbMask();
        Debug.Log("WallMask is " + wallMask.value);
        wallExitKick = pPlayerConfig.GetWallExit();
        pRigidbody.gravityScale = 0f;
        pRigidbody.velocity = new Vector2(0, 0);
        remainingClimbTime = pInfo.GetClimbTime();
    }

    public override void updateState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        Debug.Log("In climb state");
        remainingClimbTime = pInfo.GetClimbTime();
        if (remainingClimbTime <= 0f)
        {
            Debug.Log("Current climb time is around zero" + remainingClimbTime);
            PlayerEvents.instance.ClimbTimerEnd();
            return;
        }
        if (!pInfo.MovementLocked())
        {
            ClimbWall(pGameObject, pRigidbody, pAnimator, pInfo);
        }

    }

    public override void exitState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        Debug.Log("Exiting climb state");
        pRigidbody.gravityScale = gravityScaleAtStart;
        pAnimator.SetBool("isClimbing", false);
        pInfo.SetMovementLock(false);
        pRigidbody.gravityScale = 3;
        climbLeft = false;
        pInfo.climbLeft = false;
        climbRight = false;
        pInfo.climbRight = false;
    }

    public override void updatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pInfo)
    {
        pRigidbody.gravityScale = 0f;
    }

    void ClimbWall(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        //TO DO - Causes player to move up, need to fix this
        if (climbRight)
        {
            wallRight = Physics2D.OverlapCircle((Vector2)pGameObject.transform.position + rightOffset, collisionRadius, wallMask);
            if (!wallRight && !sentAlert)
            {
                Debug.Log("End climb on right");
                sentAlert = true;
                pInfo.SetMovementLock(true);
                pRigidbody.velocity = new Vector2(0, 0);
                PlayerEvents.instance.EndClimb();
                return;
            }
        }
        else if (climbLeft)
        {

            wallLeft = Physics2D.OverlapCircle((Vector2)pGameObject.transform.position + leftOffset, collisionRadius, wallMask);
            if (!wallLeft && !sentAlert)
            {
                Debug.Log("End climb on left");
                sentAlert = true;
                pInfo.SetMovementLock(true);
                pRigidbody.velocity = new Vector2(0, 0);
                PlayerEvents.instance.EndClimb();
                return;
            }
        }

        //To avoid speed manipulation, we'll just round to nearest whole number and skip regular normalization
        //Idk what it is who cares
        if (pInfo.GetMoveInput().y > 0.15)
        {
            yVelocity = 1f;
        }
        else if (pInfo.GetMoveInput().y < -0.15)
        {
            yVelocity = -1f;
        }
        else
        {
            yVelocity = 0f;
        }

        Vector2 climbVelocity = new Vector2(pInfo.GetMoveInput().x, yVelocity);
        pRigidbody.velocity = new Vector2(0f, climbVelocity.y * climbSpeed);
        //This is a check to see if we should pause the animation and set it to either climbing or idle
        pAnimator.SetBool("isClimbing", true);
        pAnimator.SetFloat("ClimbInput", climbVelocity.y);
    }

    public override PlayerState GetStateType()
    {
        return PlayerState.Climbing;
    }
}

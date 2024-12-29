using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimmingState : BaseState
{
    private LayerMask waterMask;
    float waterGravity = 1f;
    private float swimSpeed = 2f;
    Vector2 previousDirection;

    public override void enterState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerConfig pPlayerConfig, PlayerInfo pInfo)
    {
        
        Debug.Log("Entering swimming state");
        waterGravity = pPlayerConfig.GetWaterGravity();
        swimSpeed = pPlayerConfig.GetSwimSpeed();
        pRigidbody.velocity = Vector2.zero;
        pInfo.SetSpeed(pPlayerConfig.GetSwimSpeed());
        pAnimator.SetBool("IsSwimming", true);
        pInfo.SetCanDash(true);
    }

    public override void exitState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        Debug.Log("Exiting swimming state");
        pInfo.ResetSpeed();
        pInfo.SetSwimming(false);
        pAnimator.SetBool("IsSwimming", false);
    }

    public override void updatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pInfo)
    {
        pRigidbody.gravityScale = 1f;
        SwimMovement(pRigidbody, pInfo.GetMoveInput());
    }

    public override void updateState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        if (pInfo.GetMoveInput().x > 0.05)
        {
            pAnimator.SetFloat("SwimInput", 1);
        }
        else if (pInfo.GetMoveInput().x < -0.05)
        {
            pAnimator.SetFloat("SwimInput", 1);
        }
        else
        {
            pAnimator.SetFloat("SwimInput", 0);
        }
    }

    void SwimMovement(Rigidbody2D pRigidbody, Vector2 movement)
    {
        //Slow down y movement by just a little
        float moveAmountY = movement.y * (swimSpeed * 0.75f);
                
        pRigidbody.velocity = new Vector2(movement.x * swimSpeed, moveAmountY);

        //Determine direction we should face
        bool playerHasHorizontalSpeed = Mathf.Abs(pRigidbody.velocity.x) > Mathf.Epsilon;
        if (playerHasHorizontalSpeed)
        {
            float angle = Mathf.Atan2(pRigidbody.velocity.y, pRigidbody.velocity.x) * Mathf.Rad2Deg;
            pRigidbody.transform.localScale = new Vector2(Mathf.Sign(pRigidbody.velocity.x), 1);
        }
        
        //Stop velocity if we're no longer moving
        if (movement == Vector2.zero)
        {
            pRigidbody.velocity = new Vector2(0f, 0f);
        }

        previousDirection = movement;
    }

    public override PlayerState GetStateType()
    {
        return PlayerState.Swimming;
    }
}

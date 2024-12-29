using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariel.PlayerStates
{
    public class DashState : BaseState
    {
        private float dashSpeed;
        private float originalGravity = 0f;
        Vector2 modifiedDirection = Vector2.zero;

        public override void enterState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerConfig pPlayerConfig, PlayerInfo pInfo)
        {
            //Debug.Log("Entering dash state");
            pAnimator.SetBool("IsDashing", true);
            originalGravity = pRigidbody.gravityScale;
            dashSpeed = pPlayerConfig.GetDashSpeed();
            pInfo.SetSpeed(dashSpeed);
            Vector2 dashDirection = new Vector2(pInfo.GetMoveInput().x, pInfo.GetMoveInput().y).normalized;
            originalGravity = pRigidbody.gravityScale;
            pRigidbody.gravityScale = 0f;

            //mTrailRenderer.enabled = true;
            if (dashDirection != Vector2.zero)
            {
                //Debug.Log("Dashing in a direction");
                //Angle math
                float angle = Mathf.Atan2(dashDirection.y, dashDirection.x) * Mathf.Rad2Deg;
                angle = Mathf.Round(angle / 45) * 45;
                angle *= Mathf.Deg2Rad;
                Vector2 modifiedDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                //Debug.Log("Dash direction not eqaul to zero for modified direction is " + modifiedDirection + " these are the values of " + pInfo.GetMoveInput().x.ToString() + " in the y direction of " + pInfo.GetMoveInput().y.ToString());
                pRigidbody.velocity = new Vector2(modifiedDirection.x * pInfo.GetMoveSpeed(), modifiedDirection.y * (pInfo.GetMoveSpeed() * 0.75f));
                //Debug.Log("This is the velocity" + pRigidbody.velocity.ToString());
            }
            else
            {
                //Debug.Log("Dashing in default direction " + transform.localScale.x);
                modifiedDirection = new Vector2(pGameObject.transform.localScale.x, 0f);
                Debug.Log("Modified direction is " + modifiedDirection);
                pRigidbody.velocity = new Vector2(modifiedDirection.x * dashSpeed, 0f);

            }
        }

        public override void exitState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
        {
            pInfo.ResetSpeed();
            pRigidbody.gravityScale = originalGravity;
            pRigidbody.velocity = new Vector2(0f, pRigidbody.velocity.y);
            pAnimator.SetBool("IsDashing", false);
        }

        public override PlayerStateType GetStateType()
        {
            return PlayerStateType.Dashing;
        }

        public override void updatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pInfo)
        {
            pRigidbody.gravityScale = 0f;
        }

        public override void updateState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
        {
            Debug.Log("In dash state");
            //pRigidbody.velocity = new Vector2(modifiedDirection.x * dashSpeed, modifiedDirection.y * dashSpeed);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariel.PlayerStates
{
    public class WallSlidingState : BaseState
    {
        private bool isSliding = false;
        private float wallSlidingSpeed = 1.5f;

        public override void enterState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerConfig pPlayerConfig, PlayerInfo pInfo)
        {
            Debug.Log("Entering slide state");
            isSliding = true;
            wallSlidingSpeed = pPlayerConfig.GetWallSlidingSpeed();
            //pAnimator.SetBool("isSliding", true);
            pRigidbody.velocity = new Vector2(0, 0);
        }

        public override void exitState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
        {
            Debug.Log("Exiting slide state");
            pInfo.wallSlideLeft = false;
            pInfo.wallSlideRight = false;
            //pRigidbody.velocity = new Vector2(pRigidbody.velocity.x, 0f);
            pAnimator.SetBool("isSliding", false);
        }

        public override void updatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pInfo)
        {
            pRigidbody.velocity =
                    new Vector2(pRigidbody.velocity.x, Mathf.Clamp(pRigidbody.velocity.y,
                    -wallSlidingSpeed, float.MaxValue));
        }

        public override void updateState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
        {

        }

        public override PlayerStateType GetStateType()
        {
            return PlayerStateType.WallSliding;
        }
    }
}
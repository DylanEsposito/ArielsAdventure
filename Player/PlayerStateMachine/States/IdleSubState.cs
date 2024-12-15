using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IdleSubState : BaseState
{
    public override void enterState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerConfig pPlayerConfig, PlayerInfo pStateManager)
    {
        throw new System.NotImplementedException();
    }

    public override void exitState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pStateManager)
    {
        throw new System.NotImplementedException();
    }

    public override void updatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pStateManager)
    {
        throw new System.NotImplementedException();
    }

    public override void updateState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pStateManager)
    {
        throw new System.NotImplementedException();
    }
}

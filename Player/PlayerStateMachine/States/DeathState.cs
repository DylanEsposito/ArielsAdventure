using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : BaseState
{
    public override void enterState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerConfig pPlayerConfig, PlayerInfo pPlayerInfo)
    {
        Debug.Log("Player is dead");
        pAnimator.SetTrigger("Dying");
        pRigidbody.velocity = new Vector2(0, 0);
        pRigidbody.AddForce(pPlayerConfig.GetDeathKick(), ForceMode2D.Impulse);
    }

    public override void exitState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pPlayerInfo)
    {
        
    }

    public override void updatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pPlayerInfo)
    {
        
    }

    public override void updateState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pPlayerInfo)
    {
        
    }
}

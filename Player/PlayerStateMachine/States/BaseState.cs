using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState 
{
    public enum SubType : ushort
    {
        movement = 0,
        temporary = 1,
        action = 2
    }

    private Rigidbody2D playerRB;
    public abstract void enterState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerConfig pPlayerConfig, PlayerInfo pPlayerInfo);

    public abstract void updateState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pPlayerInfo);

    public abstract void exitState(GameObject pGameObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pPlayerInfo);

    public abstract void updatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pPlayerInfo);
}

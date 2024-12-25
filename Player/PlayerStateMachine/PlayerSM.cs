using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerSM 
{
    private BaseState currentState;


    public void UpdateState(GameObject pObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        currentState.updateState(pObject, pRigidbody, pAnimator, pInfo);
    }

    public void UpdatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pInfo)
    {
        currentState.updatePhysics(pGameObject, pRigidbody, pInfo);
    }

    public void SwitchState(BaseState pState, GameObject pObject, Rigidbody2D pRigidbody2D, Animator pAnimator, PlayerConfig pConfig, PlayerInfo pInfo)
    {
        if (currentState.GetType() != pState.GetType())
        {
            currentState.exitState(pObject, pRigidbody2D, pAnimator, pInfo);
            currentState = pState;
            currentState.enterState(pObject, pRigidbody2D, pAnimator, pConfig, pInfo);
        }
    }
}
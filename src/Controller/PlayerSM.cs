using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Ariel.PlayerStates;

public class PlayerSM 
{
    private BaseState currentState;

    public PlayerSM()
    {
        currentState = new IdleState();
    }

    public void UpdateState(GameObject pObject, Rigidbody2D pRigidbody, Animator pAnimator, PlayerInfo pInfo)
    {
        currentState.updateState(pObject, pRigidbody, pAnimator, pInfo);
    }

    public void UpdatePhysics(GameObject pGameObject, Rigidbody2D pRigidbody, PlayerInfo pInfo)
    {
        currentState.updatePhysics(pGameObject, pRigidbody, pInfo);
    }

    public void SwitchState(PlayerStateType pState, GameObject pObject, Rigidbody2D pRigidbody2D, Animator pAnimator, PlayerConfig pConfig, PlayerInfo pInfo)
    {
        Debug.Log("Passed in  playerstate is " + pState);
        if (currentState.GetStateType() != pState)
        {
            switch (pState)
            {                
               case PlayerStateType.Running:
                    Debug.Log("Swithcing to run state");
                    currentState.exitState(pObject, pRigidbody2D, pAnimator, pInfo);
                    currentState = new RunState();
                    currentState.enterState(pObject, pRigidbody2D, pAnimator, pConfig, pInfo);
                    break;
                case PlayerStateType.WallSliding:
                    Debug.Log("Swithcing to run state");
                    currentState.exitState(pObject, pRigidbody2D, pAnimator, pInfo);
                    currentState = new WallSlidingState();
                    currentState.enterState(pObject, pRigidbody2D, pAnimator, pConfig, pInfo);
                    break;
                case PlayerStateType.Jumping:
                    Debug.Log("Swithcing to run state");
                    currentState.exitState(pObject, pRigidbody2D, pAnimator, pInfo);
                    currentState = new JumpState();
                    currentState.enterState(pObject, pRigidbody2D, pAnimator, pConfig, pInfo);
                    break;
                case PlayerStateType.WallJumping:
                    Debug.Log("Swithcing to run state");
                    currentState.exitState(pObject, pRigidbody2D, pAnimator, pInfo);
                    currentState = new WallJumpState();
                    currentState.enterState(pObject, pRigidbody2D, pAnimator, pConfig, pInfo);
                    break;
                case PlayerStateType.Falling:
                    Debug.Log("Swithcing to run state");
                    currentState.exitState(pObject, pRigidbody2D, pAnimator, pInfo);
                    currentState = new FallState();
                    currentState.enterState(pObject, pRigidbody2D, pAnimator, pConfig, pInfo);
                    break;
                case PlayerStateType.Dashing:
                    Debug.Log("Swithcing to run state");
                    currentState.exitState(pObject, pRigidbody2D, pAnimator, pInfo);
                    currentState = new DashState();
                    currentState.enterState(pObject, pRigidbody2D, pAnimator, pConfig, pInfo);
                    break;
                case PlayerStateType.Climbing:
                    Debug.Log("Swithcing to run state");
                    currentState.exitState(pObject, pRigidbody2D, pAnimator, pInfo);
                    currentState = new ClimbState();
                    currentState.enterState(pObject, pRigidbody2D, pAnimator, pConfig, pInfo);
                    break;
                case PlayerStateType.Swimming:
                    Debug.Log("Swithcing to run state");
                    currentState.exitState(pObject, pRigidbody2D, pAnimator, pInfo);
                    currentState = new SwimmingState();
                    currentState.enterState(pObject, pRigidbody2D, pAnimator, pConfig, pInfo);
                    break;
                case PlayerStateType.WaterDashing:
                    Debug.Log("Swithcing to run state");
                    currentState.exitState(pObject, pRigidbody2D, pAnimator, pInfo);
                    currentState = new WaterDashState();
                    currentState.enterState(pObject, pRigidbody2D, pAnimator, pConfig, pInfo);
                    break;
                default:
                    Debug.Log("Swithcing to idle state");
                    currentState.exitState(pObject, pRigidbody2D, pAnimator, pInfo);
                    currentState = new IdleState();
                    currentState.enterState(pObject, pRigidbody2D, pAnimator, pConfig, pInfo);
                    break;
            }
            
        }
    }

    public BaseState GetCurrentState()
    {
        return currentState;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    protected SpriteRenderer spriteRender;
    protected Animator theAnimator;
    protected Rigidbody2D theRigidbody;

    protected float gravityScaleAtStart;

    //Expected states in order to run
    protected BaseState currentState;
    protected IdleState idleState;
    protected RunState runState;
    protected DeathState deathState;
    protected FallState fallState;

    protected bool isAlive = true;
    protected bool isGrounded = false;
    protected bool isPaused = false;
    protected bool isDashing = false;

    [SerializeField] protected BoxCollider2D bodyCollider;
    [SerializeField] protected LayerMask groundMask, wallMask, interactMask, waterMask;
}

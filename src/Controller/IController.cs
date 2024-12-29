using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IController : MonoBehaviour
{
    [SerializeField] protected BoxCollider2D bodyCollider;
    [SerializeField] protected LayerMask groundMask, wallMask, interactMask, waterMask;
    [SerializeField] protected float groundCheckDistance = 0.2f;
    [SerializeField] protected PlayerConfig playerConfig;
    [SerializeField] protected AudioSource characterAudio;
    [SerializeField] private Transform wallCheck;

    protected float gravityScaleAtStart;
    protected bool isAlive = true;
    protected bool isGrounded = false;
    protected bool isPaused = false;

    //DEEM Move value set in LoadInConfig check
    protected float wallCorrectionAmount = 0.45f;
    protected float wallSlideSeperation = 0.5f;
    protected float collisionRadius = 0.2f;
    protected float wallJumpSpeed = 5f;

    //Allows player to escape ground check when jumping due to checking every frame
    protected float inAirBuffer;
    //Max amount of time player can be jumping before returning back down
    protected float jumpingBuffer;
    protected float gravityModifier = 1.0f;

    //DEEM Move these to parent class
    protected float waterExit = 50f;

    //DEEM Move to upper class or just place under playerinfo
    protected SpriteRenderer spriteRenderer;
    protected Animator theAnimator;
    protected Rigidbody2D theRigidbody;

    //For when you want to adjust characters color
    protected Color originalColor;
    protected Color adjustedColor;

    protected abstract void Die();

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

//Purpose of this class is to manage input and send out updates to statemanager
//Should consider keeping statemanager on monobehaviour and just instantiate it from statemanager
public class PlayerController : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    //Stores player's state
    //DEEM Convert to monobehaviour

    [field: SerializeField] public PlayerInfo playerInfo { get; private set; }
    private PlayerSM stateMachine;

    [SerializeField] PlayerConfig playerConfig;
    [SerializeField] PlayerInput playerInput;
    InputAction moveVector;

    [Header("Ground Check")]
    [SerializeField] GameObject frontFoot;
    [SerializeField] GameObject backFoot;
    [SerializeField] Transform wallCheck;
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] protected LayerMask groundMask;

    [Header("Sound")]
    [SerializeField] AudioSource playerAudio;

    Rigidbody2D theRigidbody;
    Animator theAnimator;
    SpriteRenderer spriteRender;
    bool isGrounded = false;
    

    private void Awake()
    {
        LoadInConfig();
        //DEEM Convert playerInfo to monobehaviour so we can store 
        playerInfo = new PlayerInfo(wallCheck, playerConfig.GetMoveSpeed());
        spriteRender = GetComponent<SpriteRenderer>();
        theAnimator = GetComponent<Animator>();
        theRigidbody = GetComponent<Rigidbody2D>();
    }

    void LoadInConfig()
    {
        //collisionRadius = playerConfig.GetCollisionRadius();
        //wallCorrectionAmount = playerConfig.GetWallCorrection();
        //gravityModifier = playerConfig.GetGravityModifer();
        //nAirBuffer = playerConfig.GetAirTimer();
        //umpingBuffer = playerConfig.GetJumpTimer();
    }

    void Start()
    {
        GameSessionManager.instance.UpdateLastSave();
        //gravityScaleAtStart = theRigidbody.gravityScale;
        //playerInfo.SetWallJumpSpeed(wallJumpSpeed);
        // currentTimeClimbTime = climbTime;
        /*playerInfo.SetClimbTime(climbTime);
        PlayerEvents.instance.onEndClimb += LeaveClimb;
        PlayerEvents.instance.onClimbTimeEnd += FallOutOfClimb;
        PlayerEvents.instance.onResetJump += ZeroOutJumpingVars;
        originalColor = spriteRender.color;*/
        stateMachine.SwitchState(new IdleState(), this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
    }

    private void OnDestroy()
    {
        /*PlayerEvents.instance.onEndClimb -= LeaveClimb;
        PlayerEvents.instance.onClimbTimeEnd -= FallOutOfClimb;
        PlayerEvents.instance.onResetJump -= ZeroOutJumpingVars;*/
    }

    void Update()
    {
        /*if (!isAlive)
        {
            if (stateMachine.currentState.GetType() != stateMachine.deathState.GetType())
            {
                SwitchState(deathState);
            }
            return;
        }

        if (CheckGround())
        {
            if (currentState.GetType() == playerJumpState.GetType() && inAirBuffer > 0.05)
            {
                inAirBuffer -= Time.deltaTime;
                return;
            }
            else if (currentState.GetType() == playerClimbState.GetType())
            {
                continousClimbCheck = false;
            }
            else
            {
                if (currentState.GetType() == playerJumpState.GetType()
                    || currentState.GetType() == playerWallJumpState.GetType()
                    || currentState.GetType() == fallState.GetType())
                {
                    SwitchCheckIfMoving();
                    CreateDust();
                }
                ResetCoyoteTimer();
                playerInfo.SetCanDash(true);
                isJumping = false;
                leavingClimb = false;
                ResetClimbTime();
                //theRigidbody.gravityScale = gravityScaleAtStart;
            }
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            jumpingBuffer -= Time.deltaTime;
            wallCheckTimer -= Time.deltaTime;
        }*/

        ActiveMoveCheck();
        //ContextualChecks();
        //JumpingTimer();
        //ClimbingTimer();
        //FlipSprite();

        //Now finally update the current state
        stateMachine.UpdateState(this.gameObject, this.theRigidbody, this.theAnimator, this.playerInfo);
    }

    private void FixedUpdate()
    {

        //stateMachine.UpdatePhysics(this.gameObject, this.theRigidbody, this.playerInfo);

        /*if (!isGrounded && !isDashing && !playerInfo.climbLeft && !playerInfo.climbRight)
        {
            theRigidbody.velocity += Vector2.up * Physics2D.gravity.y * (gravityModifier) * Time.fixedDeltaTime;
        }*/
    }

    private void ActiveMoveCheck()
    {
        moveVector = playerInput.actions["move"];
        OnWalk(moveVector);
    }

    private void OnWalk(InputAction context)
    {
        //if (!isAlive || playerInfo.MovementLocked()) { return; }

        playerInfo.moveInput = context.ReadValue<Vector2>();
        playerInfo.lastMoveInput = playerInfo.moveInput;
        SwitchCheckIfMoving();

        /*switch (currentState)
        {
            case DeathState:
                break;
            case ClimbState:
                break;
            case FallState:
                break;
            case JumpState:
                break;
            case WallJumpState:
                break;
            case WallSlidingState:
                break;
            case DashState:
                break;
            case WaterDashState:
                break;
            case SwimmingState:
                break;
            default:
                SwitchCheckIfMoving();
                break;
        }*/
    }

    public void SwitchCheckIfMoving()
    {
        if (isGrounded)
        {
            if (playerInfo.moveInput.x > 0.05 || playerInfo.moveInput.x < -0.05)
            {
                stateMachine.SwitchState(new RunState(), this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
            }
            else
            {
                stateMachine.SwitchState(new IdleState(), this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
            }
        }
        else
        {
            stateMachine.SwitchState(new FallState(), this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
        }
    }


    private void UpdateState(BaseState pState)
    {
        frontFoot.SetActive(true);
        backFoot.SetActive(true);

        //stateMachine.SwitchState(new pState(), );
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Water check
        /*if (LayerMask.LayerToName(collision.gameObject.layer).Equals("Water") && !bodyCollider.IsTouchingLayers(LayerMask.GetMask("Water")))
        {
            if (currentState.GetType() != playerWaterDashState.GetType())
            {
                theRigidbody.velocity = new Vector2(0, 3.5f);
                theRigidbody.AddForce(new Vector2(theRigidbody.velocity.x, theRigidbody.velocity.y * waterExit), ForceMode2D.Force);
            }
            SwitchCheckIfMoving();
        }*/
    }

    private bool CheckGround()
    {
        //Feet check for both front and back foot
        RaycastHit2D hitFront = Physics2D.Raycast(
            new Vector3(frontFoot.transform.position.x,
            frontFoot.transform.position.y, frontFoot.transform.position.z),
            -Vector2.up,
            groundCheckDistance,
            groundMask);
        //Back foot
        RaycastHit2D hitBack = Physics2D.Raycast(
            new Vector3(backFoot.transform.position.x,
            backFoot.transform.position.y, backFoot.transform.position.z),
            -Vector2.up,
            groundCheckDistance,
            groundMask);

        if (hitFront || hitBack)
        {
            isGrounded = true;
            playerInfo.SetIsGrounded(isGrounded);
            return isGrounded;
        }

        isGrounded = false;
        playerInfo.SetIsGrounded(isGrounded);
        return isGrounded;
    }

    public void OnJump(InputAction.CallbackContext context)
    {

    }

    public void OnClimb(InputAction.CallbackContext context)
    {

    }

    public void OnDash(InputAction.CallbackContext context)
    {

    }

    public void OnPause(InputAction.CallbackContext context)
    {

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnCling(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnGravChange(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnMeow(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }
}
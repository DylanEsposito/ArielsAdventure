using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerStateManager : StateManager
{

    //Stores player's state
    PlayerInfo playerInfo;

    //Stores player movement related params
    [SerializeField] PlayerConfig playerConfig;
    [SerializeField] AudioSource playerAudio;

    //State management
    JumpState playerJumpState;
    WallJumpState playerWallJumpState;
    ClimbState playerClimbState;
    DashState playerDashState;
    WallSlidingState playerWallSlideState;
    SwimmingState playerSwimState;
    WaterDashState playerWaterDashState;
   
    [SerializeField] PlayerInput playerInput;
    InputAction moveVector;

    [Header("Ground Check")]
    [SerializeField] GameObject frontFoot;
    [SerializeField] GameObject backFoot;
    [SerializeField] private float waterExit = 50f;
    private Collider2D frontFootCollider;
    private Collider2D backFootCollider;
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] float wallJumpSpeed = 5f;

    [SerializeField] private BoxCollider2D headCollider;
    [SerializeField] ParticleSystem dust;

    [Header("Jump Vars")]
    //Allows player to escape ground check when jumping due to checking every frame
    private float inAirBuffer;
    //Max amount of time player can be jumping before returning back down
    private float jumpingBuffer;
    private float coyoteTimeCounter;
    private bool isJumping = false;
    private float gravityModifier = 1.0f;

    [Header("Climb Vars")]
    //private float climbExitTime = 0.2f; // set to private and inherit
    private float wallCorrectionAmount = 0.45f;
    private float wallSlideDist = 0.5f;
    private float collisionRadius = 0.2f;
    private bool continousClimbCheck = false;

    [Header("WallSliding Vars")]
    private float wallCheckTimer = 0f;
    private float wallCheckCooldown = 0.3f;
    private bool isWallSliding = false; // set to private
    [SerializeField] private Transform wallCheck;

    [Header("Dash Vars")]
    private float dashTime = 0.2f;

    [Header("Pause Menu")]
    [SerializeField] public UnityEvent PauseGame;

    //Climb time
    [SerializeField] float climbTime = 10f;
    private float currentTimeClimbTime = 1f;
    private bool leavingClimb = false;
    private Color originalColor;
    private Color adjustedColor;

    private void Awake()
    {
        LoadInConfig();
        SetUpStates();
        playerInfo = new PlayerInfo(wallCheck, playerConfig.GetMoveSpeed());
        spriteRender = GetComponent<SpriteRenderer>();
        theAnimator = GetComponent<Animator>();
        theRigidbody = GetComponent<Rigidbody2D>();
        frontFootCollider = frontFoot.GetComponent<Collider2D>();
        backFootCollider = backFoot.GetComponent<Collider2D>();
    }

    void LoadInConfig()
    {
        collisionRadius = playerConfig.GetCollisionRadius();
        wallCorrectionAmount = playerConfig.GetWallCorrection();
        gravityModifier = playerConfig.GetGravityModifer();
        inAirBuffer = playerConfig.GetAirTimer();
        jumpingBuffer = playerConfig.GetJumpTimer();
    }

    void SetUpStates()
    {
        //Will be a costly for overhead but idc, don't care to do either lazy initialization/or check with these 
        idleState = new IdleState();
        runState = new RunState();
        playerJumpState = new JumpState();
        playerWallJumpState = new WallJumpState();
        playerClimbState = new ClimbState();
        playerDashState = new DashState();
        playerWallSlideState = new WallSlidingState();
        deathState = new DeathState();
        fallState = new FallState();
        playerSwimState = new SwimmingState();
        playerWaterDashState = new WaterDashState();
    }

    void Start()
    {
        GameSessionManager.instance.UpdateLastSave();
        gravityScaleAtStart = theRigidbody.gravityScale;
        playerInfo.SetWallJumpSpeed(wallJumpSpeed);
        currentTimeClimbTime = climbTime;
        playerInfo.SetClimbTime(climbTime);
        PlayerEvents.instance.onEndClimb += LeaveClimb;
        PlayerEvents.instance.onClimbTimeEnd += FallOutOfClimb;
        PlayerEvents.instance.onResetJump += ZeroOutJumpingVars;
        originalColor = spriteRender.color;
        currentState = idleState;
        currentState.enterState(this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
    }

    private void OnDestroy()
    {
        PlayerEvents.instance.onEndClimb -= LeaveClimb;
        PlayerEvents.instance.onClimbTimeEnd -= FallOutOfClimb;
        PlayerEvents.instance.onResetJump -= ZeroOutJumpingVars;
    }

    void Update()
    {
        Debug.Log("This is the current state" + currentState);
        if (!isAlive)
        {
            if (currentState.GetType() != deathState.GetType())
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
        }

        ContextualChecks();
        JumpingTimer();
        ClimbingTimer();
        FlipSprite();

        //Now finally update the current state
        currentState.updateState(this.gameObject, this.theRigidbody, this.theAnimator, this.playerInfo);
    }

    private void ContextualChecks()
    {
        //Checking if player is idle, running, or falling
        ActiveCheck();

        //Player input is required as long as the player is moving in that direction
        //Unfortuntally the unity trigger detection is not reliable enough so we have to do a raycast check each frame
        CollisionCheck();
        WallSlide();

        //Should consider updating yet input system can be annoying to reimplement
        if (continousClimbCheck)
        {
            ClimbCheck();
        }
    }

    private void FixedUpdate(){
        currentState.updatePhysics(this.gameObject, theRigidbody, playerInfo);

        if (!isGrounded && !isDashing && !playerInfo.climbLeft && !playerInfo.climbRight)
        {
            theRigidbody.velocity += Vector2.up * Physics2D.gravity.y * (gravityModifier) * Time.fixedDeltaTime;
        }
    }

    private void SwitchState(BaseState pState)
    {
        frontFoot.SetActive(true);
        backFoot.SetActive(true);
        if (currentState.GetType() != pState.GetType())
        {
            currentState.exitState(this.gameObject, this.theRigidbody, this.theAnimator, this.playerInfo);
            currentState = pState;
            currentState.enterState(this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
        }
    }

    private void SwitchCheckIfMoving()
    {
        if (isGrounded)
        {
            if (playerInfo.moveInput.x > 0.05 || playerInfo.moveInput.x < -0.05)
            {
                SwitchState(runState);
            }
            else
            {
                SwitchState(idleState);
            }
        }
        else
        {
            SwitchState(fallState);
        }
    }

    private void SwitchWaterState()
    {
        switch (currentState)
        {
            case WaterDashState:
                break;
            default:
                SwitchState(playerSwimState);
                break;
        }
    }

    //NOTE: Only works if there is continuous detection on rigidbody
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

    private void ActiveCheck()
    {
        moveVector = playerInput.actions["move"];
        OnActive(moveVector);
    }

    private void OnActive(InputAction context)
    {
        if (!isAlive || playerInfo.MovementLocked()) { return; }

        playerInfo.moveInput = context.ReadValue<Vector2>();
        playerInfo.lastMoveInput = playerInfo.moveInput;

        switch (currentState)
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
        }
    }

    //JUMP AREA
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!isAlive || isDashing || playerInfo.MovementLocked()) { return; }
        if (context.started && coyoteTimeCounter > 0f && !isWallSliding)
        {
            isJumping = true;
            wallCheckTimer = wallCheckCooldown;
            ResetJumpBuffers();

            //DEE Testing for a little boost in x direction -> original value will have us just remove the moveInput.x == 0 check
            if (playerInfo.moveInput.x > 0.05 || playerInfo.moveInput.x < -0.05)
            {
                theRigidbody.AddForce(new Vector2(theRigidbody.velocity.x * 25f, theRigidbody.velocity.y), ForceMode2D.Force);
            }
            CreateDust();
            SwitchState(playerJumpState);
        }
        else if (context.canceled && !playerInfo.isContextCanceled() && currentState.GetType() == playerJumpState.GetType() && jumpingBuffer >= 0)
        {
            playerInfo.SetContextCancel(true);
            ZeroOutJumpingVars();
        }

        //Consider squeezing wallsliding and climbing 
        if (context.started && (playerInfo.climbLeft || playerInfo.climbRight || isWallSliding))
        {
            ResetJumpBuffers();
            if (playerInfo.climbLeft || playerInfo.wallSlideLeft)
            {
                playerInfo.wallJumpLeft = true;
                playerInfo.wallJumpRight = false;
            }
            else if (playerInfo.climbRight || playerInfo.wallSlideRight)
            {
                playerInfo.wallJumpRight = true;
                playerInfo.wallJumpLeft = false;
            }
            playerInfo.climbLeft = false;
            playerInfo.climbRight = false;
            wallCheckTimer = wallCheckCooldown;
            SwitchState(playerWallJumpState);
        }
    }

    private void ResetJumpBuffers()
    {
        inAirBuffer = playerConfig.GetAirTimer();
        jumpingBuffer = playerConfig.GetJumpTimer();
    }

    private void JumpingTimer()
    {
        if (currentState.GetType() == playerJumpState.GetType())
        {
            inAirBuffer -= Time.deltaTime;
            jumpingBuffer -= Time.deltaTime;
        }
    }

    private void ResetCoyoteTimer()
    {
        coyoteTimeCounter = playerConfig.GetCoyoteTimer();

    }

    //CLIMB AREA
    private void ClimbingTimer()
    {
        if (currentState.GetType() == playerClimbState.GetType())
        {
            if (currentTimeClimbTime > 0)
            {
                currentTimeClimbTime -= Time.deltaTime;
                
                //Adjust color of sprite based around current time and mulitply it by 0.01 to increment slowly
                adjustedColor.b = (15 * currentTimeClimbTime) * 0.01f;
                adjustedColor.g = (15 * currentTimeClimbTime) * 0.01f;
                spriteRender.color = adjustedColor;
                playerInfo.SetClimbTime(currentTimeClimbTime);
            }
        }
    }

    private void ResetClimbTime()
    {
        currentTimeClimbTime = climbTime;
        playerInfo.SetClimbTime(currentTimeClimbTime);
        spriteRender.color = originalColor;
        adjustedColor = originalColor;
    }

    public void OnClimb(InputAction.CallbackContext context)
    {
        if (!isAlive || playerInfo.GetClimbTime() <= 0 || playerInfo.MovementLocked()) { continousClimbCheck = false; return; }

        //DEE test
        if (context.started && currentState.GetType() != playerClimbState.GetType())
        {
            continousClimbCheck = true;
        }
        else if (context.canceled)
        {
            continousClimbCheck = false;
        }
        //Detach
        if ((playerInfo.climbRight || playerInfo.climbLeft) && context.started)
        {
            playerInfo.climbRight = false;
            playerInfo.climbLeft = false;
            continousClimbCheck = false;
            SwitchCheckIfMoving();
            return;
        }
    }

    private void ClimbCheck()
    {
        if (!isAlive || playerInfo.GetClimbTime() <= 0) { continousClimbCheck = false; return; }

        //TO DO - Need to identify raycast, since a single one will not be as reliable as a overlap circle
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, 0.8f, playerConfig.GetClimbMask());
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.8f, playerConfig.GetClimbMask());

        if (hitRight)
        {
            Vector2 distance = hitRight.point - new Vector2(transform.position.x, transform.position.y);
            if (distance.x != wallCorrectionAmount)
            {
                //Need to get leftmost position of collider and base new play off of that
                theRigidbody.velocity = new Vector2(0, 0);
                transform.position = new Vector2(hitRight.point.x - wallCorrectionAmount, transform.position.y);
            }
            StartCoroutine(PhaseIntoClimb(false, true));
        }

        else if (hitLeft)
        {
            Vector2 distance = hitLeft.point - new Vector2(transform.position.x, transform.position.y);
            if (distance.x != -(wallCorrectionAmount))
            {
                //Need to get leftmost position of collider and base new play off of that
                theRigidbody.velocity = new Vector2(0, 0);
                transform.position = new Vector2(hitLeft.point.x + wallCorrectionAmount, transform.position.y);
            }
            StartCoroutine(PhaseIntoClimb(true, false));
        }
    }

    public IEnumerator PhaseIntoClimb(bool pClimbLeft, bool pClimbRight)
    {
        theRigidbody.gravityScale = 0f;
        theRigidbody.velocity = new Vector2(0, 0);
        yield return new WaitForSeconds(0.1f);
        playerInfo.climbLeft = pClimbLeft;
        playerInfo.climbRight = pClimbRight;
        isJumping = false;
        continousClimbCheck = false;
        SwitchState(playerClimbState);
        frontFoot.SetActive(false);
        backFoot.SetActive(true);
    }

    private void LeaveClimb()
    {
        if (leavingClimb) { return; }
        leavingClimb = true;
        StartCoroutine(LeavingClimb());
    }

    private void FallOutOfClimb()
    {
        if (leavingClimb) { return; }
        leavingClimb = true;
        SwitchCheckIfMoving();
        leavingClimb = false;
    }

    IEnumerator LeavingClimb()
    {
        bool climbLeft = playerInfo.climbLeft;
        bool climbRight = playerInfo.climbRight;
        //SwitchState(idleState);

        if (climbLeft)
        {
            //Need to shoot a boxcast and verify we don't hit a platform, if so then we just drop
            bool groundLeft = Physics2D.OverlapCircle(
                new Vector2(transform.position.x - playerConfig.GetWallExit().x, transform.position.y + playerConfig.GetWallExit().y),
                collisionRadius, groundMask);

            if (!groundLeft)
            {
                Debug.Log("No ground detected on left exit");
                theAnimator.SetBool("IsLedgeClimbing", true);
                yield return new WaitForSeconds(playerConfig.GetClimbExitTime());
                theAnimator.SetBool("IsLedgeClimbing", false);
                transform.position = new Vector3(transform.position.x - playerConfig.GetWallExit().x, transform.position.y + playerConfig.GetWallExit().y, transform.position.z);
            }
        }
        else if (climbRight)
        {
            bool groundRight = Physics2D.OverlapCircle(
                new Vector2(transform.position.x + playerConfig.GetWallExit().x, transform.position.y + playerConfig.GetWallExit().y),
                collisionRadius, groundMask);
            if (!groundRight)
            {
                Debug.Log("No ground detected on right exit");
                theAnimator.SetBool("IsLedgeClimbing", true);
                yield return new WaitForSeconds(playerConfig.GetClimbExitTime());
                theAnimator.SetBool("IsLedgeClimbing", false);
                transform.position = new Vector3(transform.position.x + playerConfig.GetWallExit().x, transform.position.y + playerConfig.GetWallExit().y, transform.position.z);
            }
        }
        Debug.Log("Leaving climb and going to switch statement");
        leavingClimb = false;
        SwitchCheckIfMoving();
    }

    //WALL SLIDING AREA
    private bool IsWalled()
    {
        if (isGrounded || currentState.GetType() == playerSwimState.GetType()
            || currentState.GetType() == playerClimbState.GetType()
            || currentState.GetType() == playerDashState.GetType())
        {
            playerInfo.wallSlideRight = false;
            playerInfo.wallSlideLeft = false;
            return false;
        }

        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallSlideDist, playerConfig.GetClimbMask());
        if (hitRight && playerInfo.moveInput.x > 0.05)
        {
            playerInfo.wallSlideRight = true;
            playerInfo.wallSlideLeft = false;
            return true;
        }

        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallSlideDist, playerConfig.GetClimbMask());

        if (hitLeft && playerInfo.moveInput.x < -0.05)
        {
            playerInfo.wallSlideRight = false;
            playerInfo.wallSlideLeft = true;
            return true;
        }

        playerInfo.wallSlideRight = false;
        playerInfo.wallSlideLeft = false;
        return false;
    }

    void WallSlide()
    {
        //Verify player's current state
        if (IsWalled())
        {
            if (wallCheckTimer <= 0)
            {
                isWallSliding = true;
                SwitchState(playerWallSlideState);
            }
        }
        else
        {
            //Debug.Log("Is wall left hit? " + isWallLeft + " Is wall right hit? " + isWallRight);
            if (currentState.GetType() == playerWallSlideState.GetType())
            {
                wallCheckTimer = wallCheckCooldown;
                ResetCoyoteTimer();
                SwitchState(idleState);
            }
            isWallSliding = false;
        }
    }

    // DASH AREA
    public void OnDash(InputAction.CallbackContext context)
    {
        if (!isAlive || !context.started) { return; }
        if (!playerInfo.CanWeDash() || playerInfo.MovementLocked()) { return; }
        switch (currentState)
        {
            case SwimmingState:
                Debug.Log("Water Dashing");
                StartCoroutine(WaterDash());
                break;
            default:
                Debug.Log("Dashing");
                StartCoroutine(EightWayDash());
                break;
        }
    }

    private IEnumerator EightWayDash()
    {
        isDashing = true;
        playerInfo.SetCanDash(false);
        SwitchState(playerDashState);
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        SwitchCheckIfMoving();
    }

    private IEnumerator WaterDash()
    {
        isDashing = true;
        playerInfo.SetCanDash(false);
        SwitchState(playerWaterDashState);
        yield return new WaitForSeconds(playerConfig.GetWaterDashTime());
        isDashing = false;

        //Should check if we're still in water
        if (bodyCollider.IsTouchingLayers(LayerMask.GetMask("Water")))
        {
            SwitchState(playerSwimState);
        }
        else
        {
            SwitchCheckIfMoving();
        }
        
        playerInfo.SetCanDash(true);
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            PauseGame.Invoke();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!isAlive) { return; }

        //DEE One issue is that it isn't discerning by mask, ends up collecting templates 
        RaycastHit2D[] listOfObjects = Physics2D.CircleCastAll((Vector2)transform.position, collisionRadius, new Vector2(0, 0), interactMask);

        if (listOfObjects.Length > 0 && context.started)
        {
            foreach (RaycastHit2D rayHit in listOfObjects)
            {
                if (rayHit.transform.GetComponent<IInteractable>())
                {
                    rayHit.transform.GetComponent<IInteractable>().ChangeState();
                }
            }
        }
        else if (listOfObjects.Length > 0 && context.canceled)
        {
            foreach (RaycastHit2D rayHit in listOfObjects)
            {
                if (rayHit.transform.GetComponent<IHold>())
                {
                    rayHit.transform.GetComponent<IHold>().ChangeState();
                }
            }
        }
    }

    public void OnMeow(InputAction.CallbackContext context)
    {
        //Check if playing, if so return
        if (!isAlive || playerAudio.isPlaying) return;

        //Verify context has started
        if (context.started)
        {
            playerAudio.PlayOneShot(playerConfig.GetMeow());
            if(currentState.GetType() == idleState.GetType())
            {
                theAnimator.SetTrigger("MeowTrigger");
            }
        }
    }

    private void FlipSprite()
    {
        if (currentState.GetType() == playerSwimState.GetType())
        {
            return;
        }
        if (playerInfo.wallJumpLeft || playerInfo.climbRight || playerInfo.wallSlideRight)
        {
            transform.localScale = new Vector3(1, 1, 1);
            return;
        }
        if (playerInfo.wallJumpRight || playerInfo.climbLeft || playerInfo.wallSlideLeft)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            return;
        }

        if (playerInfo.GetMoveInput().x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            return;
        }

        if (playerInfo.GetMoveInput().x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            return;
        }
    }

    private void CreateDust()
    {
        dust.Play();
    }

    public void ZeroOutJumpingVars()
    {
        coyoteTimeCounter = 0f;
        inAirBuffer = 0f;
        jumpingBuffer = 0f;
    }

    public void CollisionCheck()
    {
        if (bodyCollider.IsTouchingLayers(LayerMask.GetMask("Hazards")))
        {
            Die();
        }
        if (headCollider.IsTouchingLayers(LayerMask.GetMask("Water")))
        {
            SwitchWaterState();
        }
    }

    private void Die()
    {
        isAlive = false;
        SwitchState(deathState);
        StartCoroutine(PlayerDeath());
    }

    IEnumerator PlayerDeath()
    {
        yield return new WaitForSeconds(0.1f);
        //Want to pause player in the air or show explosion
        GameSessionManager.instance.ProcessPlayerDeath();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Water check
        if (LayerMask.LayerToName(collision.gameObject.layer).Equals("Water") && !bodyCollider.IsTouchingLayers(LayerMask.GetMask("Water")))
        {
            if(currentState.GetType() != playerWaterDashState.GetType()){
                theRigidbody.velocity = new Vector2(0, 3.5f);
                theRigidbody.AddForce(new Vector2(theRigidbody.velocity.x, theRigidbody.velocity.y * waterExit), ForceMode2D.Force);
            }
            SwitchCheckIfMoving();            
        }
    }
}
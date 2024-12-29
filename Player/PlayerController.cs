using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : IController
{
    //DEEM Keep here, consider creating parent class for this one
    [SerializeField] PlayerInfo playerInfo;

    //Stores player movement related params
    private PlayerSM stateMachine;

    //DEEM Get rid of this and just replace with regular checks
    protected bool isDashing = false;

    [SerializeField] PlayerInput playerInput;
    InputAction moveVector;

    [Header("Ground Check")]
    [SerializeField] GameObject frontFoot;
    [SerializeField] GameObject backFoot;

    //DEEM Move these to parent class
    [SerializeField] private BoxCollider2D headCollider;
    [SerializeField] ParticleSystem dust;

    [Header("Jump Vars")]
    private float coyoteTimeCounter;

    [Header("Climb Vars")]
    //DEEM Do a check like with moveinput?
    private bool continousClimbCheck = false;

    [Header("WallSliding Vars")]
    private float wallCheckTimer = 0f;
    private float wallCheckCooldown = 0.3f;
    private bool isWallSliding = false;
    
    [Header("Dash Vars")]
    private float dashTime = 0.2f;

    [Header("Pause Menu")]
    [SerializeField] public UnityEvent PauseGame;

    //Climb time
    [SerializeField] float climbTime = 10f;
    private float currentTimeClimbTime = 1f;
    private bool leavingClimb = false;


    private void Awake()
    {
        LoadInConfig();
        spriteRenderer = GetComponent<SpriteRenderer>();
        theAnimator = GetComponent<Animator>();
        theRigidbody = GetComponent<Rigidbody2D>();
    }

    void LoadInConfig()
    {
        collisionRadius = playerConfig.GetCollisionRadius();
        wallCorrectionAmount = playerConfig.GetWallCorrection();
        gravityModifier = playerConfig.GetGravityModifer();
        inAirBuffer = playerConfig.GetAirTimer();
        jumpingBuffer = playerConfig.GetJumpTimer();
        wallSlideSeperation = playerConfig.GetWallSlideSeparation();
        wallJumpSpeed = playerConfig.GetWallJumpSpeed();
    }

    void Start()
    {
        GameSessionManager.instance.UpdateLastSave();
        stateMachine = new PlayerSM();
        gravityScaleAtStart = theRigidbody.gravityScale;
        playerInfo.SetWallJumpSpeed(wallJumpSpeed);
        currentTimeClimbTime = climbTime;
        playerInfo.SetClimbTime(climbTime);
        PlayerEvents.instance.onEndClimb += LeaveClimb;
        PlayerEvents.instance.onClimbTimeEnd += FallOutOfClimb;
        PlayerEvents.instance.onResetJump += ZeroOutJumpingVars;
        originalColor = spriteRenderer.color;
        ReactivateFootColliders();
        stateMachine.SwitchState(PlayerState.Idle, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
    }

    private void OnDestroy()
    {
        PlayerEvents.instance.onEndClimb -= LeaveClimb;
        PlayerEvents.instance.onClimbTimeEnd -= FallOutOfClimb;
        PlayerEvents.instance.onResetJump -= ZeroOutJumpingVars;
    }

    void Update()
    {
        if (!isAlive)
        {
            if (stateMachine.GetCurrentState().GetStateType() != PlayerState.NotAlive)
            {
                ReactivateFootColliders();
                stateMachine.SwitchState(PlayerState.NotAlive, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
            }
            return;
        }

        if (CheckGround())
        {
            if (stateMachine.GetCurrentState().GetStateType() == PlayerState.Jumping && inAirBuffer > 0.05)
            {
                inAirBuffer -= Time.deltaTime;
                return;
            }
            else if (stateMachine.GetCurrentState().GetStateType() == PlayerState.Climbing)
            {
                continousClimbCheck = false;
            }
            else
            {
                if (stateMachine.GetCurrentState().GetStateType() == PlayerState.Jumping
                    || stateMachine.GetCurrentState().GetStateType()  == PlayerState.WallJumping
                    || stateMachine.GetCurrentState().GetStateType() == PlayerState.Falling)
                {
                    SwitchCheckIfMoving();
                    CreateDust();
                }
                ResetCoyoteTimer();
                playerInfo.SetCanDash(true);
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
        stateMachine.UpdateState(this.gameObject, this.theRigidbody, this.theAnimator, this.playerInfo);
    }

    private void FixedUpdate()
    {
        stateMachine.UpdatePhysics(this.gameObject, theRigidbody, playerInfo);

        //DEEM Maybe move to each applicable state
        if (!isGrounded && stateMachine.GetCurrentState().GetStateType() != PlayerState.Dashing 
            && stateMachine.GetCurrentState().GetStateType() != PlayerState.WaterDashing 
            && !playerInfo.climbLeft
            && !playerInfo.climbRight)
        {
            theRigidbody.velocity += Vector2.up * Physics2D.gravity.y * (gravityModifier) * Time.fixedDeltaTime;
        }
    }

    private void ContextualChecks()
    {
        //Unfortuntally the unity trigger detection is not reliable enough so we to brute force a raycast check each frame
        CollisionCheck();

        //Checking if player is idle, running, or falling
        //Player input is required as long as the player is moving in that direction
        //Feel like I am missing something simple with the new input system
        MoveCheck();

        //Check if we're near a wall each frame
        WallSlide();

        //Should consider updating yet input system can be annoying to reimplement
        if (continousClimbCheck)
        {
            ClimbCheck();
        }
    }

    private void SwitchCheckIfMoving()
    {
        ReactivateFootColliders();
        if (isGrounded)
        {
            if (playerInfo.moveInput.x > 0.05 || playerInfo.moveInput.x < -0.05)
            {
                stateMachine.SwitchState(PlayerState.Running, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
            }
            else
            {
                stateMachine.SwitchState(PlayerState.Idle, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
            }
        }
        else
        {
            stateMachine.SwitchState(PlayerState.Falling, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
        }
    }

    private void SwitchWaterState()
    {
        switch (stateMachine.GetCurrentState().GetStateType())
        {
            case PlayerState.WaterDashing:
                break;
            default:
                stateMachine.SwitchState(PlayerState.Swimming, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
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

    private void MoveCheck()
    {
        moveVector = playerInput.actions["move"];
        OnActive(moveVector);
    }

    private void OnActive(InputAction context)
    {
        if (!isAlive || playerInfo.MovementLocked()) { return; }

        playerInfo.moveInput = context.ReadValue<Vector2>();
        playerInfo.lastMoveInput = playerInfo.moveInput;

        switch (stateMachine.GetCurrentState().GetStateType())
        {
            case PlayerState.Idle:
            case PlayerState.Running:
            case PlayerState.Falling:
                SwitchCheckIfMoving();
                break;
            default:
                break;
        }
    }

    //JUMP AREA
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!isAlive || isDashing || playerInfo.MovementLocked()) { return; }
        if (context.started && coyoteTimeCounter > 0f && !isWallSliding)
        {
            wallCheckTimer = wallCheckCooldown;
            ResetJumpBuffers();

            //DEE Testing for a little boost in x direction -> original value will have us just remove the moveInput.x == 0 check
            if (playerInfo.moveInput.x > 0.05 || playerInfo.moveInput.x < -0.05)
            {
                theRigidbody.AddForce(new Vector2(theRigidbody.velocity.x * 25f, theRigidbody.velocity.y), ForceMode2D.Force);
            }
            CreateDust();
            ReactivateFootColliders();
            stateMachine.SwitchState(PlayerState.Jumping, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
        }
        else if (context.canceled && !playerInfo.isContextCanceled() && stateMachine.GetCurrentState().GetStateType() == PlayerState.Jumping && jumpingBuffer >= 0)
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
            ReactivateFootColliders();
            stateMachine.SwitchState(PlayerState.WallJumping, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
        }
    }

    private void ResetJumpBuffers()
    {
        inAirBuffer = playerConfig.GetAirTimer();
        jumpingBuffer = playerConfig.GetJumpTimer();
    }

    private void JumpingTimer()
    {
        if (stateMachine.GetCurrentState().GetStateType() == PlayerState.Jumping)
        {
            inAirBuffer -= Time.deltaTime;
            jumpingBuffer -= Time.deltaTime;
        }
    }

    private void ResetCoyoteTimer()
    {
        coyoteTimeCounter = playerConfig.GetCoyoteTimer();
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

    public void OnClimb(InputAction.CallbackContext context)
    {
        if (!isAlive || playerInfo.GetClimbTime() <= 0 || playerInfo.MovementLocked()) { continousClimbCheck = false; return; }

        //DEE test
        if (context.started && stateMachine.GetCurrentState().GetStateType() != PlayerState.Climbing)
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

    //CLIMB AREA
    private void ClimbingTimer()
    {
        if (stateMachine.GetCurrentState().GetStateType() == PlayerState.Climbing)
        {
            if (currentTimeClimbTime > 0)
            {
                currentTimeClimbTime -= Time.deltaTime;

                //Adjust color of sprite based around current time and mulitply it by 0.01 to increment slowly
                adjustedColor.b = (15 * currentTimeClimbTime) * 0.01f;
                adjustedColor.g = (15 * currentTimeClimbTime) * 0.01f;
                spriteRenderer.color = adjustedColor;
                playerInfo.SetClimbTime(currentTimeClimbTime);
            }
        }
    }

    private void ResetClimbTime()
    {
        currentTimeClimbTime = climbTime;
        playerInfo.SetClimbTime(currentTimeClimbTime);
        spriteRenderer.color = originalColor;
        adjustedColor = originalColor;
    }

    public IEnumerator PhaseIntoClimb(bool pClimbLeft, bool pClimbRight)
    {
        theRigidbody.gravityScale = 0f;
        theRigidbody.velocity = new Vector2(0, 0);
        yield return new WaitForSeconds(0.1f);
        playerInfo.climbLeft = pClimbLeft;
        playerInfo.climbRight = pClimbRight;
        continousClimbCheck = false;
        stateMachine.SwitchState(PlayerState.Climbing, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
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
        ReactivateFootColliders();
        SwitchCheckIfMoving();
    }

    //WALL SLIDING AREA
    private bool IsWalled()
    {
        if (isGrounded || stateMachine.GetCurrentState().GetStateType() == PlayerState.Swimming
            || stateMachine.GetCurrentState().GetStateType() == PlayerState.Climbing
            || stateMachine.GetCurrentState().GetStateType() == PlayerState.Dashing)
        {
            playerInfo.wallSlideRight = false;
            playerInfo.wallSlideLeft = false;
            return false;
        }

        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallSlideSeperation, playerConfig.GetClimbMask());
        if (hitRight && playerInfo.moveInput.x > 0.05)
        {
            playerInfo.wallSlideRight = true;
            playerInfo.wallSlideLeft = false;
            return true;
        }

        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallSlideSeperation, playerConfig.GetClimbMask());

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
                ReactivateFootColliders();
                stateMachine.SwitchState(PlayerState.WallSliding, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
            }
        }
        else
        {
            //Debug.Log("Is wall left hit? " + isWallLeft + " Is wall right hit? " + isWallRight);
            if (stateMachine.GetCurrentState().GetStateType() == PlayerState.WallSliding)
            {
                wallCheckTimer = wallCheckCooldown;
                ResetCoyoteTimer();
                ReactivateFootColliders();
                stateMachine.SwitchState(PlayerState.Idle, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
            }
            isWallSliding = false;
        }
    }

    // DASH AREA
    public void OnDash(InputAction.CallbackContext context)
    {
        if (!isAlive || !context.started) { return; }
        if (!playerInfo.CanWeDash() || playerInfo.MovementLocked()) { return; }
        switch (stateMachine.GetCurrentState().GetStateType())
        {
            case PlayerState.Swimming:
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
        ReactivateFootColliders();
        stateMachine.SwitchState(PlayerState.Dashing, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        SwitchCheckIfMoving();
    }

    private IEnumerator WaterDash()
    {
        isDashing = true;
        playerInfo.SetCanDash(false);
        ReactivateFootColliders();
        stateMachine.SwitchState(PlayerState.WaterDashing, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
        yield return new WaitForSeconds(playerConfig.GetWaterDashTime());
        isDashing = false;

        //Should check if we're still in water
        if (bodyCollider.IsTouchingLayers(LayerMask.GetMask("Water")))
        {
            stateMachine.SwitchState(PlayerState.Swimming, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
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
        if (!isAlive || characterAudio.isPlaying) return;

        //Verify context has started
        if (context.started)
        {
            characterAudio.PlayOneShot(playerConfig.GetMeow());
            if (stateMachine.GetCurrentState().GetStateType() == PlayerState.Idle)
            {
                theAnimator.SetTrigger("MeowTrigger");
            }
        }
    }

    private void FlipSprite()
    {
        if (stateMachine.GetCurrentState().GetStateType() == PlayerState.Swimming)
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
    
    protected override void Die()
    {
        isAlive = false;
        stateMachine.SwitchState(PlayerState.NotAlive, this.gameObject, this.theRigidbody, this.theAnimator, this.playerConfig, this.playerInfo);
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
            if (stateMachine.GetCurrentState().GetStateType() != PlayerState.WaterDashing)
            {
                theRigidbody.velocity = new Vector2(0, 3.5f);
                theRigidbody.AddForce(new Vector2(theRigidbody.velocity.x, theRigidbody.velocity.y * waterExit), ForceMode2D.Force);
            }
            ReactivateFootColliders();
            SwitchCheckIfMoving();
        }
    }

    private void ReactivateFootColliders()
    {
        frontFoot.SetActive(true);
        backFoot.SetActive(true);
    }
}
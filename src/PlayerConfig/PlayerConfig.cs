using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Config")] //Create a new playerData object by right clicking in the Project Menu then Create/Player/Player Data and drag onto the player
public class PlayerConfig : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] public Vector2 wallExitKick = new Vector2(2f, 5f);
    [SerializeField] private Vector2 deathKick = new Vector2(0, 10f);
    [SerializeField] public float collisionRadius = 0.2f;
    [SerializeField] LayerMask climbLayers;

    [Header("Jump Vars")]
    [SerializeField] float jumpSpeed = 11f;
    [SerializeField] float gravityScale = 3f;
    [SerializeField] private float inAirTimer = 0.45f;
    [SerializeField] private float jumpingTimer = 0.4f;
    [SerializeField] private float coyoteTimer = 0.2f;
    [SerializeField] private float minCancelTimer = 0.35f;
    [SerializeField] private float cancelTimer = 0.2f; // set to private and inherit
    [SerializeField] float gravityModifier = 1.4f;
    [SerializeField] float inAirSpeed = 5f;
    [SerializeField] private float wallJumpBuffer = 0.2f;
    [SerializeField] private float wallJumpGrav = 2f;
    [SerializeField] private float wallJumpSpeed = 5f;
    private bool jumpContextCanceled;

    [Header("Climb Vars")]
    [SerializeField] float climbExitTime = 0.2f; // set to private and inherit
    [SerializeField] float wallCorrectionAmount = 0.45f;
    [SerializeField] float wallSlideDist = 0.5f;
    [SerializeField] float launchForce = 15f;
    public Vector2 rightOffset = new Vector2(1, 0);
    public Vector2 leftOffset = new Vector2(-1, 0);

    [Header("WallSliding Vars")]
    [SerializeField] float wallSlidingSpeed = 4f;
    [SerializeField] float wallCheckDistance = 0.45f;
    [SerializeField] float wallSlideSeperation = 0.5f;

    [Header("Dash Vars")]
    [SerializeField] float dashTime = 0.2f; //to remove
    [SerializeField] float dashCooldown = 2f; //to remove
    [SerializeField] float dashSpeed = 8f; //to remove

    [Header("Swimming Vars")]
    [SerializeField] float waterDashSpeed = 3f;
    [SerializeField] float waterDashTime = 0.4f;
    [SerializeField] float sinkSpeed = -1f;
    [SerializeField] float waterVelocity = -1f;
    [SerializeField] float waterJump = 1f;
    [SerializeField] float waterGravity = 1f;
    [SerializeField] float swimSpeed = 1f;

    [SerializeField] List<AudioClip> meowList;
    [SerializeField] AudioClip footstepSound;
    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public float GetJumpSpeed()
    {
        return jumpSpeed;
    }

    public float GetGravityModifer()
    {
        return gravityModifier;
    }

    public float GetInAirSpeed()
    {
        return inAirSpeed;
    }

    public void SetSpeed(float pSpeed)
    {
        moveSpeed = pSpeed;
    }

    public float GetWaterGravity()
    {
        return waterGravity;
    }

    public float GetAirTimer()
    {
        return inAirTimer;
    }

    public float GetJumpTimer()
    {
        return jumpingTimer;
    }

    public float GetMinCancelTimer()
    {
        return minCancelTimer;
    }

    public float GetCoyoteTimer()
    {
        return coyoteTimer;
    }

    public Vector2 GetWallExit()
    {
        return wallExitKick;
    }

    public float GetGravityScale()
    {
        return gravityScale;
    }

    public float GetWallSlidingSpeed()
    {
        return wallSlidingSpeed;
    }

    public float GetWallJumpBuffer()
    {
        return wallJumpBuffer;
    }

    public AudioClip GetMeow()
    {
        System.Random rnd = new System.Random();
        int num = rnd.Next(0, meowList.Count-1);
        Debug.Log("The size is " + meowList.Count);
        return meowList[num];

    }

    public Vector2 GetDeathKick()
    {
        return deathKick;
    }

    public float GetWallCorrection()
    {
        return wallCorrectionAmount;
    }

    public float GetLaunchForce()
    {
        return launchForce;
    }

    public float GetDashSpeed()
    {
        return dashSpeed;
    }

    public AudioClip GetFootstepSounds()
    {
        return footstepSound;
    }

    public float GetCollisionRadius()
    {
        return collisionRadius;
    }

    public float GetClimbExitTime()
    {
        return climbExitTime;
    }

    public float GetSwimSpeed()
    {
        return swimSpeed;
    }

    public float GetSinkSpeed()
    {
        return sinkSpeed;
    }

    public float GetWaterDashTime()
    {
        return waterDashTime;
    }

    public float GetWaterDashSpeed()
    {
        return waterDashSpeed;
    }

    public float GetWallJumpGravity()
    {
        return wallJumpGrav;
    }

    public LayerMask GetClimbMask()
    {
        return climbLayers;
    }

    public float GetWallSlideSeparation()
    {
        return wallSlideSeperation;
    }

    public float GetWallJumpSpeed()
    {
        return wallJumpSpeed;
    }
}

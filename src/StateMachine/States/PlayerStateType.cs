using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ariel.PlayerStates
{
    public enum PlayerStateType
    {
        Idle,
        NotAlive,
        Running,
        Climbing,
        Dashing,
        Crouching,
        Falling,
        Jumping,
        Swimming,
        WallJumping,
        WallSliding,
        WaterDashing
    }
}
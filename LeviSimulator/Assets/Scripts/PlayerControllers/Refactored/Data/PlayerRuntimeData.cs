using UnityEngine;
using PlayerControllers.Refactored.Core;

namespace PlayerControllers.Refactored.Data
{
    /// <summary>
    /// 玩家运行时数据
    /// </summary>
    public class PlayerRuntimeData
    {
        // 状态数据
        public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
        public PlayerState PreviousState { get; private set; } = PlayerState.Idle;
        
        // 移动数据
        public Vector3 Velocity { get; private set; }
        public Vector3 HorizontalVelocity => new Vector3(Velocity.x, 0, Velocity.z);
        public Vector3 VerticalVelocity => new Vector3(0, Velocity.y, 0);
        public float Speed => Velocity.magnitude;
        public float HorizontalSpeed => HorizontalVelocity.magnitude;
        
        // 输入数据
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public Vector3 MoveDirection { get; private set; }
        
        // 状态标志
        public bool IsGrounded { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsSliding { get; private set; }
        public bool IsGrappling { get; private set; }
        public bool IsWallRunning { get; private set; }
        
        // 地面信息
        public RaycastHit GroundHit { get; private set; }
        public Vector3 GroundNormal { get; private set; }
        public float GroundAngle { get; private set; }
        
        // 时间数据
        public float StateEnterTime { get; private set; }
        public float TimeSinceGrounded { get; private set; }
        public float TimeSinceJump { get; private set; }
        
        // 数据更新方法
        public void SetState(PlayerState newState)
        {
            if (CurrentState != newState)
            {
                PreviousState = CurrentState;
                CurrentState = newState;
                StateEnterTime = Time.time;
            }
        }
        
        public void SetVelocity(Vector3 velocity) => Velocity = velocity;
        public void SetMoveInput(Vector2 input) => MoveInput = input;
        public void SetLookInput(Vector2 input) => LookInput = input;
        public void SetMoveDirection(Vector3 direction) => MoveDirection = direction;
        public void SetGrounded(bool grounded) => IsGrounded = grounded;
        public void SetSprinting(bool sprinting) => IsSprinting = sprinting;
        public void SetCrouching(bool crouching) => IsCrouching = crouching;
        public void SetJumping(bool jumping) => IsJumping = jumping;
        public void SetSliding(bool sliding) => IsSliding = sliding;
        public void SetGrappling(bool grappling) => IsGrappling = grappling;
        public void SetWallRunning(bool wallRunning) => IsWallRunning = wallRunning;
        
        public void SetGroundInfo(RaycastHit hit, Vector3 normal, float angle)
        {
            GroundHit = hit;
            GroundNormal = normal;
            GroundAngle = angle;
        }
        
        public void UpdateTimers()
        {
            if (!IsGrounded)
                TimeSinceGrounded += Time.fixedDeltaTime;
            else
                TimeSinceGrounded = 0f;
                
            TimeSinceJump += Time.fixedDeltaTime;
        }
        
        public void SetJumpTime() => TimeSinceJump = 0f;
        
        public float GetTimeInCurrentState() => Time.time - StateEnterTime;
    }
}

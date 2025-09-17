using UnityEngine;
using PlayerControllers.Refactored.Core;
using Utilities;

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
        //玩家输入状态
        public bool PlayerPressingCrouch { get; set; } = false;
        // 移动数据
        public Vector3 Velocity { get; private set; }
        public Vector3 HorizontalVelocity => new Vector3(Velocity.x, 0, Velocity.z);
        public Vector3 VerticalVelocity => new Vector3(0, Velocity.y, 0);
        public float Speed => Velocity.magnitude;
        public float HorizontalSpeed => HorizontalVelocity.magnitude;
        // 滑墙相关数据
        public Vector3 WallNormal { get; private set; }
        // 冲刺相关数据
        public int DashCount { get; set; } = 0;
        public bool CanDash => DashCount >= 1;
        public void RecoverDash()=> DashCount +=1;
        public void ConsumeDash()=> DashCount -=1;
        public bool IsDashing { get; set; } = false;
        
        
        // 子弹时间相关数据
        public float BulletTimeEnergy { get; set; } = 1f;
        public bool IsInBulletTime { get; set; } = false;
        
        
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
        public void SetWallNormal(Vector3 normal)
        {
            WallNormal = normal;
        }

        public float GetWallSide(Transform transform)
        {
            if (!IsWallRunning)
            {
                return 0f;
            }
            Vector3 localNormal = transform.InverseTransformDirection(WallNormal);
            LogUtil.Log($"墙面法线本地坐标: {localNormal}", false);
            
            // 如果法线的x分量为负，则墙在右边，反之在左边
            return Mathf.Sign(localNormal.x);
        }

        public void ResetToDefault()
        {
            CurrentState = PlayerState.Idle;
            PreviousState = PlayerState.Idle;
            Velocity = Vector3.zero;
            DashCount = 0;
            IsDashing = false;
            BulletTimeEnergy = 1f;
            IsInBulletTime = false;
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
            MoveDirection = Vector3.zero;
            IsGrounded = false;
            IsSprinting = false;
            IsCrouching = false;
            IsJumping = false;
            IsSliding = false;
            IsGrappling = false;
            IsWallRunning = false;
            GroundHit = new RaycastHit();
            GroundNormal = Vector3.up;
            GroundAngle = 0f;
            StateEnterTime = Time.time;
            TimeSinceGrounded = 0f;
            TimeSinceJump = 0f;
            WallNormal = Vector3.zero;
            ResetSkillState();
        }
        public void ResetSkillState()
        {
            DashCount = 1;
            IsDashing = false;
            IsInBulletTime = false;
        }
    }
}

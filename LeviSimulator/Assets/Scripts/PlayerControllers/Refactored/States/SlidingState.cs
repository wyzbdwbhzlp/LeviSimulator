using UnityEngine;
using PlayerControllers.Refactored.Core;

namespace PlayerControllers.Refactored.States
{
    /// <summary>
    /// 滑铲状态
    /// </summary>
    public class SlidingState : PlayerStateBase
    {
        private float _slideStartTime;
        private Vector3 _slideDirection;
        
        public SlidingState(PlayerController controller) : base(controller) { }
        
        public override void Enter()
        {
            base.Enter();
            runtimeData.SetState(PlayerState.Sliding);
            runtimeData.SetSliding(true);
            runtimeData.SetCrouching(true);
            
            // 设置滑铲方向
            _slideDirection = runtimeData.MoveDirection;
            if (_slideDirection.magnitude < 0.1f)
            {
                // 如果没有输入方向，使用当前速度方向
                Vector3 velocity = movementSystem.Rigidbody.linearVelocity;
                _slideDirection = new Vector3(velocity.x, 0, velocity.z).normalized;
            }
            
            _slideStartTime = Time.time;
            
            // 降低碰撞体高度
            movementSystem.SetColliderHeight(config.CrouchingHeight);
            
            // 给予初始滑铲速度
            Vector3 slideVelocity = _slideDirection * config.SlideSpeed;
            slideVelocity.y = movementSystem.Rigidbody.linearVelocity.y;
            movementSystem.Rigidbody.linearVelocity = slideVelocity;
        }
        
        public override void Exit()
        {
            base.Exit();
            runtimeData.SetSliding(false);
        }
        
        protected override void CheckTransitions()
        {
            float slideTime = Time.time - _slideStartTime;
            
            // 检查滑铲是否结束
            bool slideEnded = slideTime > config.SlideDuration;
            
            // 时间限制
                
            // 速度过低
            if (runtimeData.HorizontalSpeed < config.MinSlideSpeed)
                slideEnded = true;
                
            // 停止蹲伏输入
            if (!runtimeData.IsCrouching)
                slideEnded = true;
            
            if (slideEnded)
            {
                if (runtimeData.IsCrouching)
                {
                    ChangeState(PlayerState.Crouching);
                    return;
                }
                // 检查头顶是否有障碍物
                if (!movementSystem.CheckCeiling())
                {
                    runtimeData.SetCrouching(false);
                    
                    if (runtimeData.MoveInput.magnitude > 0.1f)
                    {
                        if (runtimeData.IsSprinting&&!runtimeData.IsCrouching)
                        {
                            ChangeState(PlayerState.Running);
                            movementSystem.ResetCollider();
                        }
                        else if(!runtimeData.IsSprinting&&!runtimeData.IsCrouching)
                        {
                            ChangeState(PlayerState.Walking);
                            movementSystem.ResetCollider();
                        }
                    }
                    else
                    {
                        ChangeState(PlayerState.Idle);
                        //idle状态机Enter内已经执行了 movementSystem.ResetCollider();
                    }
                }
                else
                {
                    // 如果头顶有障碍物，转为蹲伏
                    ChangeState(PlayerState.Crouching);
                }
                return;
            }
            
            // 检查是否离开地面
            if (!runtimeData.IsGrounded)
            {
                ChangeState(PlayerState.Falling);
                return;
            }
        }
        
        protected override void HandleMovement()
        {
            // 滑铲减速
            Vector3 velocity = movementSystem.Rigidbody.linearVelocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            
            // 应用减速
            float deceleration = config.SlideDeceleration * Time.fixedDeltaTime;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, deceleration);
            
            // 保持滑铲方向
            if (horizontalVelocity.magnitude > config.MinSlideSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * Mathf.Max(horizontalVelocity.magnitude, config.MinSlideSpeed);
            }
            
            velocity.x = horizontalVelocity.x;
            velocity.z = horizontalVelocity.z;
            movementSystem.Rigidbody.linearVelocity = velocity;
        }
        
       
    }
}

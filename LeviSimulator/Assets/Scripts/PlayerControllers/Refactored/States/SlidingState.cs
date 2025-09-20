using System;
using UnityEngine;
using PlayerControllers.Refactored.Core;
using Utilities;

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
            Vector3 slideVelocity = _slideDirection * config.SlideStartBonusSpeed;
            slideVelocity.y = movementSystem.Rigidbody.linearVelocity.y;
           
            movementSystem.SetPlayerLinearVelocity(slideVelocity);
        }
        
        public override void Exit()
        {
            base.Exit();
            runtimeData.SetSliding(false);
        }
        
        protected override void CheckTransitions()
        {
         
            
            // 速度过低
            bool slideEnded = runtimeData.HorizontalSpeed < config.MinSlideActivationSpeed;
            
            // 时间限制
                
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
            Vector3 velocity = movementSystem.Rigidbody.linearVelocity;
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            
            // 如果水平速度太小，直接应用最小速度并返回
            if (horizontalVelocity.magnitude < 0.1f)
            {
                if (_slideDirection.magnitude > 0.1f)
                {
                    horizontalVelocity = _slideDirection * config.MinSlideActivationSpeed;
                }
                velocity.x = horizontalVelocity.x;
                velocity.z = horizontalVelocity.z;
                movementSystem.SetPlayerLinearVelocity(velocity);
                return;
            }
            
            // 获取当前移动方向和速度
            Vector3 currentMoveDirection = horizontalVelocity.normalized;
            float currentSpeed = horizontalVelocity.magnitude;
            
            // 基础减速度
            float effectiveDeceleration = config.SlideDeceleration;
            float speedChange = 0f;
            Vector3 finalDirection = currentMoveDirection; // 最终的滑铲方向
            
            // 如果在斜坡上
            if (runtimeData.IsOnSlope)
            {
                Vector3 slopeDirection = runtimeData.SlopeDirection; // 斜坡向下方向
                float slopeAngle = MathF.Abs(runtimeData.SlopeRotationZ);
                
                // 计算当前方向与斜坡方向的夹角和点积
                float slopeDot = Vector3.Dot(currentMoveDirection, slopeDirection);
                
                // 计算方向对齐程度 (0=完全垂直, 1=完全对齐)
                float directionAlignment = Mathf.Abs(slopeDot);
                
                // 基于中性角度计算斜坡效果
                float neutralAngle = config.NeutralSlopeAngle;
                float angleFromNeutral = slopeAngle - neutralAngle;
                
                if (slopeDot > 0.1f) // 大致沿着斜坡向下的方向
                {
                    // 方向纠正：将滑铲方向逐渐纠正到斜坡方向
                    float correctionStrength = 3f * Time.fixedDeltaTime; // 纠正强度
                    finalDirection = Vector3.Slerp(currentMoveDirection, slopeDirection, correctionStrength).normalized;
                    
                    // 只有当方向对齐度足够高时才给予斜坡增益
                    float minAlignment = 0.7f; // 最小对齐度要求（约45度内）
                    if (directionAlignment >= minAlignment)
                    {
                        // 根据对齐度调整增益效果
                        float alignmentMultiplier = (directionAlignment - minAlignment) / (1f - minAlignment);
                        
                        if (Mathf.Abs(angleFromNeutral) < 1f) // 接近中性角度时
                        {
                            // 在中性坡时维持原速，取消减速度
                            effectiveDeceleration *= (1f - alignmentMultiplier);
                        }
                        else if (angleFromNeutral > 0f) // 比中性角度更陡的下坡
                        {
                            if (config.AccelerateOnSteepSlope)
                            {
                                // 在陡坡上加速，效果随对齐度衰减
                                float accelerationBonus = angleFromNeutral * config.SlopeAngleBoostFactor * 0.1f * alignmentMultiplier;
                                speedChange += accelerationBonus * Time.fixedDeltaTime;
                            }
                            // 减少减速度，效果随对齐度衰减
                            float decelerationReduction = Mathf.Clamp01(angleFromNeutral / 45f) * config.SlopeAngleBoostFactor * alignmentMultiplier;
                            effectiveDeceleration *= (1f - decelerationReduction);
                        }
                        else // 比中性角度更缓的下坡
                        {
                            // 轻微加速，效果随对齐度衰减
                            float minorAcceleration = Mathf.Abs(angleFromNeutral) * config.SlopeAngleBoostFactor * 0.05f * alignmentMultiplier;
                            speedChange += minorAcceleration * Time.fixedDeltaTime;
                        }
                    }
                    // 如果对齐度不够，只应用正常减速度，不给予任何增益
                }
                else if (slopeDot < -0.1f) // 沿着斜坡向上滑行
                {
                    // 向上滑行时，方向纠正到斜坡向上方向
                    Vector3 slopeUpDirection = runtimeData.SlopeUpDirection;
                    float correctionStrength = 2f * Time.fixedDeltaTime; // 向上时纠正稍慢
                    finalDirection = Vector3.Slerp(currentMoveDirection, slopeUpDirection, correctionStrength).normalized;
                    
                    // 向上滑行总是增加减速度
                    float upwardPenalty = slopeAngle * config.SlopeAngleDecelerationFactor * 0.1f;
                    effectiveDeceleration *= (1f + upwardPenalty);
                    
                    // 额外的重力阻力
                    float gravityResistance = slopeAngle * 0.15f * Time.fixedDeltaTime;
                    speedChange -= gravityResistance;
                }
                else // 与斜坡方向大致垂直
                {
                    // 垂直于斜坡时，轻微向斜坡方向纠正
                    float correctionStrength = 1.5f * Time.fixedDeltaTime;
                    finalDirection = Vector3.Slerp(currentMoveDirection, slopeDirection, correctionStrength).normalized;
                    
                    // 不给予任何斜坡增益，应用正常减速度
                }
            }
            
            // 计算新的目标速度
            float targetSpeed = currentSpeed + speedChange - (effectiveDeceleration * Time.fixedDeltaTime);
            
            // 限制在最小和最大滑铲速度之间
            targetSpeed = Mathf.Clamp(targetSpeed, config.MinSlideActivationSpeed, config.MaxSlideSpeed);
            
            // 应用新的水平速度（使用纠正后的方向）
            horizontalVelocity = finalDirection * targetSpeed;
            
            velocity.x = horizontalVelocity.x;
            velocity.z = horizontalVelocity.z;
            movementSystem.SetPlayerLinearVelocity(velocity);
            
            LogUtil.Log("本帧HandleMovement各临时参数值为：" +
                        $"\n currentMoveDirection: {currentMoveDirection}" +
                        $"\n slopeDirection: {runtimeData.SlopeDirection}" +
                        $"\n angleBetween: {Vector3.Angle(currentMoveDirection, runtimeData.SlopeDirection):F1}°" +
                        $"\n directionAlignment: {Mathf.Abs(Vector3.Dot(currentMoveDirection, runtimeData.SlopeDirection)):F2}" +
                        $"\n finalDirection: {finalDirection}" +
                        $"\n currentSpeed: {currentSpeed:F1}" +
                        $"\n effectiveDeceleration: {effectiveDeceleration:F1}" +
                        $"\n speedChange: {speedChange:F2}" +
                        $"\n targetSpeed: {targetSpeed:F1}");
        }
        
       
    }
}

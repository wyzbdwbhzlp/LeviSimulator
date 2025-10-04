using System;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace PlayerControllers.Refactored.Data
{ 
    /// <summary>
    /// 玩家移动配置
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "Player/Movement Config")]
    public class PlayerMovementConfig : ScriptableObject
    {



        [BoxGroup("镜头控制")]
        [LabelText("FOV")]
        [SerializeField] private float fov = 60f;
        [LabelText("鼠标灵敏度")]
        [SerializeField] private float mouseSensitivity = 100f; // 提高默认灵敏度适合FPS
        [LabelText("平滑系数")]
        [SerializeField] private float smoothness = 10f;
        [LabelText("镜头翻滚速度")]
        [SerializeField] private float cameraTiltSpeed = 5f;
        [LabelText("最大镜头翻滚角度")]
        [SerializeField] private float maxCameraTiltAngle = 15f;
        [LabelText("滑墙启用视角修正阈值")]
        [SerializeField] private float wallRunAngleThreshold = 80f;
        [Title("视角辅助瞄准设定")]
        [SerializeField] [LabelText("辅助瞄准强度")] [Range(0f, 1f)]
        private float defaultAssistStrength = 0.103f;
        [SerializeField] [LabelText("辅助瞄准持续时间(-1为持续)")] [MinValue(-1f)]
        private float defaultAssistDuration = -1f;
        [SerializeField] [LabelText("辅助瞄准响应速度")] [MinValue(0.1f)]
        private float assistResponseSpeed = 2f;
        
        
        [BoxGroup("地面移动")]
        [LabelText("行走速度")]
        [SerializeField] private float walkSpeed = 5f;
        [LabelText("奔跑速度")]
        [SerializeField] private float runSpeed = 8f;
        [LabelText("蹲伏速度")]
        [SerializeField] private float crouchSpeed = 2f;
        [LabelText("加速度")]
        [SerializeField] private float acceleration = 10f;
        [LabelText("减速度")]
        [SerializeField] private float deceleration = 8f;

        [BoxGroup("空中移动")]
        [LabelText("空中加速度")]
        [SerializeField] private float airAcceleration = 5f;
        [LabelText("最大空中速度")]
        [SerializeField] private float maxAirSpeed = 6f;
        [LabelText("跳跃力度")]
        [SerializeField] private float jumpForce = 8f;
        [LabelText("重力")]
        [SerializeField] private float gravity = 20f;
        
        [BoxGroup("滑铲")]
        [LabelText("启动滑铲时候的额外奖励速度")]
        [SerializeField] private float slideStartBonusSpeed = 12f;
        [LabelText("滑铲最高可达速度")]
        [SerializeField]private float maxSlideSpeed = 15f;
        [LabelText("滑铲减速度")]
        [SerializeField] private float slideDeceleration = 5f;
        [LabelText("滑铲恰好不加速也不减速的角度")]
        [SerializeField][Range(0f,90f)]private float neutralSlopeAngle=25f;
        [LabelText("是否在过陡坡(超过neutralSlopeAngle)时继续加速")]
        [SerializeField] private bool accelerateOnSteepSlope = false;
        [LabelText("坡度角度加速因子")]
        [SerializeField] private float slopeAngleBoostFactor=0.5f;
        [LabelText("坡度角度减速因子")]
        [SerializeField] private float slopeAngleDecelerationFactor=0.5f;
        [LabelText("最小启动滑铲速度")]
        [SerializeField] private float minSlideActivationSpeed = 3f;
      
        
        [BoxGroup("滑墙")]
        [LabelText("滑墙检测距离")]
        [SerializeField] private float wallMaxDistance = 1f;
        [LabelText("墙面层级")]
        [SerializeField] private LayerMask wallLayerMask = 1;
        [LabelText("滑墙速度倍率")]
        [SerializeField] private float wallSpeedMultiplier = 1.2f;
        [LabelText("滑墙最小离地高度")]
        [SerializeField] private float minimumHeightForWallRun = 1.2f;
        [LabelText("滑墙启动速度")]
        [SerializeField] private float wallRunThresholdSpeed = 8f;
        [LabelText("滑墙最低维持速度")]
        [SerializeField] private float wallRunMinimumSpeed = 2f;
        [LabelText("滑墙摩擦力")]
        [SerializeField] private float wallFriction = 0.5f;
        [LabelText("滑墙跳跃力")]
        [SerializeField] private float wallJumpForce = 4f;
        [LabelText("滑墙跳跃反弹力")]
        [SerializeField] private float wallJumpBounceForce = 2f;
        
        [BoxGroup("地面检测")]
        [LabelText("地面检测距离")]
        [SerializeField] private float groundCheckDistance;
        [LabelText("地面检测半径")]
        [SerializeField] private float groundCheckRadius;
        [LabelText("地面层级")]
        [SerializeField] private LayerMask groundLayerMask;
        [LabelText("滑坡层级")]
        [SerializeField] private LayerMask slopeLayerMask;
        
        [BoxGroup("碰撞体")]
        [LabelText("蹲伏高度")] 
        [SerializeField] private float crouchingHeight = 1f;
        [BoxGroup("物理材质")]
        [SerializeField] private PhysicsMaterial playerInAirMaterial;
        [SerializeField] private PhysicsMaterial playerOnGroundMaterial;
        
        // 公共访问器
        public float MouseSensitivity => mouseSensitivity;
        public float Smoothness => smoothness;
        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;
        public float CrouchSpeed => crouchSpeed;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float AirAcceleration => airAcceleration;
        public float MaxAirSpeed => maxAirSpeed;
        public float JumpForce => jumpForce;
        public float Gravity => gravity;
        public float SlideStartBonusSpeed => slideStartBonusSpeed;
        public float SlideDeceleration => slideDeceleration;
        public float MinSlideActivationSpeed => minSlideActivationSpeed;
        public float WallMaxDistance => wallMaxDistance;
        public LayerMask WallLayerMask => wallLayerMask;
        public float WallSpeedMultiplier => wallSpeedMultiplier;
        public float MinimumHeightForWallRun => minimumHeightForWallRun;
        public float WallRunThresholdSpeed => wallRunThresholdSpeed;
        public float WallRunMinimumSpeed => wallRunMinimumSpeed;
        public float WallFriction => wallFriction;
        public float WallJumpForce => wallJumpForce;
        public float WallJumpBounceForce => wallJumpBounceForce;
        public float GroundCheckDistance => groundCheckDistance;
        public float GroundCheckRadius => groundCheckRadius;
        public LayerMask GroundLayerMask => groundLayerMask;
        public float CrouchingHeight => crouchingHeight;
        public PhysicsMaterial PlayerInAirMaterial => playerInAirMaterial;
        public PhysicsMaterial PlayerOnGroundMaterial => playerOnGroundMaterial;
        public float CameraTiltSpeed => cameraTiltSpeed;
        public float MaxCameraTiltAngle => maxCameraTiltAngle;
        public float WallRunAngleThreshold => wallRunAngleThreshold;
        public float DefaultAssistStrength => defaultAssistStrength;
        public float DefaultAssistDuration => defaultAssistDuration;
        public float AssistResponseSpeed => assistResponseSpeed;
        public LayerMask SlopeLayerMask => slopeLayerMask;
        public float Fov => fov;

        public float MaxSlideSpeed => maxSlideSpeed;
        
        public float NeutralSlopeAngle => neutralSlopeAngle;

        public bool AccelerateOnSteepSlope => accelerateOnSteepSlope;

        public float SlopeAngleBoostFactor => slopeAngleBoostFactor;

        public float SlopeAngleDecelerationFactor => slopeAngleDecelerationFactor;
        public float SetMouseSensitivity {
            set => mouseSensitivity=value;
        }
        public float SetFOV {
            set => fov=value;
        }
    }
}

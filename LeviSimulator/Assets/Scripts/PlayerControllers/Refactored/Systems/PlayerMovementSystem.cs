using System.Collections;
using System.Collections.Generic;
using ExternPropertyAttributes;
using UnityEngine;
using PlayerControllers.Refactored.Core;
using PlayerControllers.Refactored.Data;
using Utilities;

namespace PlayerControllers.Refactored.Systems
{
    /// <summary>
    /// 玩家移动系统
    /// </summary>
    public class PlayerMovementSystem : MonoBehaviour, IPlayerSystem
    {
        [Header("依赖组件")]
        [SerializeField] private Rigidbody playerRigidbody;
        [SerializeField] private CapsuleCollider playerCollider;
        [SerializeField] private Transform cameraTransform;

        [Header("配置")]
        [SerializeField] private PlayerMovementConfig config;

        private PlayerController _playerController;
        private PlayerRuntimeData _runtimeData;
        private Vector3 _originalColliderCenter;
        private float _originalColliderHeight;
        private Queue<IEnumerator> _dashCoolDownQueue = new Queue<IEnumerator>();
        private bool _isDashCooldownRunning = false;
        
        public bool IsEnabled { get; set; } = true;
        public Rigidbody Rigidbody => playerRigidbody;
        public CapsuleCollider PlayerCollider => playerCollider;
        public PlayerMovementConfig Config => config;

        public void Initialize(PlayerController playerController, ScriptableObject playerConfig)
        {
            _playerController = playerController;
            _runtimeData = playerController.RuntimeData;

            // 获取组件引用
            if (playerRigidbody == null)
                playerRigidbody = GetComponent<Rigidbody>();
            if (playerCollider == null)
                playerCollider = GetComponent<CapsuleCollider>();
            if (cameraTransform == null)
                cameraTransform = Camera.main?.transform;

            // 保存原始碰撞体参数
            _originalColliderCenter = playerCollider.center;
            _originalColliderHeight = playerCollider.height;

            // 订阅输入事件
            PlayerInputEvents.OnMoveInput += HandleMoveInput;
            PlayerInputEvents.OnSprintPressed += HandleSprintPressed;
            PlayerInputEvents.OnSprintReleased += HandleSprintReleased;

            // 应用配置
            this.config = playerConfig as PlayerMovementConfig;

        }

        public void Update()
        {
            if (!IsEnabled) return;

            UpdateMovementDirection();
        }

        public void FixedUpdate()
        {
            if (!IsEnabled) return;

            UpdateGroundDetection();
            _runtimeData.SetVelocity(playerRigidbody.linearVelocity);
            _runtimeData.UpdateTimers();
        }

        private void HandleMoveInput(Vector2 input)
        {
            _runtimeData.SetMoveInput(input);
        }

        private void HandleSprintPressed()
        {
            _runtimeData.SetSprinting(true);
        }

        private void HandleSprintReleased()
        {
            _runtimeData.SetSprinting(false);
        }

        /// <summary>
        /// 更新地面检测状态
        /// </summary>
        private void UpdateGroundDetection()
        {
            if (playerCollider == null || config == null) return;

            // 计算检测球体位置（脚底位置）
            Vector3 playerBottom = transform.position - new Vector3(0, playerCollider.height * 0.5f, 0);
            Vector3 spherePosition = playerBottom + new Vector3(0, config.GroundCheckRadius, 0);
            
            // 球体检测地面
            bool isGrounded = Physics.CheckSphere(spherePosition, config.GroundCheckRadius, config.GroundLayerMask);
            
            // 如果球体检测失败，尝试射线检测作为补充
            if (!isGrounded)
            {
                // 从脚底稍微往上的位置发射射线
                Vector3 rayStart = playerBottom + Vector3.up * 0.1f;
                isGrounded = Physics.Raycast(rayStart, Vector3.down, config.GroundCheckDistance + 0.1f, config.GroundLayerMask);
            }

            _runtimeData.SetGrounded(isGrounded);

            // 获取详细的地面信息
            if (isGrounded)
            {
                // 从玩家中心稍微往下发射射线获取准确的地面信息
                Vector3 rayStart = transform.position;
                float rayDistance = playerCollider.height * 0.5f + config.GroundCheckDistance;
                
                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, config.GroundLayerMask))
                {
                    Vector3 groundNormal = hit.normal;
                    float groundAngle = Vector3.Angle(groundNormal, Vector3.up);
                    _runtimeData.SetGroundInfo(hit, groundNormal, groundAngle);
                }
                else
                {
                    // 如果中心射线没有命中，从脚底再次尝试
                    rayStart = playerBottom + Vector3.up * 0.1f;
                    if (Physics.Raycast(rayStart, Vector3.down, out hit, config.GroundCheckDistance + 0.1f, config.GroundLayerMask))
                    {
                        Vector3 groundNormal = hit.normal;
                        float groundAngle = Vector3.Angle(groundNormal, Vector3.up);
                        _runtimeData.SetGroundInfo(hit, groundNormal, groundAngle);
                    }
                }
            }
        }

        private void UpdateMovementDirection()
        {
            if (cameraTransform == null) return;

            Vector2 input = _runtimeData.MoveInput;
            if (input.magnitude > 0.1f)
            {
                // 根据摄像机方向计算移动方向
                Vector3 forward = cameraTransform.forward;
                Vector3 right = cameraTransform.right;

                // 移除Y轴分量，保持水平移动
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();

                Vector3 moveDirection = (forward * input.y + right * input.x).normalized;
                _runtimeData.SetMoveDirection(moveDirection);
            }
            else
            {
                _runtimeData.SetMoveDirection(Vector3.zero);
            }
        }

        /// <summary>
        /// 应用移动力
        /// </summary>
        public void ApplyMovement(Vector3 targetVelocity, float acceleration)
        {
            Vector3 velocityChange = targetVelocity - playerRigidbody.linearVelocity;
            velocityChange.y = 0; // 不影响垂直速度

            Vector3 force = velocityChange * acceleration;
            playerRigidbody.AddForce(force, ForceMode.Acceleration);
        }

        /// <summary>
        /// 应用跳跃力
        /// </summary>
        public void ApplyJump(float jumpForce)
        {
            Vector3 jumpVelocity = playerRigidbody.linearVelocity;
            jumpVelocity.y = jumpForce;
            playerRigidbody.linearVelocity = jumpVelocity;
            _runtimeData.SetJumpTime();
        }

        public void ApplyDash(Vector3 dashVelocity)
        {
            if (!_runtimeData.CanDash) return;
            
            // 应用冲刺速度
            playerRigidbody.linearVelocity = new Vector3(dashVelocity.x,dashVelocity.y, dashVelocity.z);
            _runtimeData.SetVelocity(playerRigidbody.linearVelocity);
            _runtimeData.SetGrappling(false); // 取消抓钩状态
            _runtimeData.SetJumping(false); // 取消跳跃状态
            //todo _runtimeData.SetDashCount(_runtimeData.DashCount + 1);
            
        }

        /// <summary>
        /// 设置碰撞体高度（用于蹲伏）
        /// </summary>
        public void SetColliderHeight(float height)
        {
            playerCollider.height = height;
            playerCollider.center = _originalColliderCenter + new Vector3(0, playerCollider.height * 0.5f, 0);
        }

        /// <summary>
        /// 重置碰撞体到原始状态
        /// </summary>
        public void ResetCollider()
        {
            playerCollider.height = _originalColliderHeight;
            playerCollider.center = _originalColliderCenter;
        }

        /// <summary>
        /// 检查头顶是否有障碍物
        /// </summary>
        public bool CheckCeiling()
        {
            Vector3 rayStart = transform.position + Vector3.up * (playerCollider.height * 0.5f);
            float rayDistance = (_originalColliderHeight - playerCollider.height) + 0.1f;
            return Physics.Raycast(rayStart, Vector3.up, rayDistance, config.GroundLayerMask);
        }

        public void Cleanup()
        {
            PlayerInputEvents.OnMoveInput -= HandleMoveInput;
            PlayerInputEvents.OnSprintPressed -= HandleSprintPressed;
            PlayerInputEvents.OnSprintReleased -= HandleSprintReleased;
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        public void EnablePlayerGravity()
        {
            playerRigidbody.useGravity = true;
        }

        public void DisablePlayerGravity()
        {
            playerRigidbody.useGravity = false;
        }

        #if UNITY_EDITOR 

        // 调试绘制
        private void OnDrawGizmosSelected()
        {
            if (config == null || playerCollider == null) return;

            // 计算脚底位置
            Vector3 playerBottom = transform.position - new Vector3(0, playerCollider.height * 0.5f, 0);
            
            // 绘制地面检测球体
            Vector3 spherePosition = playerBottom + new Vector3(0, config.GroundCheckRadius, 0);
            Gizmos.color = _runtimeData?.IsGrounded == true ? Color.green : Color.red;
            Gizmos.DrawWireSphere(spherePosition, config.GroundCheckRadius);

            // 绘制地面检测射线（从脚底）
            Gizmos.color = Color.yellow;
            Vector3 rayStart = playerBottom + Vector3.up * 0.1f;
            Vector3 rayEnd = rayStart - Vector3.up * (config.GroundCheckDistance + 0.1f);
            Gizmos.DrawLine(rayStart, rayEnd);
            
            // 绘制地面信息获取射线（从中心）
            Gizmos.color = Color.cyan;
            Vector3 centerRayStart = transform.position;
            Vector3 centerRayEnd = centerRayStart - Vector3.up * (playerCollider.height * 0.5f + config.GroundCheckDistance);
            Gizmos.DrawLine(centerRayStart, centerRayEnd);

            // 绘制头顶检测射线（用于调试蹲伏状态）
            if (_originalColliderHeight > 0 && playerCollider.height < _originalColliderHeight)
            {
                Gizmos.color = Color.blue;
                Vector3 ceilingRayStart = transform.position + Vector3.up * (playerCollider.height * 0.5f);
                float rayDistance = (_originalColliderHeight - playerCollider.height) + 0.1f;
                Vector3 ceilingRayEnd = ceilingRayStart + Vector3.up * rayDistance;
                Gizmos.DrawLine(ceilingRayStart, ceilingRayEnd);
            }
            
            // 绘制玩家碰撞体轮廓
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position + playerCollider.center, 
                new Vector3(playerCollider.radius * 2, playerCollider.height, playerCollider.radius * 2));
        }

        #endif
    }
}

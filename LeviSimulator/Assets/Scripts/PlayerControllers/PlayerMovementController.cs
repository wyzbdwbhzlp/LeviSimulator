    using System;
    using PlayerControllers.PlayerCharacterStatusStrategy;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.Serialization;
    using Utilities;

    namespace PlayerControllers
    {
        public class PlayerMovementController:MonoBehaviour
        {
            [Header("移动设定")]
            [SerializeField][LabelText("平地水平最大速度")]private float maxHorizontalSpeed = 7f; // 平地水平最大速度
            [SerializeField][LabelText("平地加速度")]private float acceleration = 150f; // 加速度
            [SerializeField][LabelText("平地减速度")]private float deceleration = 100f; // 减速度（摩擦力）
            [SerializeField][LabelText("空中加速度")]private float airAcceleration = 15f; // 空中加速度
            [SerializeField][LabelText("空中最大速度")]private float maxAirSpeed = 5f; // 空中最大速度
            [Header("跳跃设定")]
            [SerializeField][LabelText("跳跃强度")]private float jumpForce = 5f;
 
            [Header("地面检测")]
            [SerializeField]private float groundCheckDistance = 0.1f; // 地面检测距离
            [SerializeField]private float groundCheckRadius = 0.4f; // 球体半径
            [SerializeField]private LayerMask groundLayerMask; // 地面层
            [SerializeField][ReadOnly]private bool isGrounded = false; // 是否在地面上
            [Header("当前参数")]
            [SerializeField][ReadOnly]private Vector3 currentPlayerMovementTendency; 
            [SerializeField][ReadOnly]private Vector3 fixedPlayerMovementTendencyByPlayerLookAt; // 根据视角修正后的玩家移动趋势
            [SerializeField]private Rigidbody rd; 
            private PlayerInputRouter _playerInputRouter;
            [SerializeField][ReadOnly][LabelText("玩家移动方向")]private Vector3 currentPlayerRdVelocity;
            [SerializeField][ReadOnly][LabelText("玩家移动速率大小")]private float currentPlayerRdVelocityMagnitude;
            [SerializeField][ReadOnly]private bool isGrappling = false;
            private bool isAllowedToMove = true; // 是否允许移动
            private Animation playerAnimation;
            public Rigidbody PlayerRigidbody=> rd;
            public bool IsGrounded => isGrounded;
            public Vector3 CurrentPlayerMovementTendency => currentPlayerMovementTendency;
            public Vector3 CurrentPlayerRdVelocity => currentPlayerRdVelocity;
            
            
            protected void Awake()
            {
                if (rd == null)
                {
                    LogUtil.LogError("Rigidbody未设置，请检查配置。", true);
                }
              
            }

            private void Update()
            {
                FixCurrentPlayerMovementTendency( _playerInputRouter.CameraController.PlayerLookAt);
            }

            private void FixedUpdate()
            {
                currentPlayerRdVelocity= rd.linearVelocity;// 获取当前刚体的线速度
                currentPlayerRdVelocityMagnitude= currentPlayerRdVelocity.magnitude;// 获取当前刚体速率的大小
                CheckGrounded();
                
                if (isAllowedToMove)
                {
                    HandleMovementPhysics();
                }

            }
            /// <summary>
            ///  处理玩家输入的移动
            /// </summary>
            public void ApplyMovement(Vector3 moveDirection)
            {
                Vector3 move = new Vector3(moveDirection.x, 0, moveDirection.y); // 将输入转换为3D向量
                SetPlayerMovementTendency(move.normalized);
            }

            private void HandleMovementPhysics()
            {
                Vector3 currentHorizontalVelocity = new Vector3(rd.linearVelocity.x, 0, rd.linearVelocity.z);
                
                float currentMaxSpeed = isGrounded ? maxHorizontalSpeed : maxAirSpeed;
                float currentAcceleration = isGrounded ? acceleration : airAcceleration;
                
                if (currentPlayerMovementTendency.magnitude > 0.1f)//如果玩家有运动趋势
                {
                    Vector3 targetDirection = fixedPlayerMovementTendencyByPlayerLookAt.normalized;
                    Vector3 targetVelocity = targetDirection * currentMaxSpeed;
                    
                    currentHorizontalVelocity = Vector3.MoveTowards(currentHorizontalVelocity, targetVelocity, currentAcceleration * Time.fixedDeltaTime);
                }
                else if (isGrounded) // 如果没有运动趋势且在地面上：减速
                {
                    currentHorizontalVelocity = Vector3.MoveTowards(currentHorizontalVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
                }
                
                if (currentHorizontalVelocity.magnitude > currentMaxSpeed)// 如果当前速度超过最大速度
                {
                    currentHorizontalVelocity = currentHorizontalVelocity.normalized * currentMaxSpeed;
                }

                //  应用最终计算出的速度
                rd.linearVelocity = new Vector3(currentHorizontalVelocity.x, rd.linearVelocity.y, currentHorizontalVelocity.z);
            }

            /// <summary>
            ///   应用钩爪跳跃计算的速度(Ver1用，已弃置)
            /// </summary>
            [Obsolete]
            public void ApplyGrappleJump(Vector3 velocityToSet)
            {
                LogUtil.Log($"计算的速度: {velocityToSet};");
                isGrappling = true;
            }
            public void StopGrapple()
            {
                isGrappling = false;
                LogUtil.Log("停止钩爪");
            }
            
           
            private void CheckGrounded() //地面检测
            {
                Vector3 spherePosition = transform.position + Vector3.up * groundCheckRadius;
                isGrounded = Physics.SphereCast(spherePosition, groundCheckRadius, Vector3.down, out _, groundCheckDistance, groundLayerMask);

            }
            public void SetRouter(PlayerInputRouter playerInputRouter)
            {
                _playerInputRouter = playerInputRouter;
            }

            public void DisablePlayerRbGravity()
            {
                rd.useGravity = false;
            }
            public void EnablePlayerRbGravity()
            {
                rd.useGravity = true;
            }
            public void ResetPlayerMovementTendency()
            {
                currentPlayerMovementTendency = Vector3.zero;
            }

            public void SetPlayerMovementTendency(Vector3 movementTendency)
            {
                currentPlayerMovementTendency = movementTendency;
            }

            private void FixCurrentPlayerMovementTendency(Vector3 wherePlayerLookAt)
            {
                if (wherePlayerLookAt.sqrMagnitude < 0.01f)
                {
                    fixedPlayerMovementTendencyByPlayerLookAt = Vector3.zero;
                    return;
                }
                wherePlayerLookAt.y = 0f; // 确保视角方向在水平面上
                // 将玩家的移动趋势修正为相对于玩家视角的方向
                fixedPlayerMovementTendencyByPlayerLookAt = Quaternion.LookRotation(wherePlayerLookAt) * currentPlayerMovementTendency;
            }

            public void StartJump()
            {
                if (isGrounded)
                {
                    LogUtil.Log("开始跳跃");
                    rd.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                    _playerInputRouter.ChangeStatus(new JumpingStatusStrategy());
                    isGrounded = false;
                }
                else
                {
                    LogUtil.Log("无法跳跃，当前不在地面上");
                }
            }
        }
    }
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using DG.Tweening;
    using PlayerControllers.CalculationPhysicsComponents;
    using PlayerControllers.PlayerCharacterStatusStrategy;
    using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
    using PlayerControllers.PlayerMovemenSettings;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.Serialization;
    using Utilities;

    namespace PlayerControllers
    {
        public class PlayerMovementController:MonoBehaviour
        {
            [Header("移动设定-正常")]
            [SerializeField][LabelText("移动设定SO")][InlineEditor]private PlayerWalkRunSetting movementSetting; // 移动设定SO
            public float MaxHorizontalSpeed => movementSetting.MaxHorizontalSpeed; // 平地水平最大速度
            public float MaxRunSpeedMultiplier => movementSetting.MaxRunSpeedMultiplier; // 平地疾跑速度倍率
            public float Acceleration => movementSetting.Acceleration;// 加速度
            public float Deceleration => movementSetting.Deceleration; // 减速度（摩擦力）
            public float AirAcceleration => movementSetting.AirAcceleration; // 空中加速度
            public float MaxAirSpeed => movementSetting.MaxAirSpeed; // 空中最大速度
            public float JumpForce => movementSetting.JumpForce; // 跳跃力
            public float CrouchSpeedMultiplier=> movementSetting.CrouchSpeedMultiplier; // 蹲伏时速度倍率
            [Header("移动设定-滑铲&蹲伏")]
            [SerializeField][LabelText("滑铲&蹲伏设定SO")][InlineEditor]private PlayerCrouchSideSetting crouchSlideSetting; // 滑铲&蹲伏设定SO
            public float CrouchHeightDifference => crouchSlideSetting.CrouchHeightDifference; // 蹲伏时碰撞体高度变化
            public float MinSlideSpeed => crouchSlideSetting.MinSlideSpeed; // 最小滑铲速度
            public float SlideDeceleration => crouchSlideSetting.SlideDeceleration; // 滑铲减速度
            public float SlideSpeedMultiplier => crouchSlideSetting.SlideSpeedMultiplier; 
            public float MinSlopeAngle => crouchSlideSetting.MinSlopeAngle; // 最小坡度角度
            public float SlopeSlideMinAngle=> crouchSlideSetting.SlopeSlideMinAngle; // 斜坡滑铲最小角度
            public float DownhillAccelScale => crouchSlideSetting.DownhillAccelScale; // 下坡加速倍率
            public float UphillExtraDecel => crouchSlideSetting.UphillExtraDecel; // 上坡额外减速度
            public float StickToGroundForce => crouchSlideSetting.StickToGroundForce;//贴地力
            public float MinMaintainSlideSpeed => crouchSlideSetting.MinMaintainSlideSpeed; // 最小维持滑铲速度
            public float FallSpeedToSlideBoostMultiplier => crouchSlideSetting.FallSpeedToSlideBoostMultiplier; // 跌落到地面的速度
            [Header("地面检测")]
            [SerializeField]private float groundCheckDistance = 0.1f; // 地面检测距离
            [SerializeField]private float groundCheckRadius = 0.4f; // 球体半径
            [SerializeField]private LayerMask groundLayerMask; // 地面层
            [SerializeField][ReadOnly]private bool isGrounded = false; // 是否在地面上
            private RaycastHit groundHit; // 用于存储地面检测结果
            [SerializeField][ReadOnly][LabelText("地面法线")]private Vector3 groundNormal;
            [SerializeField][ReadOnly][LabelText("沿地面向量")] private Vector3 groundDirection; // 沿地面的向量
            [Header("当前参数")]
            [SerializeField][ReadOnly]private Vector3 currentPlayerMovementTendency; 
            [SerializeField][ReadOnly]private Vector3 fixedPlayerMovementTendencyByPlayerLookAt; // 根据视角修正后的玩家移动趋势
            [SerializeField][ReadOnly][LabelText("玩家移动方向")]private Vector3 currentPlayerRdVelocity;
            [SerializeField][ReadOnly][LabelText("玩家移动速率大小")]private float currentPlayerRdVelocityMagnitude;
            [SerializeField][ReadOnly][LabelText("玩家水平移动速率大小")]private float currentPlayerRdHorizontalVelocityMagnitude;
            [SerializeField][ReadOnly]private bool isCrouching = false; // 是否正在蹲伏
            [SerializeField][ReadOnly]private bool isGrappling = false;
            [SerializeField][ReadOnly]private bool isAllowedToMove = true; // 是否允许移动
            [SerializeField][ReadOnly]private bool isSprinting = false; // 是否正在冲刺
            [SerializeField][ReadOnly]private bool IsCanWallRun = false; // 是否可以进行滑墙
            [Header("高度差计算参数")]
            [SerializeField][ReadOnly]private float lastFrameYPosition;
            [SerializeField][ReadOnly][LabelText("上一Fix帧和当前Fix帧的高度差")]private float heightDifferenceBetweenFrames;
            [Header("依赖&组件")]
            [SerializeField]private Rigidbody rd; 
            [SerializeField]private Collider playerCollider; // 玩家碰撞体
            private PlayerInputRouter _playerInputRouter;
            private ICalculationPhysicsComponent _physicsCalculationComponent;// 物理计算组件
            private Tweener _normalVelocityTweener;
            private Tweener _wallRunTweener;
            private CapsuleCollider _playerCapsuleCollider;
            private float _originalColliderHeight;
            private Vector3 _originalColliderCenter;
            private Dictionary<string,ICalculationPhysicsComponent> _statusToPhysicsCalculationComponentDic;
            private ICalculationPhysicsComponent _defaultPhysicsCalculationComponent;

            public Rigidbody PlayerRigidbody=> rd;
            public bool IsGrounded => isGrounded;
            public RaycastHit GroundHit => groundHit;
            public Vector3 CurrentPlayerMovementTendency => currentPlayerMovementTendency;
            public Vector3 CurrentPlayerRdVelocity => currentPlayerRdVelocity;
            public Vector3 FixedPlayerMovementTendencyByPlayerLookAt=> fixedPlayerMovementTendencyByPlayerLookAt;
            public bool IsSpringing => isSprinting;
            public LayerMask GroundLayerMask => groundLayerMask;
            
            public float CurrentPlayerRdHorizontalVelocityMagnitude => currentPlayerRdHorizontalVelocityMagnitude;
            
            public Vector3 GroundNormal => groundNormal;
            public Vector3 GroundDirection => groundDirection;
            public float HeightDifferenceBetweenFrames => heightDifferenceBetweenFrames;
            public bool IsCrouching => isCrouching;
            public bool IsAllowToMove => isAllowedToMove;
       
            
       
            
            protected void Awake()
            {
                if (rd == null)
                {
                    LogUtil.LogError("Rigidbody未设置，请检查配置。", true);
                }
                _playerCapsuleCollider = playerCollider as CapsuleCollider;
                if (_playerCapsuleCollider == null)
                {
                    LogUtil.LogError("PlayerMovementController需要一个CapsuleCollider，请检查配置。", true);
                    return;
                }
                _originalColliderHeight = _playerCapsuleCollider.height;
                _originalColliderCenter = _playerCapsuleCollider.center;


                var newAirbornePhysicsCalculationComponent = new AirbornePhysicsCalculationComponent();
                var walkPhysicsCalculationComponent = new WalkPhysicsCalculationComponent();
                _statusToPhysicsCalculationComponentDic= new Dictionary<string, ICalculationPhysicsComponent>
                {
                    {"SlidingStatusStrategy", new SlidingPhysicsCalculationComponent()},
                    {"CrouchStatusStrategy",walkPhysicsCalculationComponent},
                    {"WallRunningStatusStrategy", walkPhysicsCalculationComponent},
                    {"JumpingStatusStrategy",newAirbornePhysicsCalculationComponent},
                    {"FallingStatusStrategy",newAirbornePhysicsCalculationComponent},
                };
                
                
                _physicsCalculationComponent = _statusToPhysicsCalculationComponentDic["WallRunningStatusStrategy"];
                _physicsCalculationComponent.OnInit(this);

            }

            protected void OnEnable()
            {
                EventBroadcaster.PlayerCharacterStatusChanged+= OnPlayerCharacterStatusChanged;
                EventBroadcaster.SetPlayerAllowedToMove+= SetAllowedToMove;
            }
            

            protected void OnDisable()
            {
                EventBroadcaster.PlayerCharacterStatusChanged-= OnPlayerCharacterStatusChanged;
                EventBroadcaster.SetPlayerAllowedToMove-= SetAllowedToMove;
                _normalVelocityTweener?.Kill();
                _wallRunTweener?.Kill();
            }

            private void Start()
            {
                lastFrameYPosition = transform.position.y;
            }

            private void Update()
            {
                FixCurrentPlayerMovementTendency( _playerInputRouter.CameraController.PlayerLookAt);
            }

            private void FixedUpdate()
            {
                currentPlayerRdVelocity= rd.linearVelocity;// 获取当前刚体的线速度
                currentPlayerRdVelocityMagnitude= currentPlayerRdVelocity.magnitude;// 获取当前刚体速率的大小
#if UNITY_EDITOR
                DebugSpeedShowController.Instance?.UpdateSpeedTMP(currentPlayerRdVelocityMagnitude);
#endif
                currentPlayerRdHorizontalVelocityMagnitude = Mathf.Sqrt(
                    Mathf.Pow(currentPlayerRdVelocity.x, 2) + Mathf.Pow(currentPlayerRdVelocity.z, 2));
                CheckGrounded();
                CalculateSlopeSliding();
                CalculateHeightDifference();
                if (!_playerInputRouter.PlayerWallRunController.IsWallRunning)
                {
                    _physicsCalculationComponent.HandleMovementPhysics(); // 处理移动物理
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

            public void TrySprint()
            {
                if (isGrounded)
                {
                    isSprinting = true;
                    LogUtil.Log("开始冲刺");
                }
                else 
                {
                    isSprinting = false;
                    LogUtil.Log("无法冲刺，当前不在地面上");
                }
            }
            public void StopSprint()
            {
                isSprinting = false;
                LogUtil.Log("停止冲刺");
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
                isGrounded = Physics.SphereCast(spherePosition, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayerMask);
                if (isGrounded)
                {
                    groundNormal = hit.normal; // 获取地面的法线
                    groundDirection= hit.point - transform.position; // 获取地面方向向量
                    groundHit = hit;
                }
                else
                {
                    groundNormal = Vector3.up; // 如果不在地面上，默认法线为向上
//                    LogUtil.Log("未检测到地面");
                }
            }

            /// <summary>
            ///  计算斜坡滑铲的加速度
            /// </summary>
            /// <returns></returns>
            private float CalculateSlopeSliding()
            {
                if (!isGrounded)
                {
                    return 0;
                }
                // 计算玩家移动与地面法线的夹角
                float angle = Vector3.Angle(currentPlayerMovementTendency, groundNormal);
                //LogUtil.Log ($"当前玩家移动趋势: {currentPlayerMovementTendency}, 地面法线: {groundNormal}, 夹角: {angle}");
                return 0;
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
                wherePlayerLookAt.y = 0f; 
                // 将玩家的移动趋势修正为相对于玩家视角的方向
                fixedPlayerMovementTendencyByPlayerLookAt = Quaternion.LookRotation(wherePlayerLookAt) * currentPlayerMovementTendency;
            }

            public void StartJump()
            {
                if (isGrounded)
                {
                    LogUtil.Log("开始跳跃");
                    rd.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
                    _playerInputRouter.ChangeParentStatus<AirborneState>();
                    isGrounded = false;
                }
                else
                {
                    LogUtil.Log("无法跳跃，当前不在地面上");
                }
            }
            
            public void SetCrouchState(bool isCrouching)
            {
                this.isCrouching = isCrouching;
                if (isCrouching)
                {
                    _playerCapsuleCollider.height = _originalColliderHeight - CrouchHeightDifference;
                    _playerCapsuleCollider.center = _originalColliderCenter - new Vector3(0, CrouchHeightDifference / 2, 0);
                }
                else
                {
                    _playerCapsuleCollider.height = _originalColliderHeight;
                    _playerCapsuleCollider.center = _originalColliderCenter;
                }
            }
            /// <summary>
            /// 尝试停止蹲伏并站起
            /// </summary>
            /// <returns>如果成功站起则返回true，否则返回false</returns>
            public bool TryStopCrouch()
            {
                // 在站起前，检测头顶是否有障碍物
                float checkDistance = _originalColliderHeight - _playerCapsuleCollider.height;
                Vector3 p1 = transform.position + _playerCapsuleCollider.center - new Vector3(0, _playerCapsuleCollider.height / 2, 0);
                Vector3 p2 = p1 + Vector3.up * _playerCapsuleCollider.height;
            
                // 忽略玩家自身
                if (Physics.CapsuleCast(p1, p2, _playerCapsuleCollider.radius, Vector3.up, checkDistance, ~LayerMask.GetMask("Player")))
                {
                    LogUtil.Log("头顶有障碍物，无法站起！");
                    return false;
                }

                // 恢复碰撞体
                isCrouching = false;
                _playerCapsuleCollider.height = _originalColliderHeight;
                return true;
            }
            private void CalculateHeightDifference()
            {
                heightDifferenceBetweenFrames = transform.position.y - lastFrameYPosition;
                lastFrameYPosition = transform.position.y; // 更新上一帧的Y位置
                
            }

            public bool IsMovingAgainstWall(Vector3 moveDirection)
            {
                // 检测移动方向是否有墙壁阻挡
                return Physics.Raycast(transform.position, moveDirection, 0.7f, _playerInputRouter.PlayerWallRunController.WallLayerMask);
            }
            private void OnPlayerCharacterStatusChanged(HierarchicalBaseState from, HierarchicalBaseState to)
            {
                if (to == null)
                {
                    return;
                }
                string toName=to.GetType().Name;
                if (_statusToPhysicsCalculationComponentDic.TryGetValue(toName,
                        out ICalculationPhysicsComponent calculationPhysicsComponent))
                {
                    _physicsCalculationComponent=calculationPhysicsComponent;
                }
                else
                {
                    _physicsCalculationComponent = _statusToPhysicsCalculationComponentDic["WallRunningStatusStrategy"];
                }
                _physicsCalculationComponent.OnInit(this);
            }
            private void SetAllowedToMove(bool isAllowed)
            {
                isAllowedToMove = isAllowed;
            }
        }
    }
using UnityEngine;
using PlayerControllers.Refactored.Core;
using PlayerControllers.Refactored.Data;

namespace PlayerControllers.Refactored.Systems
{
    /// <summary>
    /// 玩家滑墙系统
    /// </summary>
    public class PlayerWallRunSystem : MonoBehaviour, IPlayerSystem
    {
        [Header("滑墙状态")]
        [SerializeField] private bool isWallRunning = false;
        [SerializeField] private Vector3 wallNormal;
        [SerializeField] private Vector3 wallForward;
        [SerializeField] private float wallRunTimer = 0f;
        [SerializeField] private bool canWallRun = false;
        
        private PlayerController _playerController;
        private PlayerRuntimeData _runtimeData;
        private PlayerMovementSystem _movementSystem;
        private PlayerCameraSystem _cameraSystem;
        private PlayerMovementConfig _config;
        
        // 公共访问器
        public bool IsEnabled { get; set; } = true;
        public bool IsWallRunning => isWallRunning;
        public bool CanWallRun => canWallRun;
        public Vector3 WallNormal => wallNormal;
        public Vector3 WallForward => wallForward;
        public float WallRunThresholdSpeed => _config?.WallRunThresholdSpeed ?? 8f;
        public float WallRunMinimumSpeed => _config?.WallRunMinimumSpeed ?? 2f;
        public LayerMask WallLayerMask => _config?.WallLayerMask ?? 1;
        
        public void Initialize(PlayerController playerController, PlayerMovementConfig config)
        {
            _playerController = playerController;
            _runtimeData = playerController.RuntimeData;
            _movementSystem = playerController.MovementSystem;
            _cameraSystem = playerController.CameraSystem;
            _config = config;
            
            // 订阅输入事件
            PlayerInputEvents.OnJumpPressed += HandleWallJump;
        }
        
        public void Update()
        {
            if (!IsEnabled) return;
            
            canWallRun = CheckCanWallRun();
            
            if (isWallRunning)
            {
                wallRunTimer += Time.deltaTime;
                
                // 检查是否应该停止滑墙
                if (!canWallRun || _runtimeData.HorizontalSpeed < _config.WallRunMinimumSpeed)
                {
                    StopWallRun();
                }
            }
        }
        
        public void FixedUpdate()
        {
            if (!IsEnabled || !isWallRunning) return;
            
            HandleWallRunPhysics();
        }
        
        private bool CheckCanWallRun()
        {
            // 检查基本条件
            if (_runtimeData.IsGrounded) return false;
            if (_runtimeData.HorizontalSpeed < _config.WallRunThresholdSpeed) return false;
            
            // 检查是否离地足够高
            Vector3 rayStart = transform.position;
            if (Physics.Raycast(rayStart, Vector3.down, _config.MinimumHeightForWallRun, _movementSystem.Config.GroundLayerMask))
                return false;
            
            // 检查附近的墙壁
            return FindBestWall();
        }
        
        private bool FindBestWall()
        {
            Collider[] nearbyWalls = Physics.OverlapSphere(transform.position, _config.WallMaxDistance, _config.WallLayerMask);
            
            if (nearbyWalls.Length == 0) return false;
            
            float bestScore = -1f;
            RaycastHit bestWallHit = new RaycastHit();
            
            foreach (var wallCollider in nearbyWalls)
            {
                Vector3 closestPoint = wallCollider.ClosestPoint(transform.position);
                Vector3 directionToWall = (closestPoint - transform.position).normalized;
                
                if (Physics.Raycast(transform.position, directionToWall, out RaycastHit hit, _config.WallMaxDistance, _config.WallLayerMask))
                {
                    // 检查墙壁是否足够垂直
                    if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.1f)
                    {
                        Vector3 playerMovement = _runtimeData.MoveDirection;
                        if (playerMovement.sqrMagnitude < 0.01f)
                        {
                            playerMovement = transform.forward;
                        }
                        
                        // 评分：找到与玩家移动方向最垂直的墙面
                        float score = 1 - Mathf.Abs(Vector3.Dot(hit.normal, playerMovement.normalized));
                        
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestWallHit = hit;
                        }
                    }
                }
            }
            
            if (bestScore > -1f)
            {
                wallNormal = bestWallHit.normal;
                wallForward = Vector3.Cross(wallNormal, Vector3.up);
                
                // 确保滑墙方向与玩家朝向大致一致
                if (Vector3.Dot(wallForward, transform.forward) < 0)
                {
                    wallForward = -wallForward;
                }
                return true;
            }
            
            return false;
        }
        
        private void HandleWallRunPhysics()
        {
            Vector3 inputDirection = _runtimeData.MoveDirection;
            Vector3 projectedInput = Vector3.Project(inputDirection, wallForward);
            
            float accumulatedResistance = _config.WallFriction * wallRunTimer;
            Vector3 targetVelocity = projectedInput * (_movementSystem.Config.RunSpeed * _config.WallSpeedMultiplier - accumulatedResistance);
            
            // 应用滑墙移动
            _movementSystem.ApplyMovement(targetVelocity, _movementSystem.Config.Acceleration);
            
            // 处理摄像机倾斜
            HandleCameraTilt();
        }
        
        private void HandleCameraTilt()
        {
            if (_cameraSystem == null) return;
            
            // 根据墙面位置设置摄像机倾斜
            float wallSide = GetWallSide();
            // 这里可以调用摄像机系统的倾斜方法
            // _cameraSystem.SetWallRunTilt(wallSide);
        }
        
        public void StartWallRun()
        {
            if (isWallRunning) return;
            
            isWallRunning = true;
            wallRunTimer = 0f;
            _runtimeData.SetWallRunning(true);
            
            // 清除垂直速度
            Vector3 velocity = _movementSystem.Rigidbody.linearVelocity;
            velocity.y = 0f;
            _movementSystem.Rigidbody.linearVelocity = velocity;
            
            // 关闭重力
            _movementSystem.Rigidbody.useGravity = false;
            
            Debug.Log("开始滑墙");
        }
        
        public void StopWallRun()
        {
            if (!isWallRunning) return;
            
            isWallRunning = false;
            wallRunTimer = 0f;
            _runtimeData.SetWallRunning(false);
            
            // 恢复重力
            _movementSystem.Rigidbody.useGravity = true;
            
            Debug.Log("停止滑墙");
        }
        
        private void HandleWallJump()
        {
            if (!isWallRunning) return;
            
            StopWallRun();
            
            // 应用墙跳力
            Vector3 upwardForce = Vector3.up * _config.WallJumpForce;
            Vector3 bounceForce = wallNormal * _config.WallJumpBounceForce;
            
            _movementSystem.Rigidbody.linearVelocity = Vector3.zero;
            _movementSystem.Rigidbody.AddForce(upwardForce + bounceForce, ForceMode.Impulse);
            
            // 切换到跳跃状态
            _playerController.ForceChangeState(PlayerState.Jumping);
        }
        
        public float GetWallSide()
        {
            if (!isWallRunning) return 0;
            Vector3 localNormal = transform.InverseTransformDirection(wallNormal);
            return Mathf.Sign(localNormal.x);
        }
        
        public void Cleanup()
        {
            PlayerInputEvents.OnJumpPressed -= HandleWallJump;
            if (isWallRunning)
            {
                StopWallRun();
            }
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
        
        // 调试绘制
        private void OnDrawGizmosSelected()
        {
            if (_config == null) return;
            
            // 绘制滑墙检测范围
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, _config.WallMaxDistance);
            
            if (isWallRunning)
            {
                // 绘制墙面法线
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, wallNormal * 2f);
                
                // 绘制滑墙方向
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, wallForward * 2f);
            }
            
            // 绘制最小离地高度检测
            Gizmos.color = Color.yellow;
            Vector3 groundCheckStart = transform.position;
            Vector3 groundCheckEnd = groundCheckStart - Vector3.up * _config.MinimumHeightForWallRun;
            Gizmos.DrawLine(groundCheckStart, groundCheckEnd);
        }
    }
}

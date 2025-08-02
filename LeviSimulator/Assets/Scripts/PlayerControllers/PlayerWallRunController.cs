using Sirenix.OdinInspector;
using UnityEngine;

namespace PlayerControllers
{
    public class PlayerWallRunController : MonoBehaviour
    {
        [Header("滑墙设定")] [SerializeField] [LabelText("爬墙检测距离")]
        private float wallMaxDistance = 1f;

        [SerializeField] [LabelText("墙面层")] private LayerMask wallLayerMask;
        [SerializeField] [LabelText("爬墙速度倍率")] private float wallSpeedMultiplier = 1.2f;
        [SerializeField] [LabelText("离地最小高度")] private float minimumHeightForWallRun = 1.2f;
        [SerializeField] [LabelText("启用滑墙阈值速度")] private float wallRunThresholdSpeed = 8f; // 启用滑墙的速度阈值
        [SerializeField] [LabelText("维持滑墙最低容忍速度")] private float wallRunMinimumSpeed = 2f; // 维持滑墙的最低容忍速度
        [SerializeField] [LabelText("滑墙阻力")] private float wallFriction = 0.5f; // 滑墙时的摩擦力
        [SerializeField] [LabelText("滑墙跳跃力")] private float wallJumpForce = 4f;
        [SerializeField] [LabelText("滑墙跳跃角度（垂直于墙面而言）")] private float wallJumpAngle = 45f; // 滑墙跳跃角度
        [SerializeField] [LabelText("离开墙面的弹力")] private float wallJumpBounceForce = 2f; // 离开墙面的弹力
        [Header("滑墙状态")] 
        [SerializeField] [ReadOnly] private bool isWallRunning = false;
        [SerializeField] [ReadOnly] private Vector3 wallNormal; // 墙壁的法线向量
        [SerializeField] [ReadOnly] private Vector3 wallForward; // 沿墙壁移动的方向
        [ReadOnly] public float wallRunTimer = 0f;
        [SerializeField] [ReadOnly] private bool isCanWallRun;
        private PlayerInputRouter _playerInputRouter;
        public float WallRunThresholdSpeed => wallRunThresholdSpeed;
        public bool IsWallRunning => isWallRunning;
        public LayerMask WallLayerMask => wallLayerMask;
        private Rigidbody rd =>_playerInputRouter.MovementController.PlayerRigidbody;

        private readonly Vector3[] wallCheckDirections = { Vector3.right, Vector3.left }; // 简化为只检测左右

        private void Update()
        {
            isCanWallRun = CanWallRun(); // 检查是否可以进行滑墙
        }

        private void FixedUpdate()
        {
            if (isWallRunning)
            {
                HandleWallRunPhysics();
            }
        }

        private void HandleWallRunPhysics()
        {
            var movementController =
                _playerInputRouter.MovementController;
            Vector3 inputDirection = movementController.FixedPlayerMovementTendencyByPlayerLookAt.normalized;

            Vector3 projectedInput = Vector3.Project(inputDirection, wallForward);
            float accumulatedResistance = wallFriction * wallRunTimer;

            // 计算目标速度
            Vector3 targetVelocity =
                projectedInput * (movementController.MaxHorizontalSpeed * wallSpeedMultiplier - accumulatedResistance);

            // 只影响水平速度，保留原有的垂直速度
            Vector3 newHorizontalVelocity = Vector3.Lerp(
                new Vector3(rd.linearVelocity.x, 0, rd.linearVelocity.z),
                new Vector3(targetVelocity.x, 0, targetVelocity.z),
                15f * Time.fixedDeltaTime);

            // 应用最终速度
            rd.linearVelocity = new Vector3(newHorizontalVelocity.x, rd.linearVelocity.y, newHorizontalVelocity.z);
        }

        public bool CanWallRun()
        {
            var movementController = _playerInputRouter.MovementController;
            // 检查是否有前进输入
            if ( movementController.CurrentPlayerMovementTendency.z <= 0) return false;

            if ( movementController.CurrentPlayerRdHorizontalVelocityMagnitude <= wallRunMinimumSpeed) return false;
            // 检查是否离地足够高
            if (Physics.Raycast(transform.position, Vector3.down, minimumHeightForWallRun, movementController.GroundLayerMask))
                return false;

            // 检查左右是否有墙
            foreach (var dir in wallCheckDirections)
            {
                if (Physics.Raycast(transform.position, transform.TransformDirection(dir), out RaycastHit hit,
                        wallMaxDistance, wallLayerMask))
                {
                    // 检查墙壁是否足够垂直
                    if (Mathf.Abs(Vector3.Dot(hit.normal, Vector3.up)) < 0.1f)
                    {
                        wallNormal = hit.normal;
                        wallForward = Vector3.Cross(wallNormal, Vector3.up);

                        // 确保移动方向与玩家朝向一致
                        if (Vector3.Dot(wallForward, transform.forward) < 0)
                        {
                            wallForward = -wallForward;
                        }

                        return true;
                    }
                }
            }

            return false;
        }


        public void StartWallRun()
        {
            rd.linearVelocity = new Vector3(rd.linearVelocity.x, 0, rd.linearVelocity.z); // 清除垂直速度
            isWallRunning = true;
            rd.useGravity = false;
        }

        public void StopWallRun()
        {
            isWallRunning = false;
            rd.useGravity = true;
        }

        public void WallJump()
        {
            StopWallRun();

            rd.linearVelocity = new Vector3(rd.linearVelocity.x, 0f, rd.linearVelocity.z);

            Vector3 upwardForce = Vector3.up * wallJumpForce;
            Vector3 bounceForce = wallNormal * wallJumpBounceForce;

            rd.AddForce(upwardForce + bounceForce, ForceMode.Impulse);
        }

        public float GetWallSide()
        {
            if (!isWallRunning) return 0;
            // 将墙壁法线转换到玩家的局部空间
            Vector3 localNormal = transform.InverseTransformDirection(wallNormal);
            // 如果法线的x分量为负，则墙在右边，反之在左边
            return Mathf.Sign(localNormal.x);
        }

        public void SetRouter(PlayerInputRouter playerInputRouter)
        {
            _playerInputRouter = playerInputRouter;
        }
        public bool IsWallNearby()
        {
            foreach (var dir in wallCheckDirections)
            {
                if (Physics.Raycast(transform.position, transform.TransformDirection(dir), wallMaxDistance, wallLayerMask))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
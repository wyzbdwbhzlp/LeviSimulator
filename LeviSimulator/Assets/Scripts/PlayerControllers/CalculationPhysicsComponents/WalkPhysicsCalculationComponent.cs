using DG.Tweening;
using PlayerControllers.PlayerCharacterStatusStrategy;
using UnityEngine;

namespace PlayerControllers.CalculationPhysicsComponents
{
    public class WalkPhysicsCalculationComponent:ICalculationPhysicsComponent
    {
        private PlayerMovementController _movementController;
        private Tweener _normalVelocityTweener;
        public void OnInit(PlayerMovementController movementController)
        {
            _movementController = movementController;
        }

        public void HandleMovementPhysics()
        {
            var isGrounded = _movementController.IsGrounded;
            var maxHorizontalSpeed = _movementController.MaxHorizontalSpeed;
            var crouchSpeedMultiplier= _movementController.CrouchSpeedMultiplier;
            var maxAirSpeed = _movementController.MaxAirSpeed;
            var maxRunSpeedMultiplier = _movementController.MaxRunSpeedMultiplier;
            var acceleration = _movementController.Acceleration;
            var isSprinting= _movementController.IsSpringing;
            var currentPlayerMovementTendency = _movementController.CurrentPlayerMovementTendency;
            var fixedPlayerMovementTendencyByPlayerLookAt = _movementController.FixedPlayerMovementTendencyByPlayerLookAt;
            var deceleration= _movementController.Deceleration;
            var rd = _movementController.PlayerRigidbody;
            
            // 计算当前状态下的最大速度和加速度
            float currentMaxSpeed = isGrounded ? maxHorizontalSpeed :maxAirSpeed;
            float sprintMultiplier = isSprinting && isGrounded ? maxRunSpeedMultiplier : 1f;
            float actualMaxSpeed = currentMaxSpeed * sprintMultiplier;

            Vector3 targetVelocity;
            float duration;

            if (currentPlayerMovementTendency.magnitude > 0.1f) // 如果玩家有输入
            {
                Vector3 targetDirection = fixedPlayerMovementTendencyByPlayerLookAt.normalized;
        
                //检测是否撞墙
                bool isAgainstWall = _movementController.IsMovingAgainstWall(targetDirection);
        
                if (isAgainstWall && !isGrounded)
                {
                    return;
                }
                    
                targetVelocity = targetDirection * actualMaxSpeed;
                    
                duration = actualMaxSpeed / acceleration;
            }
            else if (isGrounded) // 如果在地面上且无输入，则减速
            {
                targetVelocity = Vector3.zero;
                duration = rd.linearVelocity.magnitude / deceleration;
            }
            else // 在空中且无输入，则不处理，保持惯性
            {
                return;
            }
            targetVelocity=_movementController.IsCrouching? targetVelocity* crouchSpeedMultiplier : targetVelocity; // 如果是蹲下状态，应用蹲下速度倍率
            // 使用 Dotween 平滑地改变水平速度
            _normalVelocityTweener?.Kill(); 
            _normalVelocityTweener = DOTween.To(
                () => new Vector3(rd.linearVelocity.x, 0, rd.linearVelocity.z), // 获取当前水平速度
                (v) => rd.linearVelocity = new Vector3(v.x, rd.linearVelocity.y, v.z), // 设置新的水平速度，保持Y轴速度不变
                targetVelocity, // 目标速度
                duration // 动画时长
            ).SetEase(Ease.Linear); 
        }
    }
}
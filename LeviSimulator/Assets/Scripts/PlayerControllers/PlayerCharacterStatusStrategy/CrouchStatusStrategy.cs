using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class CrouchStatusStrategy : HierarchicalBaseState
    {
        private PlayerMovementController _movementController;

        protected override void EnterState()
        {
            _movementController = Ctx.MovementController;
            LogUtil.Log("进入蹲伏状态");
        }

        protected override void ExitState()
        {
            // 退出蹲伏状态时不再强制尝试站起。
            // 站起的逻辑由具体的输入（如跳跃、松开蹲伏键）或状态转换（如滑铲结束）来处理。
            LogUtil.Log("退出蹲伏状态");
        }

        protected override void UpdateStates()
        {
            // 如果在蹲伏时速度增加到超过最小滑铲速度，则切换到滑铲状态
            if (_movementController.CurrentPlayerRdHorizontalVelocityMagnitude > _movementController.MinSlideSpeed)
            {
                SwitchSubState<SlidingStatusStrategy>();
                return;
            }
            
        }

        protected override void HandleInput(InputAction.CallbackContext obj)
        {
            // CrouchStatusStrategy 不直接处理顶层输入
        }

        protected override bool HandleSubStateInput(InputAction.CallbackContext obj)
        {
            switch (obj.action.name)
            {
                case "Jump":
                    if (obj.phase == InputActionPhase.Started)
                    {
                        // 尝试站起，如果成功则跳跃
                        if (_movementController.TryStopCrouch())
                        {
                            _movementController.StartJump();
                        }
                        return true; // 输入已处理
                    }
                    break;

                case "Crouch":
                    // 当蹲伏键抬起时，尝试站起
                    if (obj.phase == InputActionPhase.Canceled)
                    {
                        // 尝试站起，如果成功则切换到行走状态
                        if (_movementController.TryStopCrouch())
                        {
                            SwitchSubState<WalkingStatusStrategy>();
                        }
                        // 如果无法站起，则保持蹲伏状态，等待下一次机会
                        return true; // 输入已处理
                    }
                    break;
            }
            return false; // 输入未处理
        }
    }
}
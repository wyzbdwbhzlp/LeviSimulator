using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class SlidingStatusStrategy : HierarchicalBaseState
    {
        private PlayerMovementController _movementController;

        protected override void EnterState()
        {
            _movementController = Ctx.MovementController;
            EventBroadcaster.CallSetPlayerAllowedToMove(false);//禁用玩家移动输入
            LogUtil.Log("进入滑铲状态");
        }

        protected override void ExitState()
        {
            EventBroadcaster.CallSetPlayerAllowedToMove(true);//启用玩家移动输入
        }

        protected override void UpdateStates()
        {
            if (Ctx.IsHoldingCrouch)
            {
                if (_movementController.CurrentPlayerRdHorizontalVelocityMagnitude < _movementController.MinMaintainSlideSpeed)
                {
                    SwitchSubState<CrouchStatusStrategy>();
                }
            }
            else
            {
                if (_movementController.TryStopCrouch())
                {
                    SwitchSubState<WalkingStatusStrategy>();
                }
                else
                {
                    SwitchSubState<CrouchStatusStrategy>();
                }
            }
        }

        protected override void HandleInput(InputAction.CallbackContext obj)
        {
        }

        protected override bool HandleSubStateInput(InputAction.CallbackContext obj)
        {
            switch (obj.action.name)
            {
                case "Jump":
                    if (obj.phase == InputActionPhase.Started)
                    {
                       
                        if (_movementController.TryStopCrouch())
                        {
                            _movementController.StartJump();
                        }
                        return true;//已处理输入
                    }
                    break;

                case "Crouch":
                    // 当蹲伏键抬起时，尝试站起
                    if (obj.phase == InputActionPhase.Canceled)
                    {
                        if (_movementController.TryStopCrouch())
                        {
                            SwitchSubState<WalkingStatusStrategy>();
                        }
                        // 如果无法站起，UpdateStates会在下一帧处理后续逻辑
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}
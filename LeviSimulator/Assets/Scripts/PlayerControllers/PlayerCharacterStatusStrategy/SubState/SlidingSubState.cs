using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy.SubState
{
    public class SlidingSubState:BaseSubState
    {
        private PlayerMovementController _movementController;

        public override void EnterState(HierarchicalBaseState superState, PlayerInputRouter ctx)
        {
            base.EnterState(superState, ctx);
            _movementController = Ctx.MovementController;
            EventBroadcaster.CallSetPlayerAllowedToMove(false); // 禁用玩家移动输入
        }

        public override void ExitState()
        {
            EventBroadcaster.CallSetPlayerAllowedToMove(true);// 恢复玩家移动输入
        }

        public override void UpdateStates()
        {
            bool speedTooLow = _movementController.CurrentPlayerRdHorizontalVelocityMagnitude < _movementController.MinMaintainSlideSpeed;

            if (!Ctx.IsHoldingCrouch)
            {
                // 松开蹲伏键，尝试站起
                if (_movementController.TryStopCrouch())
                {
                    LogUtil.Log("滑铲结束，切换到行走状态");
                    _currentSuperState.SwitchSubState<WalkingSubState>();
                }
                else
                {
                    LogUtil.Log("无法站起，切换到蹲伏状态");
                    _currentSuperState.SwitchSubState<CrouchSubState>();
                }
            }
            else if (speedTooLow)
            {
                // 按住蹲伏键但速度过低，切换到蹲伏
                LogUtil.Log("速度过低，从滑铲切换到蹲伏状态");
                _currentSuperState.SwitchSubState<CrouchSubState>();
            }
        }

        public override bool HandleInput(InputAction.CallbackContext obj)
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
                        return true; // 已处理输入
                    }
                    break;

                case "Crouch":
                    // 当蹲伏键抬起时，尝试站起
                    if (obj.phase == InputActionPhase.Canceled)
                    {
                        if (_movementController.TryStopCrouch())
                        {
                            _currentSuperState.SwitchSubState<WalkingSubState>();
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
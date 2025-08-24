using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy.SubState
{
    public class CrouchSubState:BaseSubState
    {
        private PlayerMovementController _movementController;
        public override void EnterState(HierarchicalBaseState superState, PlayerInputRouter ctx)
        {
           base.EnterState(superState, ctx);
            _movementController = Ctx.MovementController;
            EventBroadcaster.CallSetPlayerAllowedToMove(true);
        }

        public override void ExitState()
        {
           
        }

        public override void UpdateStates()
        {
          
        }

        public override bool HandleInput(InputAction.CallbackContext obj)
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
                            _currentSuperState.SwitchSubState<WalkingSubState>();
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
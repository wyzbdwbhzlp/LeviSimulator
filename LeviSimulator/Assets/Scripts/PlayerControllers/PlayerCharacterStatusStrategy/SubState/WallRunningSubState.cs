using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy.SubState
{
    public class WallRunningSubState:BaseSubState
    {
        private PlayerWallRunController _wallRunController;
        public override void EnterState(HierarchicalBaseState superState, PlayerInputRouter ctx)
        {
            base.EnterState(superState, ctx);
            _wallRunController= Ctx.PlayerWallRunController;
            LogUtil.Log("进入滑墙状态");
            _wallRunController.StartWallRun(); 
            _wallRunController.wallRunTimer = 0f;
        }

        public override void ExitState()
        {
            _wallRunController.wallRunTimer = 0f;
        }

        public override void UpdateStates()
        {
            if (_wallRunController.IsWallRunning&&!_wallRunController.IsCanWallRun)
            {
                _wallRunController.WallJump();
            }
            _wallRunController.wallRunTimer+= Time.deltaTime;
        }

        public override bool HandleInput(InputAction.CallbackContext obj)
        {
            if (obj.action.name == "Jump" && obj.phase == InputActionPhase.Started)
            {
                _wallRunController.WallJump();
                return true;
            }
            return false;
        }
    }
}
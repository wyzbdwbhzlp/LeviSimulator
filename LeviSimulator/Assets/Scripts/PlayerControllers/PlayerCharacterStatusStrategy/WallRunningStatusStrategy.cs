// Assets/Scripts/PlayerControllers/PlayerCharacterStatusStrategy/WallRunningStatusStrategy.cs

using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class WallRunningStatusStrategy : HierarchicalBaseState
    {
        private PlayerWallRunController _wallRunController;
        

        protected override void EnterState()
        {
            _wallRunController= Ctx.PlayerWallRunController;
            LogUtil.Log("进入滑墙状态");
            _wallRunController.StartWallRun(); 
            _wallRunController.wallRunTimer = 0f;
        }

        protected override void ExitState()
        {
            _wallRunController.StopWallRun(); // 通知控制器停止
            _wallRunController.wallRunTimer = 0f;
        }

        protected override void UpdateStates()
        {
            if (!_wallRunController.CanWallRun())
            {
                SwitchSubState<FallingStatusStrategy>();
            }
            _wallRunController.wallRunTimer+= Time.deltaTime;
        }

        protected override void HandleInput(InputAction.CallbackContext obj)
        {
            
        }

        protected override bool HandleSubStateInput(InputAction.CallbackContext obj)
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
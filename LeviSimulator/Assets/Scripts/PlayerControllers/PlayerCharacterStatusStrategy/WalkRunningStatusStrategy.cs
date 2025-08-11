// Assets/Scripts/PlayerControllers/PlayerCharacterStatusStrategy/WallRunningStatusStrategy.cs
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class WallRunningStatusStrategy : BaseStatusStrategy
    {
        private PlayerMovementController movementController;
        private PlayerWallRunController wallRunController;

        public override void OnEnter(PlayerInputRouter input)
        {
            base.OnEnter(input);
            movementController = _playerInputRouter.MovementController;
            wallRunController= _playerInputRouter.PlayerWallRunController;
            LogUtil.Log("进入滑墙状态");
            wallRunController.StartWallRun(); 
            wallRunController.wallRunTimer = 0f;
        }

        public override void LogicUpdate()
        {
            if (!wallRunController.CanWallRun())
            {
                _playerInputRouter.ChangeStatus<FallingStatusStrategy>();
            }
            wallRunController.wallRunTimer+= Time.deltaTime;
        }

        public override void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
            if (obj.action.name == "Jump" && obj.phase == InputActionPhase.Started)
            {
                wallRunController.WallJump();
                _playerInputRouter.ChangeStatus<JumpingStatusStrategy>(); // 跳跃后进入跳跃状态
                return;
            }
            switch (obj.action.name)
            {
                case "LaunchGrapple":
                    HandleLaunchGrapple(obj);
                    break;
            }
        }

        public override void OnExit()
        {
            base.OnExit();  
            wallRunController.StopWallRun(); // 通知控制器停止
            wallRunController.wallRunTimer = 0f;
        }

        public override void HandleInput(PlayerInput input) { }
        private void HandleLaunchGrapple(InputAction.CallbackContext callbackContext)
        {
            switch (callbackContext.phase)
            {
                case InputActionPhase.Started:
                    _playerInputRouter.PlayerGrappleController.StartGrapple();
                    LogUtil.Log("开始发射钩爪");
                    break;
                case InputActionPhase.Canceled:
                    _playerInputRouter.PlayerGrappleController.StopGrapple();
                    break;
            }
        }
    }
}
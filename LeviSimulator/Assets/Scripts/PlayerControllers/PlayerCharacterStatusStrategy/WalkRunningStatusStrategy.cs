// Assets/Scripts/PlayerControllers/PlayerCharacterStatusStrategy/WallRunningStatusStrategy.cs
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class WallRunningStatusStrategy : IPlayerCharacterStatusStrategy
    {
        private PlayerInputRouter playerInputRouter;
        private PlayerMovementController movementController;
        private PlayerWallRunController wallRunController;

        public void OnEnter(PlayerInputRouter input)
        {
            playerInputRouter = input;
            movementController = playerInputRouter.MovementController;
            wallRunController= playerInputRouter.PlayerWallRunController;
            LogUtil.Log("进入滑墙状态");
            wallRunController.StartWallRun(); 
            wallRunController.wallRunTimer = 0f;
            playerInputRouter.PlayerInput.onActionTriggered += HandleonActionTriggered;
        }

        public void LogicUpdate()
        {
            if (!wallRunController.CanWallRun())
            {
                wallRunController.WallJump();
                playerInputRouter.ChangeStatus(new FallingStatusStrategy());
            }
            wallRunController.wallRunTimer+= Time.deltaTime;
        }

        public void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
            if (obj.action.name == "Jump" && obj.phase == InputActionPhase.Started)
            {
                wallRunController.WallJump();
                playerInputRouter.ChangeStatus(new JumpingStatusStrategy()); // 跳跃后进入跳跃状态
                return;
            }
            switch (obj.action.name)
            {
                case "LaunchGrapple":
                    HandleLaunchGrapple(obj);
                    break;
            }
        }

        public void OnExit()
        {
            wallRunController.StopWallRun(); // 通知控制器停止
            wallRunController.wallRunTimer = 0f;
            playerInputRouter.PlayerInput.onActionTriggered -= HandleonActionTriggered;
        }

        public void HandleInput(PlayerInput input) { }
        private void HandleLaunchGrapple(InputAction.CallbackContext callbackContext)
        {
            switch (callbackContext.phase)
            {
                case InputActionPhase.Started:
                    playerInputRouter.PlayerGrappleController.StartGrapple();
                    LogUtil.Log("开始发射钩爪");
                    break;
                case InputActionPhase.Canceled:
                    playerInputRouter.PlayerGrappleController.StopGrapple();
                    break;
            }
        }
    }
}
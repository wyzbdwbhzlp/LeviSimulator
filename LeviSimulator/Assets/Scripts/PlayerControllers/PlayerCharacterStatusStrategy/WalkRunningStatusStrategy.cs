// Assets/Scripts/PlayerControllers/PlayerCharacterStatusStrategy/WallRunningStatusStrategy.cs
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public class WallRunningStatusStrategy : IPlayerCharacterStatusStrategy
    {
        private PlayerInputRouter playerInputRouter;

        public void OnEnter(PlayerInputRouter input)
        {
            playerInputRouter = input;
           // playerInputRouter.MovementController.StartWallRun(); // 通知控制器开始爬墙
            playerInputRouter.PlayerInput.onActionTriggered += HandleonActionTriggered;
        }

        public void LogicUpdate()
        {
            // 持续检查是否应该退出爬墙状态
            // 1. 如果接触到地面
            // 2. 如果不再检测到墙壁
            // 3. 如果玩家停止向前移动
           // if (playerInputRouter.MovementController.IsGrounded || !playerInputRouter.MovementController.CanWallRun())
            {
                playerInputRouter.ChangeStatus(new FallingStatusStrategy());
            }
        }

        public void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
            if (obj.action.name == "Jump" && obj.phase == InputActionPhase.Started)
            {
                //playerInputRouter.MovementController.WallJump();
                playerInputRouter.ChangeStatus(new JumpingStatusStrategy()); // 跳跃后进入跳跃状态
            }
        }

        public void OnExit()
        {
            //playerInputRouter.MovementController.StopWallRun(); // 通知控制器停止爬墙
            playerInputRouter.PlayerInput.onActionTriggered -= HandleonActionTriggered;
        }

        public void HandleInput(PlayerInput input) { }
    }
}
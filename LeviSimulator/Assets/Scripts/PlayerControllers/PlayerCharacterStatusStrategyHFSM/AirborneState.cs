// Assets/Scripts/PlayerControllers/PlayerCharacterStatusStrategy/AirborneState.cs

using PlayerControllers.PlayerCharacterStatusStrategy;
using UnityEngine.InputSystem;

namespace PlayerControllers.PlayerCharacterStatusStrategyHFSM
{
    public class AirborneState : HierarchicalBaseState
    {
        protected override void EnterState()
        {
            if (!Ctx.PlayerWallRunController.CanWallRun())
            {
                SwitchSubState<FallingStatusStrategy>();
            }
            else
            {
                SwitchSubState<WallRunningStatusStrategy>();
            }
        }

        protected override void ExitState() { }

        protected override void UpdateStates()
        {
            // 所有空中状态都共有的检查：如果落地，则进入地面状态
            if (Ctx.MovementController.IsGrounded)
            {
                Ctx.ChangeParentStatus<GroundedState>();
            }
        }

        protected override void HandleInput(InputAction.CallbackContext obj)
        {
            // 所有空中状态都共有的输入，例如发射钩爪
            if (obj.action.name == "LaunchGrapple")
            {
                switch (obj.phase)
                {
                    case InputActionPhase.Started:
                        Ctx.PlayerGrappleController.StartGrapple();
                        break;
                    case InputActionPhase.Canceled:
                        Ctx.PlayerGrappleController.StopGrapple();
                        break;
                }
            }
        }
    }
}
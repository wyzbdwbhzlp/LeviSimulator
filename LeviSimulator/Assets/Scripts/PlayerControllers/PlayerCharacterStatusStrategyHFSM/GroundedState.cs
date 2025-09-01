// Assets/Scripts/PlayerControllers/PlayerCharacterStatusStrategy/GroundedState.cs

using PlayerControllers.PlayerCharacterStatusStrategy;
using PlayerControllers.PlayerCharacterStatusStrategy.SubState;
using UnityEngine.InputSystem;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategyHFSM
{
    public class GroundedState : HierarchicalBaseState
    {
        protected override void EnterState()
        {
            var isHoldingCrouch = Ctx.IsHoldingCrouch;
            if (!isHoldingCrouch)
            {
                SwitchSubState<WalkingSubState>();
            }
            else
            {
                StartCrouchOrSliding();
            }
            
        }

        protected override void ExitState() { }

        protected override void UpdateStates()
        {
            // 如果离地，则进入空中状态
            if (!Ctx.MovementController.IsGrounded)
            {
                Ctx.ChangeParentStatus<AirborneState>();
            }
        }

        protected override void HandleInput(InputAction.CallbackContext obj)
        {
            if (obj.action.name == "Jump" && obj.phase == InputActionPhase.Started)
            {
                Ctx.MovementController.StartJump();
            }
        }
    }
}
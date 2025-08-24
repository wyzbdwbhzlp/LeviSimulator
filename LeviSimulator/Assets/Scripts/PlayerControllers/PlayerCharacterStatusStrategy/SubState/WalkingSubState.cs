
using PlayerControllers.PlayerCharacterStatusStrategyHFSM;
using UnityEngine.InputSystem;

namespace PlayerControllers.PlayerCharacterStatusStrategy.SubState
{
    public class WalkingSubState: BaseSubState
    {
        public override void EnterState(HierarchicalBaseState superState,PlayerInputRouter ctx)
        { 
            base.EnterState(superState, ctx);
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
                case "Crouch":
                    if(obj.phase == InputActionPhase.Started)
                    {
                        _currentSuperState.StartCrouchOrSliding();
                        return true; // 已处理
                    }
                    break;
                case "Sprint":
                    switch (obj.phase)
                    {
                        case InputActionPhase.Performed:
                            Ctx.MovementController.TrySprint();
                            break;
                        default:
                            Ctx.MovementController.StopSprint();
                            break;
                    }
                    return true; 
            }
            return false; 
        }
    }
}
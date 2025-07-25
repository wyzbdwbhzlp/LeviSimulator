using UnityEngine.InputSystem;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public interface IPlayerCharacterStatusStrategy
    {
        void HandleInput(PlayerInput input);
        void LogicUpdate();
        void OnEnter(PlayerInputRouter input);
        void OnExit();
        void HandleonActionTriggered(InputAction.CallbackContext obj);
    }
}
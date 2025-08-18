using UnityEngine.InputSystem;

namespace PlayerControllers.PlayerCharacterStatusStrategy
{
    public interface IPlayerCharacterStatusStrategy
    {
        void LogicUpdate();
        void OnEnter(PlayerInputRouter input);
        void OnExit();
        void HandleonActionTriggered(InputAction.CallbackContext obj);
    }
}
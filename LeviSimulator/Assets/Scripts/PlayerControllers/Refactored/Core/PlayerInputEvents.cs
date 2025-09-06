using System;
using UnityEngine;

namespace PlayerControllers.Refactored.Core
{
    /// <summary>
    /// 输入事件系统
    /// </summary>
    public static class PlayerInputEvents
    {
        // 移动输入
        public static event Action<Vector2> OnMoveInput;
        public static event Action<Vector2> OnLookInput;
        
        // 动作输入
        public static event Action OnJumpPressed;
        public static event Action OnJumpReleased;
        public static event Action OnCrouchPressed;
        public static event Action OnCrouchReleased;
        public static event Action OnSprintPressed;
        public static event Action OnSprintReleased;
        
        // 特殊动作
        public static event Action OnGrapplePressed;
        public static event Action OnGrappleReleased;
        
        // 触发输入事件的方法
        public static void TriggerMoveInput(Vector2 input) => OnMoveInput?.Invoke(input);
        public static void TriggerLookInput(Vector2 input) => OnLookInput?.Invoke(input);
        public static void TriggerJumpPressed() => OnJumpPressed?.Invoke();
        public static void TriggerJumpReleased() => OnJumpReleased?.Invoke();
        public static void TriggerCrouchPressed() => OnCrouchPressed?.Invoke();
        public static void TriggerCrouchReleased() => OnCrouchReleased?.Invoke();
        public static void TriggerSprintPressed() => OnSprintPressed?.Invoke();
        public static void TriggerSprintReleased() => OnSprintReleased?.Invoke();
        public static void TriggerGrapplePressed() => OnGrapplePressed?.Invoke();
        public static void TriggerGrappleReleased() => OnGrappleReleased?.Invoke();
        
        // 清理所有事件订阅
        public static void ClearAllEvents()
        {
            OnMoveInput = null;
            OnLookInput = null;
            OnJumpPressed = null;
            OnJumpReleased = null;
            OnCrouchPressed = null;
            OnCrouchReleased = null;
            OnSprintPressed = null;
            OnSprintReleased = null;
            OnGrapplePressed = null;
            OnGrappleReleased = null;
        }
    }
}

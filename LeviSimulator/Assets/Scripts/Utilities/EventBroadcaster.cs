using System;
using Manager;
using PlayerControllers.PlayerCharacterStatusStrategy;
using UIManager;
using Unity.VisualScripting;
using UnityEngine;

namespace Utilities
{
    public static class EventBroadcaster
    {
        public static event Action RequestStopGrappleEvent;
        public static void CallRequestStopGrappleEvent()
        {
            RequestStopGrappleEvent?.Invoke();
        }

        public static event Action EnablePlayerRbGravity;
        public static event Action DisablePlayerRbGravity;
        public static void CallEnablePlayerRbGravity()
        {
            EnablePlayerRbGravity?.Invoke();
        }
        public static void CallDisablePlayerRbGravity()
        {
            DisablePlayerRbGravity?.Invoke();
        }
        
        public static event Action EchoViewUIOpened;
        public static void CallEchoViewUIOpened()
        {
            EchoViewUIOpened?.Invoke();
        }
        public static event Action EchoViewUIClosed;
        public static void CallEchoViewUIClosed()
        {
            EchoViewUIClosed?.Invoke();
        }
        public static event Action<BaseStatusStrategy,BaseStatusStrategy> PlayerCharacterStatusChanged;
        /// <summary>
        /// 调用玩家状态改变事件(from statusStrategy to newStatusStrategyInstance)
        /// </summary>
        /// <param name="statusStrategy"></param>
        /// <param name="newStatusStrategyInstance"></param>
        public static void CallPlayerChangeStatusEvent(BaseStatusStrategy statusStrategy, BaseStatusStrategy newStatusStrategyInstance)
        {
            PlayerCharacterStatusChanged?.Invoke(statusStrategy,newStatusStrategyInstance);
        }
    }
}
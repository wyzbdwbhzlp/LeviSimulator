using System;
using Unity.VisualScripting;

namespace Utilities
{
    public static class OnEventHandler
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
    }
}
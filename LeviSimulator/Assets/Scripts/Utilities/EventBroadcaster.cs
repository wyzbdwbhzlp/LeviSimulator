using System;
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
        
        public static event Action PlayerEndGrappleEvent;
        public static void CallPlayerEndGrappleEvent()
        {
            PlayerEndGrappleEvent?.Invoke();
        }

        public static event Action<bool> SetPlayerAllowedToMove;
        public static void CallSetPlayerAllowedToMove(bool allowed)
        {
            SetPlayerAllowedToMove?.Invoke(allowed);
        }


        public static event Action<Vector3,Quaternion> OnPlayerReadySpawn;
        public static void CallPlayerReadySpawn(Vector3 spawnPosition,Quaternion spawnRotation)
        {
            OnPlayerReadySpawn?.Invoke(spawnPosition,spawnRotation);
        }
    }
}


using UnityEngine;

namespace PlayerControllers.Grapple
{
    public interface IGrapple
    {
        GrappleState GrappleState { get; set; }
        Vector3 GrapplePoint { get; set; }
        Transform GrappleTipTransform { get; } // 钩爪尖端位置引用(钩爪起始点)
        bool IsInvalidGrapple{ get; set; }
        void StartGrapple();
        void StopGrapple();
        void UpdateGrapple();

    }
}
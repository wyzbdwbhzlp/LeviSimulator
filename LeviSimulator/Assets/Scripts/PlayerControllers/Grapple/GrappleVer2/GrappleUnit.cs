using UnityEngine;
using Utilities;

namespace PlayerControllers.Grapple.GrappleVer2
{
    public class GrappleUnit : MonoBehaviour
    {
        private Rigidbody playerRigidbody;
        private Transform playerTransform;

        [Header("抓取设定")]
        [Tooltip("弹力系数，越高牵引越快")]
        public float springStrength = 80f;

        [Tooltip("阻尼系数，越高越快减速")]
        public float damping = 20f;

        [Tooltip("进入此距离后，钩爪会自动断开")]
        public float stableZone = 4f;

        private void Awake()
        {
            var rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        public void Initialize(Rigidbody playerRb)
        {
            playerRigidbody = playerRb;
            playerTransform = playerRb?.transform;

            if (playerRigidbody == null)
            {
                LogUtil.LogError("Player Rigidbody is null in GrappleUnit.", true);
                Destroy(gameObject);
                return;
            }

            // 关闭重力，改为完全靠钩爪拉力控制
            OnEventHandler.CallDisablePlayerRbGravity();
        }

        private void FixedUpdate()
        {
            if (playerTransform == null) return;

            Vector3 delta = transform.position - playerTransform.position;
            float distance = delta.magnitude;

            if (distance <= stableZone)
            {
                OnEventHandler.CallRequestStopGrappleEvent();
                return;
            }

            Vector3 direction = delta.normalized;

            // Hooke’s Law: F = -k * x - d * v
            Vector3 springForce = direction * springStrength * distance;
            Vector3 dampingForce = -playerRigidbody.linearVelocity * damping;

            Vector3 totalForce = springForce + dampingForce;

            playerRigidbody.AddForce(totalForce, ForceMode.Acceleration);
            LogUtil.Log($"速度: {playerRigidbody.linearVelocity}");
        }
    }
}

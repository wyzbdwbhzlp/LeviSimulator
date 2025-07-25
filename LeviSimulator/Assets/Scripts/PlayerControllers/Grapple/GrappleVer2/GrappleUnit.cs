using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace PlayerControllers.Grapple.GrappleVer2
{
    public class GrappleUnit : MonoBehaviour
    {
        private Rigidbody playerRigidbody;
        private Transform playerTransform;
        private PlayerInputRouter playerInputRouter;

        [Header("抓取设定")]
        [LabelText("弹力系数")]
        [SerializeField]private float springStrength = 80f; //越高牵引越快
        
        [LabelText("阻尼系数")]
        [SerializeField]private float damping = 20f;//越高越快减速
        
        [LabelText("进入此距离后，钩爪会自动断开")]
        [SerializeField]private float stableZone = 4f;
        
        [LabelText("是否启用玩家视角速度占比")]
        [SerializeField] private bool usePlayerViewParameters; 
        [ShowIf("usePlayerViewParameters")][LabelText("玩家视角占比因子")]
        [SerializeField][Range(0f,1f)]private float playerViewParameters;
        
        [LabelText("是否启用玩家视角夹角销毁检测")]
        [SerializeField] private bool usePlayerViewAngleCheck = true;
        [ShowIf("usePlayerViewAngleCheck")] [LabelText("玩家视角夹角阈值")] 
        [SerializeField][Range(30f, 180f)] private float maxAllowedViewAngle;

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
            playerInputRouter = PlayerInputRouter.Instance;

            if (playerRigidbody == null)
            {
                LogUtil.LogError("未能获取到玩家刚体", true);
                Destroy(gameObject);
                return;
            }

            // 关闭重力，改为完全靠钩爪拉力控制
            OnEventHandler.CallDisablePlayerRbGravity();
        }

        private void FixedUpdate()
        {
            if (playerRigidbody == null || playerTransform == null|| playerInputRouter == null)
            {
                LogUtil.LogError("玩家相关组件引用未设置，无法检查销毁条件", true);
                return;
            }
            Vector3 delta = transform.position - playerTransform.position;
            float distance = delta.magnitude;
            if (CheckDestroyCondition(delta))
            {
                OnEventHandler.CallRequestStopGrappleEvent();
                return;
            }

            Vector3 direction = delta.normalized; // 获取玩家与钩爪之间的方向向量
            
            Vector3 totalForce = CalculateGrappleForce(direction, distance);

            playerRigidbody.AddForce(totalForce, ForceMode.Acceleration);
            LogUtil.Log($"速度: {playerRigidbody.linearVelocity}");
        }
         /// <summary>
         /// F = -k * x - d * v
         /// </summary>
         /// <param name="direction"></param>
         /// <param name="distance"></param>
         /// <returns></returns>
        private Vector3 CalculateGrappleForce(Vector3 direction,float distance)
        {
            if (usePlayerViewParameters)
            {
                Vector3 viewDirection = playerInputRouter.CameraController.PlayerLookAt;
                direction = Vector3.Lerp(direction, viewDirection.normalized, playerViewParameters).normalized;
            }
            
            Vector3 springForce = direction * springStrength * distance;
            Vector3 dampingForce = -playerRigidbody.linearVelocity * damping;
            return springForce + dampingForce;
        }
        /// <summary>
        /// 检查钩爪是否满足销毁条件
        ///  </summary>
        private bool CheckDestroyCondition(Vector3 delta)
        {
            if (delta.magnitude <= stableZone)
            {
                LogUtil.Log("钩爪已进入稳定区，准备销毁");
                return true;
            }

            if (usePlayerViewAngleCheck)
            {
                Vector3 playerForward = playerInputRouter.CameraController.PlayerLookAt;
                float angle = Vector3.Angle(playerForward.normalized, delta.normalized);
                if (angle > maxAllowedViewAngle)
                {
                    LogUtil.Log($"钩爪与玩家视角夹角过大({angle}°)，准备销毁");
                    return true;
                }
            }


            return false;
        }
    }
}

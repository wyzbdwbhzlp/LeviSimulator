using DG.Tweening;
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

        [Title("抓取设定")]
        [LabelText("弹力系数")]
        [SerializeField]private float springStrength = 80f; //越高牵引越快
        
        [LabelText("阻尼系数")]
        [SerializeField]private float damping = 20f;//越高越快减速
        
        [LabelText("进入此距离后，钩爪会自动断开")]
        [SerializeField]private float stableZone = 4f;

        
        [Title("钩爪自销毁检测")]
        [LabelText("是否启用玩家视角夹角销毁检测")]
        [SerializeField] private bool usePlayerViewAngleCheck = true;
        [ShowIf("usePlayerViewAngleCheck")] [LabelText("玩家视角夹角阈值")] 
        [SerializeField][Range(30f, 180f)] private float maxAllowedViewAngle;
        
        [Title("玩家操作相关")]
        [InfoBox("当前因子总和超过1f，请注意",InfoMessageType.Warning,"IsParametersOverflow")]
        [LabelText("是否启用玩家视角速度占比")]
        [SerializeField] private bool usePlayerViewParameters; 
        [ShowIf("usePlayerViewParameters")][LabelText("玩家视角占比因子")]
        [SerializeField][PropertyRange(0f,1f)]private float playerViewParameters;
        [LabelText("是否启用玩家方向输入增益")]
        [SerializeField] private bool usePlayerInputDirection = true;
        [ShowIf("usePlayerInputDirection")] [LabelText("玩家输入增益力度")]
        [SerializeField]private float playerInputDirectionBuff = 0.5f; // 玩家输入方向增益的力度
        [ShowIf("usePlayerInputDirection")] [LabelText("玩家输入增益占比因子")]
        [SerializeField][PropertyRange(0f,1f)]private float playerInputDirectionParameters = 0.5f;
        [InfoBox("相关'因子'设定总和最好不超过1f，不然将有可能造成不良手感")]
        [ShowInInspector]private float totalParameters => playerViewParameters + playerInputDirectionParameters; 
        private bool IsParametersOverflow => totalParameters > 1f;
        [SerializeField]private Vector3 _smoothedWorldInputDirection;
        private Tweener _inputDirectionTween;
        
        
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
            EventBroadcaster.CallDisablePlayerRbGravity();
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
                EventBroadcaster.CallRequestStopGrappleEvent();
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

            if (usePlayerInputDirection)
            {
                Vector3 localInputDirection = playerInputRouter.MovementController.CurrentPlayerMovementTendency;
                Vector3 targetWorldInputDirection = Vector3.zero; // 默认目标方向为零

                if (localInputDirection.sqrMagnitude > 0.01f)
                {
                    Transform cameraTransform = playerInputRouter.CameraController.transform;
                    Vector3 cameraForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
                    Vector3 cameraRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;
                    targetWorldInputDirection = (cameraRight * localInputDirection.x + cameraForward * localInputDirection.z).normalized;
                }
                
                if (_inputDirectionTween == null || !_inputDirectionTween.IsActive())
                {
                    _inputDirectionTween = DOTween.To(() => _smoothedWorldInputDirection,
                            x => _smoothedWorldInputDirection = x,
                            targetWorldInputDirection,
                            0.25f) 
                        .SetEase(Ease.OutCubic); 
                }
                else
                {
                    _inputDirectionTween.ChangeEndValue(targetWorldInputDirection, true);
                }

                
                if (_smoothedWorldInputDirection.sqrMagnitude > 0.01f)
                {
                    Vector3 playerInputForceBuff = _smoothedWorldInputDirection * playerInputDirectionBuff;
                    direction = Vector3.Lerp(direction, playerInputForceBuff, playerInputDirectionParameters).normalized;
                }
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
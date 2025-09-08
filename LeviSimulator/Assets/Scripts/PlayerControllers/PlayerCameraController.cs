using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerControllers
{
    [Obsolete]
    public class PlayerCameraController : MonoBehaviour
    {
        [Title("鼠标控制设定")]
        [SerializeField] [LabelText("鼠标灵敏度")]
        private float mouseSensitivity = 2f;
        [SerializeField] [LabelText("平滑系数")] private float smoothness = 10f;
        [Title("滑墙视角设定")]
        [SerializeField] [LabelText("镜头翻滚角度")] private float maxRollAngle = 15f;
        [SerializeField] [LabelText("镜头翻滚速度")] private float rollSpeed = 5f;
        [Title("视角辅助瞄准设定")]
        [SerializeField] [LabelText("辅助瞄准强度")] [Range(0f, 1f)]
        private float defaultAssistStrength = 0.103f;
        [SerializeField] [LabelText("辅助瞄准持续时间(-1为持续)")] [MinValue(-1f)]
        private float defaultAssistDuration = -1f;
        [SerializeField] [LabelText("辅助瞄准响应速度")] [MinValue(0.1f)]
        private float assistResponseSpeed = 2f;
        [Title("当前输入参数")]
        [SerializeField] [ReadOnly]
        private Vector2 lookInput;
        [SerializeField] [ReadOnly] private float xRotation = 0f;
        [SerializeField] [ReadOnly] private float currentRollAngle = 0f;
        [ShowInInspector] [ReadOnly] public Vector3 PlayerLookAt => transform.forward;
        [Title("依赖引用")] 
        [SerializeField] private Transform playerBody;
        [SerializeField] private PlayerInputRouter _playerInputRouter;

        // 平滑旋转目标值
        private float targetXRotation = 0f;
        private float targetYRotation = 0f;

        private void OnEnable()
        {
            _playerInputRouter.PlayerInput.onActionTriggered += HandleonActionTriggered;
        }

        private void LateUpdate()
        {
            ApplySmoothRotation();
        }

        private void HandleonActionTriggered(InputAction.CallbackContext obj)
        {
            switch (obj.action.name)
            {
                case "Look":
                    HandleMouseLookInput(obj);
                    break;
            }
        }

        private void HandleMouseLookInput(InputAction.CallbackContext callbackContext)
        {
            if (callbackContext.phase == InputActionPhase.Performed)
            {
                lookInput = callbackContext.ReadValue<Vector2>();

                // 使用Time.deltaTime确保帧率独立性
                float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime * 60f;
                float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime * 60f;

                // 更新目标旋转值
                targetXRotation -= mouseY;
                targetXRotation = Mathf.Clamp(targetXRotation, -90f, 90f);

                targetYRotation += mouseX;
            }
            else if (callbackContext.phase == InputActionPhase.Canceled)
            {
                lookInput = Vector2.zero;
            }
        }

        private void ApplySmoothRotation()
        {
            xRotation = Mathf.Lerp(xRotation, targetXRotation, smoothness * Time.deltaTime);
            float currentY = playerBody.eulerAngles.y;
            float smoothY = Mathf.LerpAngle(currentY, targetYRotation, smoothness * Time.deltaTime);

            //处理镜头翻滚
            float targetRollAngle = 0f;
            if (_playerInputRouter.PlayerWallRunController.IsWallRunning)
            {
                Vector3 wallNormal = _playerInputRouter.PlayerWallRunController.WallNormal;
                Vector3 playerLookDirection = transform.forward;
                
                float dot = Mathf.Abs(Vector3.Dot(playerLookDirection, wallNormal));
                
                float rollMultiplier = 1 - dot;

                // 根据墙壁在左边还是右边决定基础翻滚方向和角度
                float baseRollAngle = -_playerInputRouter.PlayerWallRunController.GetWallSide() * maxRollAngle;

                // 应用系数
                targetRollAngle = baseRollAngle * rollMultiplier;
            }

            currentRollAngle = Mathf.Lerp(currentRollAngle, targetRollAngle, rollSpeed * Time.deltaTime);

            // 应用旋转
            transform.localRotation = Quaternion.Euler(xRotation, 0f, currentRollAngle);
            playerBody.rotation = Quaternion.Euler(0f, smoothY, 0f);
        }

        /// <summary>
        /// 平滑地将摄像机朝向指定方向（类似手柄辅助瞄准，不阻止玩家输入）
        /// </summary>
        /// <param name="targetDirection">目标方向（世界坐标）</param>
        /// <param name="assistStrength">辅助强度（可选，默认使用Inspector设置）</param>
        /// <param name="duration">辅助持续时间（可选，默认使用Inspector设置）</param>
        public void SmoothLookAtDirection(Vector3 targetDirection, float? assistStrength = null, float? duration = null)
        {
            if (targetDirection.sqrMagnitude < 0.01f)
            {
                Debug.LogWarning("目标方向不能为零向量");
                return;
            }

            // 使用传入参数或默认值
            float actualAssistStrength = assistStrength ?? defaultAssistStrength;
            float actualDuration = duration ?? defaultAssistDuration;

            // 停止之前的辅助瞄准
            StopLookAssist();

            Vector3 normalizedDirection = targetDirection.normalized;

            // 计算目标旋转角度
            float targetY = Mathf.Atan2(normalizedDirection.x, normalizedDirection.z) * Mathf.Rad2Deg;
            float horizontalDistance = Mathf.Sqrt(normalizedDirection.x * normalizedDirection.x +
                                                  normalizedDirection.z * normalizedDirection.z);
            float targetX = -Mathf.Atan2(normalizedDirection.y, horizontalDistance) * Mathf.Rad2Deg;
            targetX = Mathf.Clamp(targetX, -90f, 90f);

            // 启动辅助瞄准协程
            StartCoroutine(LookAssistCoroutine(targetX, targetY, actualAssistStrength, actualDuration));
        }

        private System.Collections.IEnumerator LookAssistCoroutine(float targetX, float targetY, float assistStrength,
            float duration)
        {
            float timer = 0f;

            while (duration < 0f || timer < duration)
            {
                // 计算当前角度与目标角度的差值
                float deltaX = Mathf.DeltaAngle(targetXRotation, targetX);
                float deltaY = Mathf.DeltaAngle(targetYRotation, targetY);

                // 应用辅助力，不覆盖玩家输入，而是添加到目标值上
                float assistForceX = deltaX * assistStrength * Time.deltaTime * assistResponseSpeed;
                float assistForceY = deltaY * assistStrength * Time.deltaTime * assistResponseSpeed;

                targetXRotation += assistForceX;
                targetYRotation += assistForceY;

                // 限制垂直角度
                targetXRotation = Mathf.Clamp(targetXRotation, -90f, 90f);

                timer += Time.deltaTime;
                yield return null;
            }
        }

        private Coroutine lookAssistCoroutine;


        /// <summary>
        /// 停止视角辅助
        /// </summary>
        public void StopLookAssist()
        {
            if (lookAssistCoroutine != null)
            {
                StopCoroutine(lookAssistCoroutine);
                lookAssistCoroutine = null;
            }
        }

        private void OnDisable()
        {
            _playerInputRouter.PlayerInput.onActionTriggered -= HandleonActionTriggered;
        }

        public void SetRouter(PlayerInputRouter playerInputRouter)
        {
            _playerInputRouter = playerInputRouter;
        }
    }
}
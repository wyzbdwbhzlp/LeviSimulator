using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerControllers
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Title("鼠标控制设定")]
        [SerializeField][LabelText("鼠标灵敏度")]private float mouseSensitivity = 2f;
        [SerializeField][LabelText("平滑系数")]private float smoothness = 10f;
        [Title("滑墙视角设定")]
        [SerializeField][LabelText("镜头翻滚角度")] private float maxRollAngle = 15f;
        [SerializeField][LabelText("镜头翻滚速度")] private float rollSpeed = 5f;
        [Title("当前输入参数")]
        [SerializeField][ReadOnly]private Vector2 lookInput;
        [SerializeField][ReadOnly]private float xRotation = 0f;
        [SerializeField][ReadOnly]private float currentRollAngle = 0f;
        [ShowInInspector][ReadOnly]public Vector3 PlayerLookAt=>transform.forward;
        
        [Title("依赖引用")]
        [SerializeField]private Transform playerBody;
        [SerializeField]private PlayerInputRouter _playerInputRouter;
        
        // 平滑旋转目标值
        private float targetXRotation = 0f;
        private float targetYRotation = 0f;

        private void OnEnable()
        {
            _playerInputRouter.PlayerInput.onActionTriggered += HandleonActionTriggered;
        }

        private void Update()
        {
            // 在Update中应用平滑旋转
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
            else if(callbackContext.phase == InputActionPhase.Canceled)
            {
                lookInput = Vector2.zero;
            }
        }

        private void ApplySmoothRotation()
        {
            // ... 原有的 xRotation 和 yRotation 计算 ...
            xRotation = Mathf.Lerp(xRotation, targetXRotation, smoothness * Time.deltaTime);
            float currentY = playerBody.eulerAngles.y;
            float smoothY = Mathf.LerpAngle(currentY, targetYRotation, smoothness * Time.deltaTime);

            // 新增：处理镜头翻滚
            float targetRollAngle = 0f;
            if (_playerInputRouter.PlayerWallRunController.IsWallRunning) // 需要在MovementController中将isWallRunning设为public
            {
                // 根据墙壁在左边还是右边决定翻滚方向
                targetRollAngle = -_playerInputRouter.PlayerWallRunController.GetWallSide() * maxRollAngle; // 需要添加GetWallSide方法
            }
            currentRollAngle = Mathf.Lerp(currentRollAngle, targetRollAngle, rollSpeed * Time.deltaTime);

            // 应用旋转
            transform.localRotation = Quaternion.Euler(xRotation, 0f, currentRollAngle);
            playerBody.rotation = Quaternion.Euler(0f, smoothY, 0f);
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
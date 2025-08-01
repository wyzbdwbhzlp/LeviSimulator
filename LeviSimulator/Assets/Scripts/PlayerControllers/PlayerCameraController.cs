using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerControllers
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Title("鼠标控制设定")]
        [SerializeField][LabelText("鼠标灵敏度")]private float mouseSensitivity = 100f;
        [Title("当前输入参数")]
        [SerializeField][ReadOnly]private Vector2 lookInput;
        [SerializeField][ReadOnly]private float xRotation = 0f;
        [ShowInInspector][ReadOnly]public Vector3 PlayerLookAt=>transform.forward;
        [Title("依赖引用")]
        [SerializeField]private Transform playerBody;
        private PlayerInputRouter _playerInputRouter;

        private void OnEnable()
        {
            _playerInputRouter.PlayerInput.onActionTriggered += HandleonActionTriggered;
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
                float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
                float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 防止过度旋转
                
                transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                playerBody.Rotate(Vector3.up * mouseX);  
            }
            else if(callbackContext.phase == InputActionPhase.Canceled)
            {
                
                lookInput= Vector2.zero;
                
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

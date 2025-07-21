using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PlayerControllers
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("鼠标控制设定")]
        private float mouseX;
        private float mouseY;
        [SerializeField][LabelText("鼠标灵敏度")]private float mouseSensitivity = 100f;
        [Header("依赖引用")]
        [SerializeField]private Transform playerBody;
        
        private void Update()
        {
            HandleMouseLook();
        }

        private void HandleMouseLook()
        {
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            playerBody.Rotate(Vector3.up * mouseX);
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x - mouseY, 0f, 0f);
        }


    }
}

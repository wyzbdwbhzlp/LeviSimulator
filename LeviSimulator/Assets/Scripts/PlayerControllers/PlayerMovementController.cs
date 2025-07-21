using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace PlayerControllers
{
    public class PlayerMovementController:Singleton<PlayerMovementController>
    {
        [Header("移动速度设定")]
        public float moveSpeed = 5f;
        [Header("地面检测")]
        [SerializeField]private float groundCheckDistance = 0.1f; // 地面检测距离
        [SerializeField]private LayerMask groundLayerMask; // 地面层
        [SerializeField][ReadOnly]private bool isGrounded = false; // 是否在地面上
        [Header("场景引用")]
        [SerializeField][SceneObjectsOnly]private Rigidbody rd; 
        
        private Vector3 playerVelocity;
        [SerializeField][ReadOnly]private bool isGrappling = false;
        
        private void Update()
        {
            HandlePlayerMoveInput();
        }
        private void FixedUpdate()
        {
            CheckGrounded();
            
            if (isGrappling)
            {
                rd.linearVelocity = playerVelocity; // 设置玩家速度为钩爪计算的速度
            }
        }
        /// <summary>
        ///  处理玩家输入的移动
        /// </summary>
        private void HandlePlayerMoveInput()
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");
            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            rd.linearVelocity = move * moveSpeed + new Vector3(0, rd.linearVelocity.y, 0); 
        }
        /// <summary>
        ///   应用钩爪跳跃计算的速度
        /// </summary>
        public void ApplyGrappleJump(Vector3 velocityToSet)
        {
            LogUtil.Log($"计算的速度: {velocityToSet};");
            playerVelocity = velocityToSet;
            isGrappling = true;
        }
        /// <summary>
        /// 钩爪位移计算
        /// </summary>
        public void StopGrapple()
        {
            isGrappling = false;
            LogUtil.Log("停止钩爪");
        }
        private void CheckGrounded()
        {
            Vector3 rayOrigin = transform.position;
            isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundLayerMask);
            
            if (isGrounded && rd.linearVelocity.y < 0)
            {
                rd.linearVelocity = new Vector3(rd.linearVelocity.x, 0, rd.linearVelocity.z);
            }
        }
    }
}
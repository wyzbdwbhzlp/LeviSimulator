    using PlayerControllers.PlayerCharacterStatusStrategy;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using Utilities;

    namespace PlayerControllers
    {
        public class PlayerMovementController:MonoBehaviour
        {
            [Header("移动设定")]
            public float moveSpeed = 5f;
            [Header("地面检测")]
            [SerializeField]private float groundCheckDistance = 0.1f; // 地面检测距离
            [SerializeField]private LayerMask groundLayerMask; // 地面层
            [SerializeField][ReadOnly]private bool isGrounded = false; // 是否在地面上
            [Header("依赖引用")]
            [SerializeField]private Rigidbody rd; 
            private PlayerInputRouter _playerInputRouter;
            private Vector3 playerVelocity;
            [SerializeField][ReadOnly]private bool isGrappling = false;
            public Rigidbody PlayerRigidbody=> rd;
            public bool IsGrounded => isGrounded;
            protected void Awake()
            {
                if (rd == null)
                {
                    LogUtil.LogError("Rigidbody未设置，请检查配置。", true);
                }
              
            }
       
            private void FixedUpdate()
            {
                CheckGrounded();
                
            }
            /// <summary>
            ///  处理玩家输入的移动
            /// </summary>
            public void ApplyMovement(Vector3 moveDirection)
            {
                float moveX = moveDirection.x;
                float moveZ = moveDirection.y;
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
            public void StopGrapple()
            {
                isGrappling = false;
                LogUtil.Log("停止钩爪");
            }
            
           
            private void CheckGrounded() //地面检测
            {
                Vector3 rayOrigin = transform.position;
                isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundLayerMask);
                
                if (isGrounded && rd.linearVelocity.y < 0)
                {
                    rd.linearVelocity = new Vector3(rd.linearVelocity.x, 0, rd.linearVelocity.z);
                }
            }
            public void SetRouter(PlayerInputRouter playerInputRouter)
            {
                _playerInputRouter = playerInputRouter;
            }

            public void DisablePlayerRbGravity()
            {
                rd.useGravity = false;
            }
            public void EnablePlayerRbGravity()
            {
                rd.useGravity = true;
            }
        }
    }
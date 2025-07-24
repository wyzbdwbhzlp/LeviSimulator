using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace PlayerControllers.Grapple
{
    public class PlayerGrappleManager:MonoBehaviour
    {
        [Header("按键设定")]
        [SerializeField] private KeyCode grappleKey = KeyCode.Mouse0; // 鼠标左键
        [Header("钩爪设定")]
        [LabelText("钩爪飞行时的过冲高度")] public float overshootYAxis = 2f; // 钩爪飞行时的过冲高度
        [Header("场景引用")]
        [SerializeField] private GrappleHook grappleHook;
        [SerializeField] private GrappleCableRenderer grappleCableRenderer;
        private PlayerInputRouter _playerInputRouter;
        

        protected void Awake()
        {
            if (grappleHook == null || grappleCableRenderer == null)
            {
                LogUtil.LogError("钩爪,线渲染器或玩家移动组件未设置，请检查Inspector配置", true);
            }
        }
        public void SetRouter(PlayerInputRouter playerInputRouter)
        {
            _playerInputRouter = playerInputRouter;
        }

        private void Update()
        {
            

            if (grappleHook.GrappleState != GrappleState.Idle)
            {
                grappleHook.UpdateGrapple();
            }
            grappleCableRenderer.UpdateCable();
        }
        public void StartGrapple()
        {
            grappleHook.StartGrapple();
        }
        public void StopGrapple()
        {
            grappleHook.StopGrapple();
        }
        public void ExecuteGrappleJump()
        {
            var result=CalculateJumpVelocity();
            _playerInputRouter.MovementController.ApplyGrappleJump(result);
        }

        public Vector3 CalculateJumpVelocity()
        {
            var startPoint = transform.position;
            var endPoint = grappleHook.GrapplePoint;
            Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
            float grapplePointRelativeYPos = endPoint.y - lowestPoint.y;
            float trajectoryHeight = grapplePointRelativeYPos + overshootYAxis;
            
            float gravity = Physics.gravity.y;
            float displacementY = endPoint.y - startPoint.y;
            Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);
            Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
            Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) 
                                                   + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

            return velocityXZ + velocityY;
        }

    }
}
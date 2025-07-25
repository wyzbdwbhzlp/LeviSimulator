using PlayerControllers.PlayerCharacterStatusStrategy;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace PlayerControllers.Grapple
{
    public enum GrappleState
    {
        Idle, // 空闲状态
        Flight, // 飞行状态
        Grappling, // 钩爪状态
    }
    public class PlayerGrappleController:MonoBehaviour
    {
        private Vector3 recoveryVelocity;
        [Header("场景引用")]
        [ShowInInspector] private IGrapple grappleHook;
        [SerializeField] private GrappleCableRenderer grappleCableRenderer;
        private PlayerInputRouter _playerInputRouter;
        

        protected void Awake()
        {
            if (grappleCableRenderer == null)
            {
                LogUtil.LogError("线渲染器，请检查Inspector配置", true);
            }
            grappleHook = GetComponentInChildren<IGrapple>();
            if (grappleHook == null)
            {
                LogUtil.LogError("钩爪组件未设置，请检查Inspector配置", true);
            }
        }
        private void OnEnable()
        {
           OnEventHandler.RequestStopGrappleEvent+= StopGrapple;
        }
        private void OnDisable()
        {
            OnEventHandler.RequestStopGrappleEvent -= StopGrapple;
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
            _playerInputRouter.DisablePlayerRbGravity();
        }
        public void StopGrapple()
        {
            HandleGrappleStop();
            grappleHook.StopGrapple();
            
            _playerInputRouter.EnablePlayerRbGravity();
            _playerInputRouter.ChangeStatus(new FallingStatusStrategy());
            _playerInputRouter.MovementController.PlayerRigidbody.linearVelocity = recoveryVelocity; // 恢复玩家速度
        }

        private void HandleGrappleStop()
        {
            var rd= _playerInputRouter.MovementController.PlayerRigidbody;
            recoveryVelocity = rd.linearVelocity;

        }


    }
}
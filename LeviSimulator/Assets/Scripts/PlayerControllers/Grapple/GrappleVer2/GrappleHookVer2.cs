using PlayerControllers.PlayerCharacterStatusStrategy;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace PlayerControllers.Grapple.GrappleVer2
{
    public class GrappleHookVer2 : MonoBehaviour,IGrapple
    {
        [Header("钩爪设定")]
        [SerializeField]private GameObject grapplePrefab; // 钩爪预制体
        [SerializeField][ReadOnly] private GameObject grapplePrefabInstance; // 钩爪实例
        [SerializeField] private GrappleState grappleState;
        [SerializeField] private float grappleSpeed = 20f; 
        [SerializeField] private float maxGrappleDistance = 20f; // 最大钩爪距离
        [SerializeField] private LayerMask grappleLayer=default; // 钩爪可抓取的层
        [Header("钩爪状态")]
        [SerializeField][ReadOnly] private Vector3 grapplePoint; // 钩爪抓取点
        [SerializeField][ReadOnly] private float grappleFlightTime; // 钩爪飞行时间
        [SerializeField][ReadOnly] private bool isInvalidGrapple = false; // 是否抓取到了无效对象
        [Header("依赖引用")]
        private Transform cameraTransform; // 摄像机位置引用
        private PlayerInputRouter playerInputRouter; // 玩家输入路由器引用
        [SerializeField]private Transform grappleTipTransform; // 钩爪尖端位置引用(钩爪起始点)
        
        
        public GrappleState GrappleState
        {
            get => grappleState;
            set => grappleState = value;
        }

        public Vector3 GrapplePoint 
        {
            get => grapplePoint;
            set => grapplePoint = value;
        }

        public Transform GrappleTipTransform => grappleTipTransform; // 钩爪尖端位置引用(钩爪起始点)
        public bool IsInvalidGrapple
        {
            get => isInvalidGrapple;
            set => isInvalidGrapple = value;
        }

        private void Awake()
        {
            if (grapplePrefab == null)
            {
                LogUtil.LogError("钩爪预制体未设置，请检查配置。", true);
            }

        }
        private void Start()
        {
            playerInputRouter = PlayerInputRouter.Instance;
            cameraTransform= playerInputRouter.CameraController.gameObject.transform; 
            grappleState = GrappleState.Idle; 
        }
        public void StartGrapple()
        {
            if (grappleState !=GrappleState.Idle)
            {
                LogUtil.LogWarning("已经在钩爪状态中，无法再次开始钩爪");
                return;
            }
            
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, maxGrappleDistance, grappleLayer))
            {
                grappleState= GrappleState.Flight; // 设置钩爪状态为飞行中
                
                grapplePoint = hit.point;
                
                var grappleFlightDistance= Vector3.Distance(transform.position, grapplePoint); // 计算钩爪飞行距离
                LogUtil.Log($"钩爪飞行距离: {grappleFlightDistance}, 目标点: {grapplePoint}, 当前钩爪位置: {transform.position}");
                grappleFlightTime = grappleFlightDistance / grappleSpeed; // 计算钩爪飞行时间
            }
            else
            {
                grapplePoint = cameraTransform.position + cameraTransform.forward * maxGrappleDistance; // 如果没有命中目标，设置钩爪点为最大距离
                
                grappleState = GrappleState.Flight; // 设置钩爪状态为飞行中
                
                isInvalidGrapple = true; 
                LogUtil.LogWarning("未命中任何可钩爪的目标，钩爪点设置为最大距离");
                grappleFlightTime = maxGrappleDistance / grappleSpeed; // 计算钩爪飞行时间
            }
        }

        public void StopGrapple()
        {
            if(grapplePrefabInstance)
            {
                Destroy(grapplePrefabInstance);
                grapplePrefabInstance = null;
            }
            isInvalidGrapple = false; 
            grappleState = GrappleState.Idle; 
        }

        public void UpdateGrapple()
        {
            if(grappleFlightTime> 0) 
            {
                grappleFlightTime-= Time.deltaTime;
                return;
            }
            
            if (!grapplePrefabInstance&&!isInvalidGrapple)
            {
                grapplePrefabInstance = Instantiate(grapplePrefab, grapplePoint, Quaternion.identity);
                LogUtil.Log($"钩爪实例化成功，位置: {grapplePoint},{grapplePrefabInstance.gameObject.name}");
                var grappleUnit= grapplePrefabInstance.GetComponent<GrappleUnit>();
                grappleUnit.Initialize(playerInputRouter.MovementController.PlayerRigidbody);
                playerInputRouter.ChangeStatus(new GrapplingStatusStrategy()); // 切换到钩爪状态
                grappleState= GrappleState.Grappling;
            }
            if(isInvalidGrapple)
            {
                StopGrapple();
            }
        }
    }
}
using System;
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
    public class GrappleHook : MonoBehaviour
    {
        [Header("钩爪设定")] 
        [LabelText("最大钩爪距离")]public float maxGrappleDistance = 20f;
        [LabelText("钩爪可抓取的层")] public LayerMask grappleLayer; 
        [LabelText("钩爪抓取目标点的速度")] public float grappleSpeed = 5f; // 钩爪抓取目标点的速度
        [Header("依赖引用")]
        [SerializeField][SceneObjectsOnly]private Transform cameraTransform; // 摄像机位置引用
        [SerializeField][SceneObjectsOnly]private Transform grappleTipTransform; // 钩爪尖端位置引用(钩爪起始点)
        [SerializeField][SceneObjectsOnly]private GrappleManager grappleManager; // 钩爪管理器引用
        
        [Header("钩爪状态")]
        [ReadOnly][LabelText("钩爪是否抓取到了无效对象")][SerializeField]private bool isInvalidGrapple = false; 
        [ReadOnly][LabelText("钩爪状态")][SerializeField] private GrappleState grappleState = GrappleState.Idle; 
        [ReadOnly][LabelText("钩爪飞行时间")][SerializeField]private float grappleFlightTime; 
        [ReadOnly][LabelText("钩爪抓取点")][SerializeField]private Vector3 grapplePoint; 
        
        public Transform GrappleTipTransform => grappleTipTransform; 
        public bool IsInvalidGrapple => isInvalidGrapple;
        public Vector3 GrapplePoint => grapplePoint;
        public GrappleState GrappleState => grappleState; // 获取钩爪状态

        public void Awake()
        {
            if(grappleManager == null)
            {
                LogUtil.LogError("并未设置grappleManager", true);
            }
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
        public void UpdateGrapple()
        {
            if(grappleFlightTime> 0) 
            {
               grappleFlightTime-= Time.deltaTime;
               return;
            }
            if(!isInvalidGrapple)
            {
                grappleState = GrappleState.Grappling; 
                grappleManager.ExecuteGrappleJump();// 执行钩爪跳跃
            }
            else
            {
                StopGrapple(); 
            }
        }
        public void StopGrapple()
        {
            grappleState = GrappleState.Idle; // 设置钩爪状态为待机
            isInvalidGrapple = false; 
            PlayerMovementController.Instance.StopGrapple();
        }
    }
}
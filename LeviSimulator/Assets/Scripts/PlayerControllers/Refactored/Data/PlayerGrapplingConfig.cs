using Sirenix.OdinInspector;
using UnityEngine;

namespace PlayerControllers.Refactored.Data
{
    [CreateAssetMenu(fileName = "PlayerGrapplingConfig", menuName = "Player/Grappling Config")]
    public class PlayerGrapplingConfig: ScriptableObject
    {
        [BoxGroup("钩锁枪设定")]
        [SerializeField][LabelText("钩爪抓取速度")] private float grappleSpeed = 20f; 
        [SerializeField][LabelText("钩爪最大抓取距离")]private float maxGrappleDistance = 20f; 
        [SerializeField][LabelText("钩爪抓取层")]private LayerMask grappleLayer=default; 
        [BoxGroup("钩爪设定")]
        [LabelText("弹力系数")]
        [SerializeField]private float springStrength = 60f; //越高牵引越快
        [LabelText("阻尼系数")]
        [SerializeField]private float damping = 20f;//越高越快减速
        [LabelText("进入此距离后，钩爪会自动断开")]
        [SerializeField]private float stableZone = 3f;
        [Title("钩爪自销毁检测")]
        [LabelText("是否启用玩家视角夹角销毁检测")]
        [SerializeField] private bool usePlayerViewAngleCheck = true;
        [ShowIf("usePlayerViewAngleCheck")] [LabelText("玩家视角夹角阈值")] 
        [SerializeField][Range(30f, 180f)] private float maxAllowedViewAngle=120;
        [Title("玩家操作相关")]
        [InfoBox("当前因子总和超过1f，请注意",InfoMessageType.Warning,"IsParametersOverflow")]
        [LabelText("是否启用玩家视角速度占比")]
        [SerializeField] private bool usePlayerViewParameters; 
        [ShowIf("usePlayerViewParameters")][LabelText("玩家视角占比因子")]
        [SerializeField][PropertyRange(0f,1f)]private float playerViewParameters=0.9f;
        [LabelText("是否启用玩家方向输入增益")]
        [SerializeField] private bool usePlayerInputDirection = true;
        [ShowIf("usePlayerInputDirection")] [LabelText("玩家输入增益力度")]
        [SerializeField]private float playerInputDirectionBuff = 1f; // 玩家输入方向增益的力度
        [ShowIf("usePlayerInputDirection")] [LabelText("玩家输入增益占比因子")]
        [SerializeField][PropertyRange(0f,1f)]private float playerInputDirectionParameters = 0.1f;
        [InfoBox("相关'因子'设定总和最好不超过1f，不然将有可能造成不良手感")]
        [ShowInInspector]private float totalParameters => playerViewParameters + playerInputDirectionParameters;

        public float GrappleSpeed => grappleSpeed;

        public float MaxGrappleDistance => maxGrappleDistance;

        public LayerMask GrappleLayer => grappleLayer;

        public float SpringStrength => springStrength;

        public float Damping => damping;

        public float StableZone => stableZone;

        public bool UsePlayerViewAngleCheck => usePlayerViewAngleCheck;

        public float MaxAllowedViewAngle => maxAllowedViewAngle;

        public bool UsePlayerViewParameters => usePlayerViewParameters;

        public float PlayerViewParameters => playerViewParameters;

        public bool UsePlayerInputDirection => usePlayerInputDirection;

        public float PlayerInputDirectionBuff => playerInputDirectionBuff;

        public float PlayerInputDirectionParameters => playerInputDirectionParameters;
    }
}
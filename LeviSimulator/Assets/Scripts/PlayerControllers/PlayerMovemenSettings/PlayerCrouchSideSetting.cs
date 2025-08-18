using Sirenix.OdinInspector;
using UnityEngine;

namespace PlayerControllers.PlayerMovemenSettings
{
    [ CreateAssetMenu(fileName = "PlayerCrouchSideSetting", menuName = "PlayerControllers/Settings/PlayerCrouchSideSetting")]
    public class PlayerCrouchSideSetting:ScriptableObject
    {
        public float SlideDeceleration => slideDeceleration;
        public float MinSlideSpeed => minSlideSpeed;
        public float SlideSpeedMultiplier => slideSpeedMultiplier;
        public float CrouchHeightDifference => crouchHeightDifference;
        public float MinSlopeAngle => minSlopeAngle;
        public float DownhillAccelScale => downhillAccelScale;
        public float UphillExtraDecel => uphillExtraDecel;
        public float StickToGroundForce => stickToGroundForce;
        public float SlopeSlideMinAngle => slopeSlideMinAngle;
        public float FallSpeedToSlideBoostMultiplier => fallSpeedToSlideBoostMultiplier;
        public float MinMaintainSlideSpeed => minMaintainSlideSpeed;

        [Header("移动设定-滑铲")]
        [SerializeField][LabelText("下落速度到滑铲速度的转换系数")] private float fallSpeedToSlideBoostMultiplier = 0.8f;
        [SerializeField][LabelText("滑铲时减速度")]private float slideDeceleration = 50f; // 滑铲时的减速度
        [SerializeField][LabelText("蹲伏速度")]private float minSlideSpeed = 2f; // 维持滑铲的最小速度
        [SerializeField][LabelText("滑铲时速度倍率")]private float slideSpeedMultiplier = 1.2f; // 滑铲时的速度倍率
        [SerializeField][LabelText("坡度启动滑铲的最小角度需求")]private float slopeSlideMinAngle = 10f; // 坡度启动滑铲的最小角度需求
        [SerializeField][LabelText("蹲伏站立的高度差")]private float crouchHeightDifference = 0.5f;
        [SerializeField][LabelText("认为是坡的最小角度")]private float minSlopeAngle = 2f;              // 认为是坡的最小角度
        [SerializeField][LabelText("下坡加速度放大倍率")]private float downhillAccelScale = 1.2f;       // 下坡加速度放大倍率
        [SerializeField][LabelText("上坡额外减速倍率")]private float uphillExtraDecel = 1.0f;         // 上坡额外减速倍率
        [SerializeField][LabelText("贴地力")]private float stickToGroundForce = 5f;         // 贴地力（防止弹起）
        [SerializeField][LabelText("维持滑铲的最低速度")]private float minMaintainSlideSpeed = 2f; // 维持滑铲的最低速度
    }
}
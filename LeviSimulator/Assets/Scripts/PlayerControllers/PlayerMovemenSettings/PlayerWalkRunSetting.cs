using Sirenix.OdinInspector;
using UnityEngine;

namespace PlayerControllers.PlayerMovemenSettings
{
    [ CreateAssetMenu(fileName = "PlayerWalkRunSetting", menuName = "PlayerControllers/Settings/PlayerWalkRunSetting", order = 1)]
    public class PlayerWalkRunSetting:ScriptableObject
    {
        public float MaxHorizontalSpeed => maxHorizontalSpeed;
        public float MaxRunSpeedMultiplier => maxRunSpeedMultiplier;
        public float Acceleration => acceleration;
        public float Deceleration => deceleration;
        public float AirAcceleration => airAcceleration;

        public float MaxAirSpeed => maxAirSpeed;
        public float JumpForce => jumpForce;
        public float CrouchSpeedMultiplier => crouchSpeedMultiplier;

        [Header("移动设定-步行/疾跑")]
        [SerializeField][LabelText("平地水平最大速度")]private float maxHorizontalSpeed = 7f; // 平地水平最大速度
        [SerializeField][LabelText("平地疾跑速度倍率")][MinValue(1f)]private float maxRunSpeedMultiplier = 1.5f; // 平地疾跑速度倍率
        [SerializeField][LabelText("平地加速度")]private float acceleration = 150f; // 加速度
        [SerializeField][LabelText("平地减速度")]private float deceleration = 100f; // 减速度（摩擦力）
        [SerializeField][LabelText("空中加速度")]private float airAcceleration = 15f; // 空中加速度
        [SerializeField][LabelText("空中最大速度")]private float maxAirSpeed = 5f; // 空中最大速度
        [Header("跳跃设定")]
        [SerializeField][LabelText("跳跃强度")]private float jumpForce = 10f;
        [SerializeField][LabelText("蹲伏时水平最大速度倍率")][MinValue(0.1f)]private float crouchSpeedMultiplier = 0.5f; // 蹲伏时水平最大速度倍率
    }
}
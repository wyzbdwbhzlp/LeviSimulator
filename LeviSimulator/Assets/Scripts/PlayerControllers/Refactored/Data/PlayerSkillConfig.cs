using Sirenix.OdinInspector;
using UnityEngine;

namespace PlayerControllers.Refactored.Data
{
    [CreateAssetMenu (fileName = "PlayerSkillConfig", menuName = "Player/Skill Config")]
    public class PlayerSkillConfig:ScriptableObject
    {
        [BoxGroup("冲刺设定")]
        [LabelText("冲刺持续时间")]
        [SerializeField] private float dashDuration = 1f;
        [LabelText("冲刺距离")]
        [SerializeField] private float dashDistance = 10f;
        [LabelText("冲刺冷却时间")]
        [SerializeField] private float dashCooldown = 15f;
        [LabelText("冲刺最大存储数")] [SerializeField]
        private int maxDashCount = 1;
        [SerializeField, Range(0f, 1f)][LabelText("冲刺方向保留")]
        private float verticalRetention = 0.5f; // 0 = 仅水平, 1 = 完全保留摄像机 y 分量
        [SerializeField, Min(0f)][LabelText("冲刺方向平滑")]
        private float dashDirectionSmoothing = 12f; // 越大过渡越快（推荐范围 8~18）
        [SerializeField, Min(0f)][LabelText( "冲刺结束时的速度衰减")]
        private float moveInputThreshold = 0.01f; // 摇杆抖动阈值
        [SerializeField]
        private bool useMaxVerticalAngle = true; // 是否限制最大垂直角度，防止被弹上天
        [SerializeField, Range(0f, 89f)][LabelText("最大冲刺角度（度数）")]
        private float maxVerticalAngleDeg = 60f; // 最大俯仰角度（度数）
        
        [BoxGroup("子弹时间")]
        [LabelText("子弹时间最大持续时间")]
        [SerializeField] private float maxBulletTimeEnergy = 5f;
        [LabelText("启用子弹时间最小持续时间")]
        [SerializeField][MaxValue("MaxBulletTimeEnergy")] private float minEnableBulletTimeEnergy = 0.5f;
        [LabelText("子弹时间冷却系数")]//tip 实际冷却时间 = 使用时间 * 冷却系数
        [SerializeField] private float bulletTimeCooldownFactor = 1f;
        [LabelText(" 子弹时间缩放系数")]
        [SerializeField] [Range(0.1f, 1f)] private float bulletTimeScale = 0.5f;
        
        
        public float DashDuration => dashDuration;
        public float DashDistance => dashDistance;
        public float DashCooldown => dashCooldown;
        public int MaxDashCount => maxDashCount;
        public float MaxBulletTimeEnergy => maxBulletTimeEnergy;
        public float MinEnableBulletTimeEnergy => minEnableBulletTimeEnergy;
        public float BulletTimeCooldownFactor => bulletTimeCooldownFactor;
        public float BulletTimeScale => bulletTimeScale;
        public float VerticalRetention => verticalRetention;
        public float DashDirectionSmoothing => dashDirectionSmoothing;
        public bool UseMaxVerticalAngle => useMaxVerticalAngle;
        public float MaxVerticalAngleDeg => maxVerticalAngleDeg;
        public float MoveInputThreshold => moveInputThreshold;
        

    }
}
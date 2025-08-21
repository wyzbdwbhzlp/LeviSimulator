using Sirenix.OdinInspector;
using UnityEngine;

namespace PlayerControllers.PlayerMovemenSettings
{
    [ CreateAssetMenu(fileName = "PlayerDetectionSetting", menuName = "ScriptableObjects/PlayerDetectionSetting", order = 1)]
    public class PlayerDetectionSetting:ScriptableObject
    {
        public float GroundCheckDistance => groundCheckDistance;

        public float GroundCheckRadius => groundCheckRadius;

        public LayerMask GroundLayerMask => groundLayerMask;
        
        public float WallMaxDistance => wallMaxDistance;
        public LayerMask WallLayerMask => wallLayerMask;
        
        [Header("地面检测")]
        [SerializeField][LabelText("地面检测")]private float groundCheckDistance = 1.85f; // 地面检测距离
        [SerializeField]private float groundCheckRadius = 0.4f; // 球体半径
        [SerializeField]private LayerMask groundLayerMask; // 地面层
        [Header("墙面检测")]
        [SerializeField] [LabelText("爬墙检测距离")] private float wallMaxDistance = 1f;
        [SerializeField] [LabelText("墙面层")] private LayerMask wallLayerMask;
    }
}
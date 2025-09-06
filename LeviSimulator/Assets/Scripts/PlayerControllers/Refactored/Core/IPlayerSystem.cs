using PlayerControllers.Refactored.Data;
using UnityEngine;

namespace PlayerControllers.Refactored.Core
{
    /// <summary>
    /// 玩家系统基础接口
    /// </summary>
    public interface IPlayerSystem
    {
        void Initialize(PlayerController playerController,PlayerMovementConfig playerConfig);
        void Update();
        void FixedUpdate();
        void Cleanup();
        bool IsEnabled { get; set; }
    }
}

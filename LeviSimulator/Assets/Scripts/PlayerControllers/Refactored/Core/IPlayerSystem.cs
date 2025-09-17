using PlayerControllers.Refactored.Data;
using UnityEngine;

namespace PlayerControllers.Refactored.Core
{
    /// <summary>
    /// 玩家系统基础接口
    /// </summary>
    public interface IPlayerSystem
    {
        void Initialize(PlayerController playerController,ScriptableObject playerConfig);
        void Update();
        void FixedUpdate();
        void CleanUp();
        

        bool IsEnabled { get; set; }
    }
}

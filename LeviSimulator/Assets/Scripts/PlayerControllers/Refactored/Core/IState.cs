using UnityEngine;

namespace PlayerControllers.Refactored.Core
{
    /// <summary>
    /// 玩家状态枚举
    /// </summary>
    public enum PlayerState
    {
        Idle,
        Walking,
        Running,
        Crouching,
        Sliding,
        Jumping,
        Falling,
        Grappling,
        WallRunning,
        Dashing
    }

    /// <summary>
    /// 状态接口
    /// </summary>
    public interface IState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();
        bool CanTransitionTo(PlayerState targetState);
    }

    /// <summary>
    /// 状态机接口
    /// </summary>
    public interface IStateMachine
    {
        PlayerState CurrentState { get; }
        void ChangeState(PlayerState newState);
        void RegisterState(PlayerState state, IState stateInstance);
        void Update();
        void FixedUpdate();
    }
}

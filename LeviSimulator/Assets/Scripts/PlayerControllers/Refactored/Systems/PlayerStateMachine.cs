using System.Collections.Generic;
using UnityEngine;
using PlayerControllers.Refactored.Core;

namespace PlayerControllers.Refactored.Systems
{
    /// <summary>
    /// 简化的状态机实现
    /// </summary>
    public class PlayerStateMachine : IStateMachine
    {
        private Dictionary<PlayerState, IState> _states = new Dictionary<PlayerState, IState>();
        private IState _currentStateInstance;
        
        public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
        
        public void RegisterState(PlayerState state, IState stateInstance)
        {
            if (!_states.ContainsKey(state))
            {
                _states[state] = stateInstance;
            }
        }
        
        public void ChangeState(PlayerState newState)
        {
            if (!_states.ContainsKey(newState))
            {
                Debug.LogError($"State {newState} is not registered!");
                return;
            }
            
            if (CurrentState == newState) return;
            
            // 检查是否可以转换到目标状态
            if (_currentStateInstance != null && !_currentStateInstance.CanTransitionTo(newState))
            {
                return;
            }
            
            // 退出当前状态
            _currentStateInstance?.Exit();
            
            // 切换到新状态
            CurrentState = newState;
            _currentStateInstance = _states[newState];
            
            // 进入新状态
            _currentStateInstance?.Enter();
            
            Debug.Log($"State changed to: {newState}");
        }
        
        public void Update()
        {
            _currentStateInstance?.Update();
        }
        
        public void FixedUpdate()
        {
            _currentStateInstance?.FixedUpdate();
        }
        
        public void Initialize(PlayerState initialState)
        {
            if (_states.ContainsKey(initialState))
            {
                CurrentState = initialState;
                _currentStateInstance = _states[initialState];
                _currentStateInstance?.Enter();
            }
        }
        
        public void Cleanup()
        {
            _currentStateInstance?.Exit();
            _currentStateInstance = null;
            _states.Clear();
        }
    }
}

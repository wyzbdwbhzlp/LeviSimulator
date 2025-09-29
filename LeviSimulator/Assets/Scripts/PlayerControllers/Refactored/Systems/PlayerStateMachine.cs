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
        private Animator _playerAnimator;
        private PlayerController _playerController;
        public PlayerState CurrentState { get; private set; } = PlayerState.Idle;
        public IState CurrentStateInstance => _currentStateInstance;

        public PlayerStateMachine(PlayerController playerController)
        {
            _playerController = playerController;
            _playerAnimator = playerController.PlayerAnimator;
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

        public void RegisterState(PlayerState state, IState stateInstance)
        {
            if (!_states.ContainsKey(state))
            {
                _states[state] = stateInstance;
            }
        }
        
        /// <summary>
        ///  切换状态
        /// </summary>
        /// <param name="newState"></param>
        public void ChangeState(PlayerState newState)
        {
            if (!_states.ContainsKey(newState))
            {
                Debug.LogError($"State {newState} 未注册");
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
            // 同步动画器状态
            UpdateAnimatorState(newState);
            
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
        
                /// <summary>
        /// 更新动画器状态以匹配代码状态机
        /// </summary>
        private void UpdateAnimatorState(PlayerState state)
        {
            void SetWallRunningAnimation(bool isWallRunning)
            {
                if (isWallRunning)
                {
                    bool isWallRunningLeft = _playerController.RuntimeData.GetWallSide(_playerController.transform) > 0;
                    _playerAnimator.SetTrigger("WallRun");
                    if (isWallRunningLeft)
                    {
                        _playerAnimator.SetBool("IsWallRunningLeft", isWallRunning);
                        _playerAnimator.SetBool("IsWallRunningRight", !isWallRunning);
                    }
                    else
                    {
                        _playerAnimator.SetBool("IsWallRunningRight", isWallRunning);
                        _playerAnimator.SetBool("IsWallRunningLeft", !isWallRunning);
                    }
                }
                else
                {
                    _playerAnimator.SetBool("IsWallRunningLeft", false);
                    _playerAnimator.SetBool("IsWallRunningRight", false);
                }
            }
            
            
            if (_playerAnimator == null) return;
            
            // 设置动画器参数来触发对应的动画状态
            switch (state)
            {
                case PlayerState.Idle:
                    _playerAnimator.SetBool("IsWalking", false);
                    _playerAnimator.SetBool("IsRunning", false);
                    _playerAnimator.SetBool("IsSliding", false);
                    _playerAnimator.SetBool("IsJumping", false);
                    SetWallRunningAnimation(false);
                    break;
                    
                case PlayerState.Walking:
                    _playerAnimator.SetBool("IsWalking", true);
                    _playerAnimator.SetBool("IsRunning", false);
                    _playerAnimator.SetBool("IsCrouching", false);
                    SetWallRunningAnimation(false);
                    break;
                    
                case PlayerState.Running:
                    _playerAnimator.SetBool("IsWalking", false);
                    _playerAnimator.SetBool("IsRunning", true);
                    _playerAnimator.SetBool("IsCrouching", false);
                    SetWallRunningAnimation(false);
                    break;
                    
                case PlayerState.Jumping:
                    _playerAnimator.SetTrigger("Jump");
                    _playerAnimator.SetBool("IsJumping", true);
                    SetWallRunningAnimation(false);
                    break;
                    
                case PlayerState.Falling:
                    _playerAnimator.SetBool("IsWalking", false);
                    _playerAnimator.SetBool("IsRunning", false);
                    _playerAnimator.SetBool("IsSliding", false);
                    _playerAnimator.SetBool("IsJumping", false);
                    break;
                    
                case PlayerState.Sliding:
                    _playerAnimator.SetBool("IsSliding",true);
                    _playerAnimator.SetBool("IsWalking", false);
                    _playerAnimator.SetBool("IsRunning", false);
                    break;
                case PlayerState.Crouching:
                    _playerAnimator.SetBool("IsWalking", false);
                    _playerAnimator.SetBool("IsRunning", false);
                    _playerAnimator.SetBool("IsSliding", false);
                    break;
                    
                case PlayerState.WallRunning:
                    SetWallRunningAnimation(true);
                    break;
                    
                case PlayerState.Grappling:
                    _playerAnimator.SetBool("IsGrappling", true);
                    break;
                case PlayerState.Dashing:
                    _playerAnimator.SetTrigger("Dash");
                    break;
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

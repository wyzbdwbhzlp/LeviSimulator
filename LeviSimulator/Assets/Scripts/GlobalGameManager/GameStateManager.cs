using UnityEngine;
using System;

public enum GameState
{
    MainMenu,
    Loading,
    InGame,
    Paused,
    GameOver,
    Settings
}

public class GameStateManager : MonoBehaviour
{
    [Header("游戏状态")]
    public GameState currentState = GameState.MainMenu;
    public GameState previousState = GameState.MainMenu;

    public event Action<GameState, GameState> OnStateChanged;
    public event Action OnGamePaused;
    public event Action OnGameResumed;

    public void Initialize()
    {
        Debug.Log("GameStateManager 初始化完成");
    }

    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        GameState oldState = currentState;
        previousState = currentState;
        currentState = newState;

        OnStateChanged?.Invoke(oldState, newState);
        
        // 处理状态切换逻辑
        HandleStateChange(oldState, newState);
        
        Debug.Log($"游戏状态从 {oldState} 切换到 {newState}");
    }

    private void HandleStateChange(GameState from, GameState to)
    {
        switch (to)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;
            case GameState.Loading:
                Time.timeScale = 1f;
                break;
            case GameState.InGame:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                OnGamePaused?.Invoke();
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
            case GameState.Settings:
                // 保持之前的时间缩放
                break;
        }
    }

    public void PauseGame()
    {
        if (currentState == GameState.InGame)
        {
            ChangeState(GameState.Paused);
        }
    }

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            ChangeState(GameState.InGame);
            OnGameResumed?.Invoke();
        }
    }

    public void StartGame()
    {
        ChangeState(GameState.InGame);
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
    }

    public void ReturnToPreviousState()
    {
        ChangeState(previousState);
    }

    public bool IsInGame()
    {
        return currentState == GameState.InGame;
    }

    public bool IsPaused()
    {
        return currentState == GameState.Paused;
    }

    private void Update()
    {
        // ESC键暂停/恢复游戏
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.InGame)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }
}

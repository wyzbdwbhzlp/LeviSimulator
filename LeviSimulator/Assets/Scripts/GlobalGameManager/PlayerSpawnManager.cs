using System;
using System.Collections;
using System.Collections.Generic;
using Game.Audio;
using PlayerControllers.Refactored;
using PlayerControllers.Refactored.Core;
using PlayerControllers.Refactored.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace GlobalGameManager
{
    public struct PlayerSpawnInfo
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public PlayerSpawnInfo(Vector3 pos, Quaternion rot)
        {
            Position = pos;
            Rotation = rot;
        }
    }
    public class PlayerSpawnManager : MonoBehaviour
    {
        [Header("玩家配置")]
        [SerializeField] private GameObject playerPrefab;

        [Header("生成设置")]
        public bool autoSpawnOnSceneLoad = true;
        public float spawnDelay = 1f;


        [ShowInInspector] private PlayerSpawnInfo _initialSpawnInfo;
        [ShowInInspector] private PlayerSpawnInfo _rebirthSpawnInfo;
        
        private GameStateManager _gameStateManager;//用于改变游戏状态
        
        // 状态追踪
        private bool _isSpawning = false;
        private Coroutine _currentSpawnCoroutine;
        private bool _isInitialized = false;

        /// <summary>
        ///  当玩家准备生成时触发，提供生成位置
        /// </summary>
        public event Action<GameObject> OnPlayerSpawned;
        /// <summary>
        ///  当玩家被销毁时触发
        /// </summary>
        public event Action<GameObject> OnPlayerDespawned;
        /// <summary>
        ///  当玩家复活时触发
        /// </summary>
        public event Action<GameObject> OnPlayerRebirth;

        private GameObject _currentPlayer;
        private readonly List<GameObject> _spawnedPlayers = new List<GameObject>();

        public void Initialize(GameStateManager gameStateManager)
        {
            if (_isInitialized)
            {
                LogUtil.LogWarning("PlayerSpawnManager 已经初始化过了");
                return;
            }
            
            if (!ValidateConfiguration())
            {
                return;
            }

            _gameStateManager = gameStateManager;
            
            // 统一订阅事件
            SubscribeToEvents();
            
            _isInitialized = true;
            LogUtil.Log("PlayerSpawnManager 初始化完成");
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
            
            // 停止所有正在运行的协程
            if (_currentSpawnCoroutine != null)
            {
                StopCoroutine(_currentSpawnCoroutine);
                _currentSpawnCoroutine = null;
            }
        }

        private void OnSceneLoaded(SceneEnum sceneEnum)
        {
            //tip 注意若是该场景存在挂载了SpawnPointItem的物体，则会自动设置生成位置
            Invoke(nameof(SpawnPlayer), spawnDelay);

        }
        private void SetInitialSpawnInfoWhenSceneReady(Vector3 position, Quaternion rotation)
        {
            _initialSpawnInfo = new PlayerSpawnInfo(position, rotation);
        }
        private void SetRebirthSpawnInfo(Vector3 position, Quaternion rotation)
        {
            _rebirthSpawnInfo = new PlayerSpawnInfo(position, rotation);
        }

        /// <summary>
        ///  场景加载完毕后生成玩家
        /// </summary>
        private void SpawnPlayer()
        {
            if (_isSpawning)
            {
                LogUtil.LogWarning("玩家正在生成中，忽略重复请求");
                return;
            }
            
            if (!ValidateSpawnInfo(_initialSpawnInfo, "初始"))
            {
                return;
            }
            
            StartPlayerSpawn(_initialSpawnInfo, false);
        }
        private IEnumerator CreatePlayerBySpawnInfo(PlayerSpawnInfo spawnInfo, bool isRebirth = false)
        {
            _isSpawning = true;
      
            try
            {
                var spawnPosition = spawnInfo.Position;
                var spawnRotation = spawnInfo.Rotation;
                
                GameObject player = Instantiate(playerPrefab, spawnPosition, spawnRotation);
                player.name = isRebirth ? "Player (Reborn)" : "Player";
                
                _currentPlayer = player;
                _spawnedPlayers.Add(player);
                
                LogUtil.Log($"玩家{(isRebirth ? "复活" : "生成")}在位置: {spawnPosition}");
                
                // 初始化玩家
                yield return StartCoroutine(InitializePlayer(player));
                
                // 触发相应事件
                if (isRebirth)
                {
                    OnPlayerRebirth?.Invoke(player);
                    EventBroadcaster.CallPlayerRebirth(player);
                }
                else
                {
                    OnPlayerSpawned?.Invoke(player);
                }
            }
            finally
            {
                _isSpawning = false;
                _currentSpawnCoroutine = null;
            }
        }

        public void PlayerIsDeath()
        {
            AudioEventHandler.CallPlayOneShotFor2D(AudioNames.女人死亡);
            _gameStateManager.ChangeState(GameState.GameOver);// 切换到游戏结束状态
            //tip 黑屏hud会接受来自GameOver状态的事件，然后监听玩家复活输入
        }

        /// <summary>
        ///  复活玩家
        /// </summary>
        public void RebirthPlayer()
        {
            if (_isSpawning)
            {
                LogUtil.LogWarning("玩家正在生成中，无法执行复活");
                return;
            }
            
            // 清理当前玩家
            if (_currentPlayer != null)
            {
                DespawnPlayer(_currentPlayer);
            }
            
            // 确定复活点
            var spawnInfo = _rebirthSpawnInfo;
            if (!ValidateSpawnInfo(spawnInfo, "复活"))
            {
                LogUtil.Log("玩家存档点未设置，使用初始生成点");
                spawnInfo = _initialSpawnInfo;
                if (!ValidateSpawnInfo(spawnInfo, "初始"))
                {
                    LogUtil.LogError("无法找到有效的生成点，复活失败");
                    return;
                }
            }
            
            StartPlayerSpawn(spawnInfo, true);
        }
        private IEnumerator InitializePlayer(GameObject player)
        {
            if (player == null)
            {
                LogUtil.LogError("InitializePlayer: 玩家对象为null");
                yield break;
            }
            
            yield return new WaitForEndOfFrame();

            var playerController = player.GetComponent<PlayerController>();
            if (playerController == null)
            {
                LogUtil.LogError($"玩家对象 {player.name} 缺少 PlayerController 组件");
                yield break;
            }
            
                // 重新初始化玩家控制器
                playerController.Initialize();

                // 确保状态机正确启动
                yield return new WaitForEndOfFrame();

                LogUtil.Log($"玩家 {player.name} 初始化完成");
       
        }
        /// <summary>
        ///  销毁玩家
        /// </summary>
        /// <param name="player"></param>
        public void DespawnPlayer(GameObject player)
        {
            if (player != null && _spawnedPlayers.Contains(player))
            {
                _spawnedPlayers.Remove(player);
                if (_currentPlayer == player)
                    _currentPlayer = null;
                PlayerInputEvents.ClearAllEvents();
                Destroy(player);
                OnPlayerDespawned?.Invoke(player);
            }
        }

        public void DespawnAllPlayers()
        {
            for (int i = _spawnedPlayers.Count - 1; i >= 0; i--)
            {
                DespawnPlayer(_spawnedPlayers[i]);
            }
        }


        public GameObject GetCurrentPlayer()
        {
            return _currentPlayer;
        }

        public List<GameObject> GetAllPlayers()
        {
            return new List<GameObject>(_spawnedPlayers);
        }
        
        #region 私有辅助方法
        
        /// <summary>
        /// 验证配置有效性
        /// </summary>
        private bool ValidateConfiguration()
        {
            if (playerPrefab == null)
            {
                LogUtil.LogError("PlayerSpawnManager: 玩家预制体未设置！请在Inspector中分配playerPrefab", true);
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// 验证生成点信息有效性
        /// </summary>
        private bool ValidateSpawnInfo(PlayerSpawnInfo spawnInfo, string spawnType)
        {
            if (spawnInfo.Position == Vector3.zero)
            {
                LogUtil.LogWarning($"PlayerSpawnManager: {spawnType}生成点未设置，请通过相应事件设置生成点");
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// 统一订阅事件
        /// </summary>
        private void SubscribeToEvents()
        {
            if (autoSpawnOnSceneLoad)
            {
                if (GlobalManager.Instance?.sceneLoadManager != null)
                {
                    GlobalManager.Instance.sceneLoadManager.OnSceneLoadCompleted += OnSceneLoaded;
                }
                else
                {
                    LogUtil.LogWarning("GlobalManager.sceneLoadManager 不可用，无法订阅场景加载事件");
                }
                
                EventBroadcaster.OnPlayerReadySpawn += SetInitialSpawnInfoWhenSceneReady;
                EventBroadcaster.OnUpdatePlayerCheckPoint += SetRebirthSpawnInfo;
            }
        }
        
        /// <summary>
        /// 统一取消订阅事件
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (GlobalManager.Instance?.sceneLoadManager != null)
            {
                GlobalManager.Instance.sceneLoadManager.OnSceneLoadCompleted -= OnSceneLoaded;
            }
            
            EventBroadcaster.OnPlayerReadySpawn -= SetInitialSpawnInfoWhenSceneReady;
            EventBroadcaster.OnUpdatePlayerCheckPoint -= SetRebirthSpawnInfo;
        }
        
        /// <summary>
        /// 开始玩家生成流程
        /// </summary>
        private void StartPlayerSpawn(PlayerSpawnInfo spawnInfo, bool isRebirth)
        {
            // 取消之前的生成协程
            if (_currentSpawnCoroutine != null)
            {
                StopCoroutine(_currentSpawnCoroutine);
            }
            
            _currentSpawnCoroutine = StartCoroutine(CreatePlayerBySpawnInfo(spawnInfo, isRebirth));
        }
        
        #endregion
    }
}

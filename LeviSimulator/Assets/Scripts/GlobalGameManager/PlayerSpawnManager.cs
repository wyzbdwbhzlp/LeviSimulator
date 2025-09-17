using System;
using System.Collections;
using System.Collections.Generic;
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

        public void Initialize()
        {
            if (autoSpawnOnSceneLoad)
            {
                // 监听场景加载完成事件
                GlobalManager.Instance.sceneLoadManager.OnSceneLoadCompleted += OnSceneLoaded;
                EventBroadcaster.OnPlayerReadySpawn += SetInitialSpawnInfoWhenSceneReady;
                EventBroadcaster.OnUpdatePlayerCheckPoint += SetRebirthSpawnInfo;
            }
            Debug.Log("PlayerSpawnManager 初始化完成");
        }

        private void OnDisable()
        {
            if (GlobalManager.Instance != null && GlobalManager.Instance.sceneLoadManager != null)
            {
                GlobalManager.Instance.sceneLoadManager.OnSceneLoadCompleted -= OnSceneLoaded;
            }
            EventBroadcaster.OnPlayerReadySpawn -= SetInitialSpawnInfoWhenSceneReady;
            EventBroadcaster.OnUpdatePlayerCheckPoint -= SetRebirthSpawnInfo;
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
        /// <returns></returns>
        private void SpawnPlayer()
        {
            if (_initialSpawnInfo.Position == Vector3.zero)
            {
                LogUtil.LogWarning("玩家生成点未设置，请通过EventBroadcaster.OnPlayerReadySpawn事件设置生成点");
            }
            StartCoroutine(CreatePlayerBySpawnInfo(_initialSpawnInfo));
        }
        private IEnumerator CreatePlayerBySpawnInfo(PlayerSpawnInfo spawnInfo,bool isRebirth=false)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("玩家预制体未设置！");
                yield  break;
            }
            var spawnPosition = spawnInfo.Position;
            var spawnRotation = spawnInfo.Rotation;
            GameObject player = Instantiate(playerPrefab, spawnPosition, spawnRotation);
            player.name = "Player";
            
            _currentPlayer = player;
            _spawnedPlayers.Add(player);
            Debug.Log($"玩家已生成在位置: {spawnPosition}");
            yield return StartCoroutine(DelayedInputSetup(player));
            if (isRebirth)
            {
                OnPlayerRebirth?.Invoke(player);
            }
            else
            {
                OnPlayerSpawned?.Invoke(player);
            }
        }
        /// <summary>
        ///  复活玩家
        /// </summary>
        /// <returns></returns>
        public void RebirthPlayer()
        {
            var spawnInfo = _rebirthSpawnInfo;
            if (_currentPlayer != null)
                DespawnPlayer(_currentPlayer);
            if (spawnInfo.Position == Vector3.zero)
            {
                LogUtil.Log("玩家存档点未设置，使用初始生成点");
                spawnInfo = _initialSpawnInfo;
            }
            StartCoroutine(CreatePlayerBySpawnInfo(spawnInfo,true));
            
        }
        private IEnumerator DelayedInputSetup(GameObject player)
        {
            yield return new WaitForEndOfFrame();

            var playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // 重新初始化玩家控制器
                playerController.Initialize();

                // 确保状态机正确启动
                yield return new WaitForEndOfFrame();

                LogUtil.Log("玩家完全重新初始化完成");
            }
            
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
    }
}

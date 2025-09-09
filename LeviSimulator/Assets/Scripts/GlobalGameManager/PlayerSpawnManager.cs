using System;
using System.Collections.Generic;
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
        [SerializeField]private GameObject playerPrefab;

        [Header("生成设置")]
        public bool autoSpawnOnSceneLoad = true;
        public float spawnDelay = 1f;


        private PlayerSpawnInfo _initialSpawnInfo;
        private PlayerSpawnInfo _rebirthSpawnInfo;

        /// <summary>
        ///  当玩家准备生成时触发，提供生成位置
        /// </summary>
        public event Action<GameObject> OnPlayerSpawned;
        public event Action<GameObject> OnPlayerDespawned;

        private GameObject _currentPlayer;
        private readonly List<GameObject> _spawnedPlayers = new List<GameObject>();

        public void Initialize()
        {
            if (autoSpawnOnSceneLoad)
            {
                // 监听场景加载完成事件
                GlobalManager.Instance.sceneLoadManager.OnSceneLoadCompleted += OnSceneLoaded;
                EventBroadcaster.OnPlayerReadySpawn+= SetInitialSpawnInfoWhenSceneReady;
                EventBroadcaster.OnUpdatePlayerCheckPoint+= SetRebirthSpawnInfo;
            }
            Debug.Log("PlayerSpawnManager 初始化完成");
        }

        private void OnDisable()
        {
            if (GlobalManager.Instance!=null && GlobalManager.Instance.sceneLoadManager!=null)
            {
                GlobalManager.Instance.sceneLoadManager.OnSceneLoadCompleted -= OnSceneLoaded;
            }
            EventBroadcaster.OnPlayerReadySpawn-= SetInitialSpawnInfoWhenSceneReady;
            EventBroadcaster.OnUpdatePlayerCheckPoint-= SetRebirthSpawnInfo;
        }

        private void OnSceneLoaded(string sceneName)
        {
            
            Invoke(nameof(SpawnPlayer), spawnDelay);
            
        }
        private void SetInitialSpawnInfoWhenSceneReady(Vector3 position, Quaternion rotation)
        {
             _initialSpawnInfo=new PlayerSpawnInfo(position, rotation);
        }
        private void SetRebirthSpawnInfo(Vector3 position, Quaternion rotation)
        {
            _rebirthSpawnInfo=new PlayerSpawnInfo(position, rotation);
        }

        /// <summary>
        ///  场景加载完毕后生成玩家
        /// </summary>
        /// <returns></returns>
        private GameObject SpawnPlayer()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("玩家预制体未设置！");
                return null;
            }
            if(_initialSpawnInfo.Position==Vector3.zero && _initialSpawnInfo.Rotation==Quaternion.identity)
            {
                LogUtil.Log("玩家生成点未设置，请通过EventBroadcaster.OnPlayerReadySpawn事件设置生成点");
                
              
            }
            var spawnInfo=_initialSpawnInfo;
            var spawnPosition=spawnInfo.Position;
            var spawnRotation=spawnInfo.Rotation;
            GameObject player = Instantiate(playerPrefab, spawnPosition, spawnRotation);
            player.name = "Player";

            _currentPlayer = player;
            _spawnedPlayers.Add(player);

            OnPlayerSpawned?.Invoke(player);
            Debug.Log($"玩家已生成在位置: {spawnPosition}");
            

            return player;
        }
        /// <summary>
        ///  复活玩家
        /// </summary>
        /// <returns></returns>
        public GameObject RebirthPlayer()
        {
            if(_rebirthSpawnInfo.Position==Vector3.zero && _rebirthSpawnInfo.Rotation==Quaternion.identity)
            {
                LogUtil.Log("玩家复活点未设置，使用初始生成点");
                return SpawnPlayer();
            }
            else
            {
                if(_currentPlayer!=null)
                    DespawnPlayer(_currentPlayer);
                
                var spawnInfo=_rebirthSpawnInfo;
                var spawnPosition=spawnInfo.Position;
                var spawnRotation=spawnInfo.Rotation;
                GameObject player = Instantiate(playerPrefab, spawnPosition, spawnRotation);
                player.name = "Player";

                _currentPlayer = player;
                _spawnedPlayers.Add(player);

                OnPlayerSpawned?.Invoke(player);
                Debug.Log($"玩家已复活在位置: {spawnPosition}");
                
                return player;
            }
        }
        public void DespawnPlayer(GameObject player)
        {
            if (player != null && _spawnedPlayers.Contains(player))
            {
                _spawnedPlayers.Remove(player);
                if (_currentPlayer == player)
                    _currentPlayer = null;

                OnPlayerDespawned?.Invoke(player);
                Destroy(player);
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

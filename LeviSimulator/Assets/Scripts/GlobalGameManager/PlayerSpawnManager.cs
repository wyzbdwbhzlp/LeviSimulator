using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace GlobalGameManager
{
    public class PlayerSpawnManager : MonoBehaviour
    {
        [Header("玩家配置")]
        [SerializeField]private GameObject playerPrefab;
        private Vector3 spawnPosition = Vector3.zero;
        private Quaternion spawnRotation = Quaternion.identity;

        [Header("生成设置")]
        public bool autoSpawnOnSceneLoad = true;
        public float spawnDelay = 1f;

        /// <summary>
        ///  当玩家准备生成时触发，提供生成位置
        /// </summary>
        public event Action<GameObject> OnPlayerSpawned;
        public event Action<GameObject> OnPlayerDespawned;

        private GameObject currentPlayer;
        private List<GameObject> spawnedPlayers = new List<GameObject>();

        public void Initialize()
        {
            if (autoSpawnOnSceneLoad)
            {
                // 监听场景加载完成事件
                GlobalManager.Instance.sceneLoadManager.OnSceneLoadCompleted += OnSceneLoaded;
                EventBroadcaster.OnPlayerReadySpawn+= SetSpawnPostionRotation;
            }
            Debug.Log("PlayerSpawnManager 初始化完成");
        }

        private void OnSceneLoaded(string sceneName)
        {
            
            Invoke(nameof(SpawnPlayer), spawnDelay);
            
        }
        private void SetSpawnPostionRotation(Vector3 position,Quaternion rotation)
        {
            spawnPosition = position;
            spawnRotation = rotation;
        }

        public GameObject SpawnPlayer()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("玩家预制体未设置！");
                return null;
            }
            if(spawnPosition==Vector3.zero||spawnRotation==Quaternion.identity)
            {
                LogUtil.LogWarning("玩家生成位置或旋转未设置");
            }
            
            GameObject player = Instantiate(playerPrefab, spawnPosition, spawnRotation);
            player.name = "Player";

            currentPlayer = player;
            spawnedPlayers.Add(player);

            OnPlayerSpawned?.Invoke(player);
            Debug.Log($"玩家已生成在位置: {spawnPosition}");
            
            spawnPosition=Vector3.zero;// 重置参数
            spawnRotation=Quaternion.identity;

            return player;
        }
        public void DespawnPlayer(GameObject player)
        {
            if (player != null && spawnedPlayers.Contains(player))
            {
                spawnedPlayers.Remove(player);
                if (currentPlayer == player)
                    currentPlayer = null;

                OnPlayerDespawned?.Invoke(player);
                Destroy(player);
            }
        }

        public void DespawnAllPlayers()
        {
            for (int i = spawnedPlayers.Count - 1; i >= 0; i--)
            {
                DespawnPlayer(spawnedPlayers[i]);
            }
        }

    
        

        public GameObject GetCurrentPlayer()
        {
            return currentPlayer;
        }

        public List<GameObject> GetAllPlayers()
        {
            return new List<GameObject>(spawnedPlayers);
        }
    }
}

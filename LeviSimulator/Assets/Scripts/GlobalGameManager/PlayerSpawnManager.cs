using System;
using System.Collections.Generic;
using UnityEngine;

namespace GlobalGameManager
{
    public class PlayerSpawnManager : MonoBehaviour
    {
        [Header("玩家配置")]
        public GameObject playerPrefab;
        public Transform[] spawnPoints;
        public Vector3 defaultSpawnPosition = Vector3.zero;

        [Header("生成设置")]
        public bool autoSpawnOnSceneLoad = true;
        public float spawnDelay = 1f;

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
            }
            Debug.Log("PlayerSpawnManager 初始化完成");
        }

        private void OnSceneLoaded(string sceneName)
        {
            
            Invoke(nameof(SpawnPlayer), spawnDelay);
            
        }

        public GameObject SpawnPlayer()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("玩家预制体未设置！");
                return null;
            }

            Vector3 spawnPosition = GetSpawnPosition();
            Quaternion spawnRotation = GetSpawnRotation();

            GameObject player = Instantiate(playerPrefab, spawnPosition, spawnRotation);
            player.name = "Player";

            currentPlayer = player;
            spawnedPlayers.Add(player);

            OnPlayerSpawned?.Invoke(player);
            Debug.Log($"玩家已生成在位置: {spawnPosition}");

            return player;
        }

        public GameObject SpawnPlayerAt(Vector3 position, Quaternion rotation)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("玩家预制体未设置！");
                return null;
            }

            GameObject player = Instantiate(playerPrefab, position, rotation);
            player.name = "Player";

            currentPlayer = player;
            spawnedPlayers.Add(player);

            OnPlayerSpawned?.Invoke(player);
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

        private Vector3 GetSpawnPosition()
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                GameObject[] spawnPointsArr = GameObject.FindGameObjectsWithTag("PlayerSpawnPoint");
                if (spawnPoints.Length > 1)
                {
                    Debug.LogWarning("场景中存在多个标记为 'PlayerSpawnPoint' 的生成点，建议只保留一个以避免冲突。");
                }

                return spawnPointsArr[0].transform.position;
            }
            Debug.LogWarning("未找到生成点，使用默认位置。");
            return defaultSpawnPosition;
        }

        private Quaternion GetSpawnRotation()
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                Transform randomSpawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
                return randomSpawnPoint.rotation;
            }
            return Quaternion.identity;
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

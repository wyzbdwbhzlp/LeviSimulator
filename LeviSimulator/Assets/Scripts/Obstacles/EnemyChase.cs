using GlobalGameManager;
using UnityEngine;
using Utilities;

public class EnemyChase : MonoBehaviour
{
    [Header("追逐设置")]
    public float speed = 3f;
    public float stopDistance = 1.5f;

    private Transform player;
    private bool canChase = true;

    private void Start()
    {
        TryFindPlayer();
    }

    private void OnEnable()
    {
        EventBroadcaster.PlayerEnterCheckPoint += StopChasing;
        EventBroadcaster.PlayerExitCheckPoint += ResumeChasing;
        EventBroadcaster.OnGameOver += ResetToSpawnPoint;
        EventBroadcaster.OnPlayerRebirth += HandlePlayerRebirth;
    }

    private void OnDisable()
    {
        EventBroadcaster.PlayerEnterCheckPoint -= StopChasing;
        EventBroadcaster.PlayerExitCheckPoint -= ResumeChasing;
        EventBroadcaster.OnGameOver -= ResetToSpawnPoint;
        EventBroadcaster.OnPlayerRebirth -= HandlePlayerRebirth;
    }

    private void Update()
    {
        if (player == null)
        {
            TryFindPlayer(); // 没找到就持续尝试
            return;
        }
        if (!canChase) return;

        Vector3 direction = (player.position - transform.position).normalized;

        if (Vector3.Distance(transform.position, player.position) > stopDistance)
        {
            transform.position += direction * speed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void TryFindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("敌人已找到玩家: " + player.name);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 玩家死亡
            GlobalManager.Instance.playerSpawnManager.PlayerIsDeath();
        }
    }

    private void StopChasing()
    {
        canChase = false;
        Debug.Log("敌人停止追逐（玩家进入存档点）");
    }

    private void ResumeChasing()
    {
        canChase = true;
        Debug.Log("敌人继续追逐（玩家离开存档点 / 玩家复活）");
    }

    /// <summary>
    /// 玩家复活时：敌人回到最近刷新点，并重新追逐
    /// </summary>
    private void HandlePlayerRebirth(GameObject newPlayer)
    {
        player = newPlayer.transform;
        canChase = true;

        Transform closest = FindClosestSpawnPoint(player.position);
        if (closest != null)
        {
            transform.position = closest.position;
            transform.rotation = closest.rotation;
            Debug.Log($"{name} 玩家复活 → 敌人刷新在 {transform.position}");
        }
        else
        {
            Debug.LogWarning("没有找到敌人出生点，敌人无法复位");
        }
    }

    /// <summary>
    /// 玩家死亡 → 敌人复位
    /// </summary>
    private void ResetToSpawnPoint()
    {
        canChase = false;

        if (player != null)
        {
            Transform closest = FindClosestSpawnPoint(player.position);
            if (closest != null)
            {
                transform.position = closest.position;
                transform.rotation = closest.rotation;
                Debug.Log($"{name} 玩家死亡 → 敌人回到最近出生点 {transform.position}");
            }
        }
    }

    /// <summary>
    /// 找到距离玩家最近的敌人出生点
    /// </summary>
    private Transform FindClosestSpawnPoint(Vector3 playerPos)
    {
        GameObject[] allPoints = GameObject.FindGameObjectsWithTag("EnemySpawnPoint");
        Transform closest = null;
        float minDist = float.MaxValue;

        foreach (var go in allPoints)
        {
            float dist = Vector3.Distance(playerPos, go.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = go.transform;
            }
        }
        return closest;
    }
}

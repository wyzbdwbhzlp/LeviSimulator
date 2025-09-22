using GlobalGameManager;
using UnityEngine;
using Utilities;

public class EnemyChase : MonoBehaviour
{
    public float speed = 3f;
    public float stopDistance = 1.5f;

    private Transform player;
    private bool canChase = true;

    private Vector3 spawnPosition; // 记录初始出生点
    private Quaternion spawnRotation;

    private void Awake()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

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

    private void HandlePlayerRebirth(GameObject newPlayer)
    {
        player = newPlayer.transform;
        canChase = true;
        Debug.Log("敌人重新锁定玩家并恢复追逐");
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

    private void ResetToSpawnPoint()
    {
        canChase = false;
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        Debug.Log($"{name} 已回到出生点（游戏结束）");
    }
}

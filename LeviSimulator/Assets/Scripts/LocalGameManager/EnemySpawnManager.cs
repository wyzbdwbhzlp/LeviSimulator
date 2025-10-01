using UnityEngine;
using Utilities;

public class EnemySpawnManager : MonoBehaviour
{
    private Transform currentSpawnPoint;

    private void OnEnable()
    {
        EventBroadcaster.OnPlayerSaved += HandlePlayerSaved;
    }

    private void OnDisable()
    {
        EventBroadcaster.OnPlayerSaved -= HandlePlayerSaved;
    }


    private void HandlePlayerSaved(Vector3 playerPos, Quaternion playerRot)
    {
        currentSpawnPoint = FindClosestSpawnPoint(playerPos);
        if (currentSpawnPoint != null)
        {
            Debug.Log("敌人出生点已更新为：" + currentSpawnPoint.position);
        }
        else
        {
            Debug.LogWarning("没有找到任何带 EnemySpawnPoint 标签的出生点！");
        }
    }
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

    public Vector3 GetSpawnPosition()
    {
        return currentSpawnPoint != null ? currentSpawnPoint.position : Vector3.zero;
    }
}

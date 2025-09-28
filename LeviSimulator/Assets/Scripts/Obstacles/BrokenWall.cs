using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class BrokenWall : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private List<GameObject> flashPoints = null;

    private bool broken = false;
    private Vector3 rootStartPos;
    private Quaternion rootStartRot;
    private Rigidbody rb;

    private void Awake()
    {
        rootStartPos = transform.position;
        rootStartRot = transform.rotation;

        // 确保有 Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true;
    }

    private void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();

        if (flashPoints != null)
        {
            foreach (GameObject point in flashPoints)
                point.SetActive(false);
        }
    }

    private void OnEnable()
    {
        EventBroadcaster.OnPlayerRebirth += ResetWallOnRebirth;
        EventBroadcaster.OnGameOver += ResetWallOnGameOver;
    }

    private void OnDisable()
    {
        EventBroadcaster.OnPlayerRebirth -= ResetWallOnRebirth;
        EventBroadcaster.OnGameOver -= ResetWallOnGameOver;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!broken && other.CompareTag("Player"))
        {
            broken = true;
            anim.SetBool("Crushed", true);

            // 开启整体重力
            rb.isKinematic = false;
            rb.useGravity = true;

            if (flashPoints != null)
            {
                foreach (GameObject point in flashPoints)
                    point.SetActive(true);
            }
        }
    }

    private void ResetWallOnRebirth(GameObject player)
    {
        ResetWall();
        Debug.Log($"{name} 玩家复活时重置");
    }

    private void ResetWallOnGameOver()
    {
        ResetWall();
        Debug.Log($"{name} 游戏结束时重置");
    }

    private void ResetWall()
    {
        broken = false;

        // 关闭重力 & 恢复位置
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;

        transform.position = rootStartPos;
        transform.rotation = rootStartRot;

        if (flashPoints != null)
        {
            foreach (GameObject point in flashPoints)
                point.SetActive(false);
        }

        // 重启 Animator 回到完整状态
        anim.enabled = true;
        anim.Rebind();
        anim.Update(0f);
        anim.SetBool("Crushed", false);
    }
}

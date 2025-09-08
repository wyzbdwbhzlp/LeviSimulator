using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class BrokenWall : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private Transform boneRoot;
    [SerializeField] private List<GameObject> flashPoints=null;
    private bool broken = false;

    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        foreach (GameObject point in flashPoints)
        {
            point.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!broken && other.CompareTag("Player"))
        {
            broken = true;
            anim.SetBool("Crushed", true);

            // 延迟到动画碎裂瞬间
            Invoke(nameof(DetachFragments), 0.5f);
            if (flashPoints != null)
            {
                foreach (GameObject point in flashPoints)
                { 
                   point.SetActive(true);
                }
            }
        }
    }

    void DetachFragments()
    {
        anim.enabled = false;

        foreach (Transform piece in boneRoot)
        {
            piece.parent = null;
            piece.AddComponent<Rigidbody>();
            Rigidbody rb = piece.GetComponent<Rigidbody>();
            if (rb == null) rb = piece.gameObject.AddComponent<Rigidbody>();

            rb.constraints = RigidbodyConstraints.None;
            rb.useGravity = true;
            rb.WakeUp();

            Collider col = piece.GetComponent<Collider>();
            if (col == null)
            {
                // BoxCollider 更稳定，MeshCollider 用 convex 会吃性能
                BoxCollider box = piece.gameObject.AddComponent<BoxCollider>();
                box.size *= 0.9f; // 稍微缩小避免互相卡住
            }

            // 微调初始位置避免重叠
            piece.position += Random.insideUnitSphere * 0.01f;

            // 爆炸力 (更强 + 冲击)
            boneRoot.gameObject.AddComponent<Rigidbody>();
            boneRoot.gameObject.GetComponent<Rigidbody>().AddExplosionForce(50f, boneRoot.transform.position, 5f, 0.5f);
            rb.AddExplosionForce(50f, piece.position, 5f, 0.5f);
        }
    }



}

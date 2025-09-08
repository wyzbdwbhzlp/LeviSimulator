using System.Collections;
using UnityEngine;

public class WallDissolveController : MonoBehaviour
{
    [Header("周期设置")]
    public float visibleDuration = 3f;   // 墙出现持续时间
    public float hiddenDuration = 2f;    // 墙消失持续时间
    public float smokeHiddenDuration = 2f;
    public float smokeVisibleDuration = 1f;

    [Header("对象设置")]
    public GameObject flashWall;
    public GameObject smoke;

    [Header("延迟设置")]
    public float startDelay = 0f;  // 初始延迟（只执行一次）

    private Coroutine cycleCoroutine;
    private bool isAppearing = true;

    void Start()
    {
        if (!flashWall || !smoke)
        {
            Debug.LogError("请在 Inspector 设置 Wall 和 Smoke");
            enabled = false;
            return;
        }

        flashWall.SetActive(false);
        smoke.SetActive(false);

        // 只延迟一次再进入循环
        cycleCoroutine = StartCoroutine(StartWithDelay());
    }

    IEnumerator StartWithDelay()
    {
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }
        cycleCoroutine = StartCoroutine(VisibilityCycle());
    }

    IEnumerator VisibilityCycle()
    {
        while (true)
        {
            if (isAppearing)
            {
                Debug.Log($"{gameObject.name} 显示阶段");
                flashWall.SetActive(true);
                smoke.SetActive(false);
                yield return new WaitForSeconds(visibleDuration);

                smoke.SetActive(true);
                yield return new WaitForSeconds(smokeVisibleDuration);

                isAppearing = false;
            }
            else
            {
                Debug.Log($"{gameObject.name} 隐藏阶段");
                flashWall.SetActive(false);
                smoke.SetActive(false);
                yield return new WaitForSeconds(hiddenDuration);

                smoke.SetActive(true);
                yield return new WaitForSeconds(smokeHiddenDuration);

                isAppearing = true;
            }
        }
    }
}

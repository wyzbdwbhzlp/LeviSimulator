using System.Collections;
using UnityEngine;

public class WallDissolveController : MonoBehaviour
{
    [Header("周期设置")]
    public float visibleDuration = 3f;   // 墙出现持续时间
    public float hiddenDuration = 2f;    // 墙消失持续时间
    public float smokeHiddenDuration = 2f;
    public float smokeVisibleDuration = 1f;
    public float dissolveSpeed = 1f;     // Dissolve 速度

    [Header("Object设置")]
    public GameObject flashWall;  
    public GameObject smoke;      

    [Header("Collider设置")]
    public Collider wallCollider;        // 墙体碰撞体，可为空

    private Coroutine cycleCoroutine;
    private bool isAppearing = true;
    private float timer = 0f;

    void Start()
    {
        flashWall.SetActive(true);
        smoke.SetActive(false);
        if (!flashWall || !smoke)
        {
            Debug.LogError("请在 Inspector 设置 Wall和 Somke");
            enabled = false;
            return;
        }
        
        cycleCoroutine = StartCoroutine(VisibilityCycle());
    }

    void Update()
    {
        
    
    }
    IEnumerator VisibilityCycle()
    {
        while (true)
        {
            if (isAppearing)
            {
                Debug.Log("显示阶段");
                flashWall.SetActive(true);
                smoke.SetActive(false);
                yield return new WaitForSeconds(visibleDuration);
                smoke.SetActive(true);
                yield return new WaitForSeconds(smokeVisibleDuration);
                isAppearing = false;
            }
            else 
            {
                Debug.Log("隐藏阶段");
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

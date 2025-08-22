using UnityEngine;

public class WallDissolveController : MonoBehaviour
{
    [Header("周期设置")]
    public float visibleDuration = 3f;   // 墙出现持续时间
    public float hiddenDuration = 2f;    // 墙消失持续时间
    public float dissolveSpeed = 1f;     // Dissolve 速度

    [Header("材质设置")]
    public Material baseMaterial;        // 内层 Lit 材质
    public Material overlayMaterial;     // 外层 Dissolve Shader 材质

    [Header("Collider设置")]
    public Collider wallCollider;        // 墙体碰撞体，可为空

    private float dissolveAmount = 1f;   // 当前消融值，0=完全出现, 1=完全消失
    private bool isAppearing = true;
    private float timer = 0f;

    void Start()
    {
        if (!baseMaterial || !overlayMaterial)
        {
            Debug.LogError("请在 Inspector 设置 baseMaterial 和 overlayMaterial");
            enabled = false;
            return;
        }

        dissolveAmount = 1f; // 初始消失
        overlayMaterial.SetFloat("_DissolveAmount", dissolveAmount);
        SetMaterialAlpha(baseMaterial, 0f);

        if (wallCollider) wallCollider.enabled = false;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 更新 dissolveAmount
        if (isAppearing)
        {
            dissolveAmount -= Time.deltaTime * dissolveSpeed;
            if (dissolveAmount <= 0f)
            {
                dissolveAmount = 0f;
                if (timer >= visibleDuration)
                {
                    isAppearing = false;
                    timer = 0f;
                }
            }
        }
        else
        {
            dissolveAmount += Time.deltaTime * dissolveSpeed;
            if (dissolveAmount >= 1f)
            {
                dissolveAmount = 1f;
                if (timer >= hiddenDuration)
                {
                    isAppearing = true;
                    timer = 0f;
                }
            }
        }

        // 更新材质
        overlayMaterial.SetFloat("_DissolveAmount", dissolveAmount);
        SetMaterialAlpha(baseMaterial, 1f - dissolveAmount);

        // 控制碰撞体
        if (wallCollider)
        {
            wallCollider.enabled = dissolveAmount < 0.99f;
        }
    }

    void SetMaterialAlpha(Material mat, float alpha)
    {
        if (mat.HasProperty("_Color"))
        {
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;

            // URP Lit 透明设置
            mat.SetFloat("_Surface", 1); // 1 = Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
    }
}

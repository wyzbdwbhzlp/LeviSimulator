using UnityEngine;
using Utilities;

namespace UIManager
{
    public class UIAssetLoader
    {
        public GameObject LoadUIPrefab(string prefabPath, string prefabName)
        {
            //todo 替换为yooasset加载
            GameObject prefab = null;
            if (!string.IsNullOrWhiteSpace(prefabPath))
            {
                prefab = Resources.Load<GameObject>(prefabPath);
                if (prefab == null)
                    LogUtil.LogError($"Resources 未找到预制体路径: {prefabPath}", true);
            }
            if (!string.IsNullOrWhiteSpace(prefabName)&& prefab == null)
            {
                LogUtil.LogWarning($"通过{prefabPath}加载失败，尝试按名称加载 {prefabName}");
                // 名称加载：约定在 Resources/UI 或 Resources/HUD 下
                prefab = Resources.Load<GameObject>($"UI/{prefabName}") ??
                         Resources.Load<GameObject>($"HUD/{prefabName}") ??
                         Resources.Load<GameObject>(prefabName);
#if UNITY_EDITOR
                if (prefab == null)
                {
                    LogUtil.LogWarning($"{prefabName} 未能通过 Resources 加载，尝试在编辑器中按名称查找（仅编辑器有效）");
                    var guids = UnityEditor.AssetDatabase.FindAssets($"{prefabName} t:prefab");
                    if (guids != null && guids.Length > 0)
                    {
                        var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                        prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    }
                }
#endif
                if (prefab == null)
                    LogUtil.LogError($"未找到名为 {prefabName} 的预制体（请放入 Resources 并传入相对路径，或在编辑器中检查名称是否匹配）", true);
            }

            return prefab;
        }
    }
}
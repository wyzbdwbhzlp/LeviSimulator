using UnityEngine;
using Utilities;

namespace DataCalculate
{

    public class BuffableScannerComponent : MonoBehaviour
    {
        private BuffableFieldManager buffableFieldManager;

        private void Start()
        {
            buffableFieldManager = BuffableFieldManager.Instance;
            if (buffableFieldManager == null)
            {
                LogUtil.LogError("初始化扫描时未找到BuffableFieldManager实例");
                return;
            }

            var scripts = GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script == this) continue;

                buffableFieldManager.ScanBuffableFields(script);
            }
        }

        private void OnDestroy()
        {
            if (buffableFieldManager == null) return;

            var scripts = GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script != null)
                {
                    buffableFieldManager.UnregisterTarget(script);
                }
            }
        }
    }
}
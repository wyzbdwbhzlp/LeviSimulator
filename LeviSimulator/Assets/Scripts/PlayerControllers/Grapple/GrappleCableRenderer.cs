using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace PlayerControllers.Grapple
{
    public class GrappleCableRenderer: MonoBehaviour
    {
        [Header("渲染设定")]
        [SerializeField][SceneObjectsOnly]private LineRenderer lineRenderer;
        [SceneObjectsOnly]private IGrapple grappleHook;
        [SerializeField] [LabelText("有效钩爪线材质")]private Material validGrappleCableMaterial; // 钩爪线材质
        [SerializeField] [LabelText("无效钩爪线材质")]private Material invalidGrappleCableMaterial; // 无效钩爪线材质
        
        public void Start()
        {
            if (lineRenderer == null)
            {
                LogUtil.LogError("线渲染器未设置，请检查配置。", true);
            }
            if (grappleHook == null)
            {
                grappleHook = GetComponentInParent<IGrapple>();
                if (grappleHook == null)
                {
                    LogUtil.LogError("钩爪组件未设置，请检查配置。", true);
                }
            }
        }
        public void UpdateCable()
        {

            if (grappleHook.GrappleState!=GrappleState.Idle&&!grappleHook.IsInvalidGrapple)
            {
                lineRenderer.enabled = true;
                lineRenderer.material = validGrappleCableMaterial; // 设置有效钩爪线材质
                lineRenderer.SetPosition(0, grappleHook.GrappleTipTransform.position);
                lineRenderer.SetPosition(1, grappleHook.GrapplePoint);
            }
            else if(grappleHook.GrappleState!=GrappleState.Idle&&grappleHook.IsInvalidGrapple)
            {
                lineRenderer.enabled = true;
                lineRenderer.material = invalidGrappleCableMaterial; // 设置无效钩爪线材质
                lineRenderer.SetPosition(0, grappleHook.GrappleTipTransform.position);
                lineRenderer.SetPosition(1, grappleHook.GrapplePoint);
            }
            else
            {
                lineRenderer.enabled = false;
            }
        }
    }
}
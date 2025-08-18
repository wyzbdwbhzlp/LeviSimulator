using Manager;
using Sirenix.OdinInspector;
using UIManager;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities;

namespace CollectibleEcho
{
    public class CollectibleEchoController : Singleton<CollectibleEchoController>
    {
        [Title("echo数据配置")]
        [SerializeField] private SO.CollectibleEchoSO _collectibleEchoDataBase;
        [Title("当前回声数据")]
        [SerializeField] [ReadOnly] [LabelText("当前播放的Echo")]
        private SO.CollectibleEcho _currentCollectibleEchoData;

        protected override void Awake()
        {
            base.Awake();
            if (_collectibleEchoDataBase == null)
            {
                LogUtil.LogError("回声数据库未设置，请检查配置。", true);
            }
            DontDestroyOnLoad(gameObject);
        }
        [Button("打开回声UI")]
        public void OpenEcho(int echoId)
        {
            var echoData= _collectibleEchoDataBase.GetCollectibleEchoByID(echoId);
            if (echoData == null)
            {
                LogUtil.LogError($"未找到ID为{echoId}的回声数据，请检查配置。", true);
                return;
            }
            var echoDisplayViewUI =MainUIManager.ShowUIComponent<EchoDisplayViewUI>();
            
            if (echoDisplayViewUI != null)
            {
                echoDisplayViewUI.ShowUIPanel(echoData);
                EventBroadcaster.CallEchoViewUIOpened();
            }
        }
        [Button("关闭回声UI")]
        public void CloseEcho()
        {
            MainUIManager.HideUIComponent<EchoDisplayViewUI>();
            EventBroadcaster.CallEchoViewUIClosed();
        }
    }
}
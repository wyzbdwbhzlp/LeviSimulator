using GlobalGameManager;
using Manager;
using PlayerControllers.Refactored;
using PlayerControllers.Refactored.Systems;
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
        [SerializeField] private SO.CollectibleEchoSO collectibleEchoDataBase;
        [Title("当前回声数据")]
        [SerializeField] [ReadOnly] [LabelText("当前播放的Echo")]
        private SO.CollectibleEcho currentCollectibleEchoData;

        protected override void Awake()
        {
            base.Awake();
            if (collectibleEchoDataBase == null)
            {
                LogUtil.LogError("回声数据库未设置，请检查配置。", true);
            }
            DontDestroyOnLoad(gameObject);
        }
        [Button("打开回声UI")]
        public void OpenEcho(int echoId)
        {
            var echoData= collectibleEchoDataBase.GetCollectibleEchoByID(echoId);
            if (echoData == null)
            {
                LogUtil.LogError($"未找到ID为{echoId}的回声数据，请检查配置。", true);
                return;
            }
            var echoDisplayViewUI =MainUIManager.ShowUIComponent<EchoDisplayViewUI>();
            
            if (echoDisplayViewUI != null)
            {
                echoDisplayViewUI.CloseButton.onClick.RemoveAllListeners();
                echoDisplayViewUI.CloseButton.onClick.AddListener(CloseEcho);
                
                echoDisplayViewUI.ShowUIPanel(echoData);
                PlayerController.UnlockAndShowCursor(); //锁定并隐藏鼠标
                EventBroadcaster.CallEchoViewUIOpened(echoId);
            }
        }
        [Button("关闭回声UI")]
        public void CloseEcho()
        {
            PlayerController.LockAndHideCursor(); //解锁并显示鼠标
            MainUIManager.HideUIComponent<EchoDisplayViewUI>();
            EventBroadcaster.CallEchoViewUIClosed();
        }
    }
}
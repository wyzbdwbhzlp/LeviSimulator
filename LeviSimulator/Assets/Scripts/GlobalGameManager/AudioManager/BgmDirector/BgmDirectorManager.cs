using GlobalGameManager;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Game.Audio
{
    /// <summary>
    /// BGM 导演管理器：负责处理 BGM 导演相关的所有逻辑
    /// 包括触发点处理、动作执行等
    /// </summary>
    public class BgmDirectorManager : MonoBehaviour
    {
        [SerializeField] private BgmDirectorScriptableObject bgmDirector;
        
        private IAudioPlayer audioPlayer;

        private BgmDirectorContext _latestPlayBgmContext;//记录最新的播放BGM的Context,以便在需要时查询
        
        private SceneLoadManager sceneLoadManager;// 用于监听场景事件
        private GameStateManager gameStateManager;// 用于监听游戏状态变化
        /// <summary>
        /// 初始化 BGM 导演管理器
        /// </summary>
        /// <param name="player">音频播放器接口</param>
        public void Initialize(IAudioPlayer player)
        {
            audioPlayer = player;
            
            if (bgmDirector == null)
            {
                LogUtil.LogWarning("BgmDirectorManager: bgmDirector 未设置");
            }
        }
        
        private void OnEnable()
        {
            SubscribeToBgmDirectorEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromBgmDirectorEvents();
        }
        
        private void SubscribeToBgmDirectorEvents()
        {
            AudioEventHandler.BgmDirectorTriggerPointReached += HandleBgmDirectorTrigger;
            AudioEventHandler.BgmDirectorActionTriggered += HandleBgmDirectorAction;
            if(GlobalManager.Instance.sceneLoadManager!=null)
            {
                sceneLoadManager = GlobalManager.Instance.sceneLoadManager;
                sceneLoadManager.OnSceneLoadCompleted+= HandleSceneLoadCompleted;
            }
            if(GlobalManager.Instance.gameStateManager!=null)
            {
                gameStateManager = GlobalManager.Instance.gameStateManager;
                gameStateManager.OnStateChanged+= HandleGameStateChanged;
            }

            //tip 临时方法,不推荐长期使用
            EventBroadcaster.EchoViewUIOpened += ModifyBgmClipBecauseLevel5Echo;
        }

        private void HandleGameStateChanged(GameState arg1, GameState arg2)
        {
            if(arg2==GameState.GameOver)
            {
                HandleBgmDirectorAction(BgmDirectorEnum.StopBgm,null);
            }
            else if(arg1==GameState.GameOver&&arg2==GameState.InGame) //玩家从GameOver状态复活
            {
                if(_latestPlayBgmContext!=null)
                {
                    HandleBgmDirectorAction(BgmDirectorEnum.PlayBgm,_latestPlayBgmContext);
                }
            }
        }

        private void UnsubscribeFromBgmDirectorEvents()
        {
            AudioEventHandler.BgmDirectorTriggerPointReached -= HandleBgmDirectorTrigger;
            AudioEventHandler.BgmDirectorActionTriggered -= HandleBgmDirectorAction;
            if(sceneLoadManager!=null)
            {
                sceneLoadManager.OnSceneLoadCompleted-= HandleSceneLoadCompleted;
            }
            if(gameStateManager!=null)
            {
                gameStateManager.OnStateChanged-= HandleGameStateChanged;
            }
            EventBroadcaster.EchoViewUIOpened -= ModifyBgmClipBecauseLevel5Echo;
        }

        private void HandleSceneLoadCompleted(SceneEnum sceneEnum)
        {
            switch (sceneEnum)
            {
                case SceneEnum.MainMenu:
                    HandleBgmDirectorTrigger(BgmDirectorTriggerPointEnum.OnMainMenu);
                    break;
                case SceneEnum.Home:
                    HandleBgmDirectorTrigger(BgmDirectorTriggerPointEnum.OnHome);
                    break;
                case SceneEnum.Level1:
                    HandleBgmDirectorTrigger(BgmDirectorTriggerPointEnum.OnLevel1);
                    break;
                case SceneEnum.Level2zhuizhu:
                    HandleBgmDirectorTrigger(BgmDirectorTriggerPointEnum.OnLevel2);
                    break;
                case SceneEnum.Level3bianhuan:
                    HandleBgmDirectorTrigger(BgmDirectorTriggerPointEnum.OnLevel3);
                    break;
                case SceneEnum.Level4pohuai:
                    HandleBgmDirectorTrigger(BgmDirectorTriggerPointEnum.OnLevel4);
                    break;
                case SceneEnum.Level5end:
                    HandleBgmDirectorTrigger(BgmDirectorTriggerPointEnum.OnLevel5);
                    break;
            }
        }

        /// <summary>
        /// 处理 BGM 导演触发点
        /// </summary>
        /// <param name="triggerPoint">触发点枚举</param>
        private void HandleBgmDirectorTrigger(BgmDirectorTriggerPointEnum triggerPoint)
        {
            if (bgmDirector == null)
            {
                LogUtil.LogWarning("BgmDirectorManager: bgmDirector 未设置,无法处理触发点");
                return;
            }
            
            var data = bgmDirector.GetBgmDirectorDataByPoint(triggerPoint);
            switch (data.TriggerAction)
            {
                case BgmDirectorEnum.PlayBgm:
                    _latestPlayBgmContext = data.Context; //记录最新的播放BGM的Context
                    HandleBgmDirectorAction(data.TriggerAction, data.Context);
                    break;
                case BgmDirectorEnum.StopBgm:
                    HandleStopBgmAction();
                    break;
            }
            
            
            // 根据触发点获取对应的动作并执行
            LogUtil.Log($"BGM导演触发点 {triggerPoint} 被触发,执行动作: {data.TriggerAction}");
        }
        
        /// <summary>
        /// 处理 BGM 导演动作
        /// </summary>
        /// <param name="action">动作类型</param>
        /// <param name="context">上下文信息</param>
        private void HandleBgmDirectorAction(BgmDirectorEnum action, BgmDirectorContext context)
        {
            if (bgmDirector == null)
            {
                LogUtil.LogWarning("BgmDirectorManager: bgmDirector 未设置,无法处理动作");
                return;
            }
            
            if (audioPlayer == null)
            {
                LogUtil.LogError("BgmDirectorManager: audioPlayer 未初始化,无法播放音频");
                return;
            }
            
            switch (action)
            {
                case BgmDirectorEnum.NoAction:
                    return;
                    
                case BgmDirectorEnum.PlayBgm:
                    HandlePlayBgmAction(context);
                    break;
                    
                case BgmDirectorEnum.StopBgm:
                    HandleStopBgmAction();
                    break;
                default:
                    LogUtil.LogWarning($"BGM导演收到未处理的动作类型: {action}");
                    break;
            }
        }
        
        /// <summary>
        /// 处理播放 BGM 动作
        /// </summary>
        /// <param name="context">上下文信息</param>
        private void HandlePlayBgmAction(BgmDirectorContext context)
        {
            if (context == null)
            {
                LogUtil.LogWarning("BGM导演尝试播放BGM,但context为空");
                return;
            }
            
            var bgmName = context.bgmName;
            if (string.IsNullOrEmpty(bgmName))
            {
                LogUtil.LogWarning("BGM导演尝试播放BGM,但bgmName为空");
                return;
            }
            
            var bgmClip = bgmDirector.GetBgmClipByName(bgmName);
            if (bgmClip == null)
            {
                LogUtil.LogWarning($"BGM导演尝试播放BGM '{bgmName}',但未找到对应的音频片段");
                return;
            }
            
            // 检查是否指定了起始时间
            float startTime = context.bgmStartTimeSeconds;
            float fadeSeconds = context.bgmFadeDuration;
            float targetVolume = 1f;   // 可以从 context 中获取,或使用默认值
            
            if (startTime > 0f)
            {
                audioPlayer.PlayBgmWithStartTime(bgmClip, startTime, fadeSeconds, targetVolume);
                LogUtil.Log($"BGM导演播放BGM '{bgmName}' (起始时间: {startTime}秒)");
            }
            else
            {
                audioPlayer.PlayBGM(bgmClip, fadeSeconds, targetVolume);
                LogUtil.Log($"BGM导演播放BGM '{bgmName}'");
            }
            
            // 触发BGM开始事件
            context.onBgmStart?.Invoke();
        }
        
        /// <summary>
        /// 处理停止 BGM 动作
        /// </summary>
        private void HandleStopBgmAction(float fadeSeconds = 1f)
        {
            audioPlayer.StopBGM(fadeSeconds);
            LogUtil.Log("BGM导演停止BGM");
        }
        
        /// <summary>
        /// 设置 BGM 导演配置
        /// </summary>
        /// <param name="director">BGM 导演配置</param>
        public void SetBgmDirector(BgmDirectorScriptableObject director)
        {
            bgmDirector = director;
        }
        
        /// <summary>
        /// 获取当前 BGM 导演配置
        /// </summary>
        public BgmDirectorScriptableObject GetBgmDirector()
        {
            return bgmDirector;
        }

        #region 临时方法,不推荐长期使用
        

        [Button(" 测试因Echo改变Bgm")]
        public void ModifyBgmClipBecauseLevel5Echo(int echoId)
        {
            if(echoId!=14)return;
            bgmDirector.ModifyBgmClipByTriggerPoint(BgmDirectorTriggerPointEnum.OnLevel5, "TZC_我可是世界冠军");
            
            HandleStopBgmAction();
            HandleBgmDirectorTrigger(BgmDirectorTriggerPointEnum.OnLevel5);
            
            LogUtil.Log("BGM导演已修改 Level5Echo 触发点的 BGM");
        }

        #endregion
       
    }
}

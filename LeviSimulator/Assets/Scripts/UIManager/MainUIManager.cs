using System;
using System.Collections.Generic;
using CollectibleEcho;
using HUD;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using GlobalGameManager;
using System.Reflection; // 新增

namespace UIManager
{
    public class MainUIManager:Singleton<MainUIManager>
    {
        
        [Header("运行时参数")]
        [ShowInInspector]private Dictionary<Type, (IUIComponent,GameObject,bool)> _uiComponentsDic = new Dictionary<Type, (IUIComponent, GameObject,bool)>();
        [ShowInInspector]private Dictionary<Type, IHUDComponent> _hudComponentsDic = new();
        [ShowInInspector]private Stack<Type> _uiComponentStack = new Stack<Type>(); // UI组件栈，用于管理UI组件的显示和隐藏
        [Header("实例化父节点（可选）")]
        [SerializeField] private Transform uiRoot; // 用于放置按特性实例化出来的UI
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
        public void Initialize()
        {
            InitUiComponentsDic();
            InitHUDComponentsDic();
            // 订阅游戏状态变化（若可用）
            var gm = GlobalManager.Instance;
            if (gm != null && gm.gameStateManager != null)
            {
                gm.gameStateManager.OnStateChanged += OnGameStateChanged;
            }
        }
        public static T ShowUIComponent<T>() where T : class, IUIComponent
        {
            return Instance?.ShowUIComponent(typeof(T)) as T;
        }
    
        public static void HideUIComponent<T>() where T : class, IUIComponent
        {
            Instance?.HideUIComponent(typeof(T));
        }
    
        public static T ShowHUDComponent<T>() where T : class, IHUDComponent
        {
            return Instance?.ShowHUDComponent(typeof(T)) as T;
        }
    
        public static void HideHUDComponent<T>() where T : class, IHUDComponent
        {
            Instance?.HideHUDComponent(typeof(T));
        }

        private void OnDestroy()
        {
            var gm = GlobalManager.Instance;
            if (gm != null && gm.gameStateManager != null)
            {
                gm.gameStateManager.OnStateChanged -= OnGameStateChanged;
            }
        }

        private void Update()
        {
            // ESC 关闭顶层UI
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PopOneUIComponent();
            }
        }

        protected void InitUiComponentsDic()
        {
            var UIfields = GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in UIfields)
            {
                if (field.GetCustomAttributes(typeof(UIComponentAttribute), false).Length > 0)
                {
                    (IUIComponent,GameObject,bool) value = (null, null,false);
                    var uiComponent = field.GetValue(this) as IUIComponent;

                    // 新增：若未赋值，尝试按特性加载并实例化
                    if (uiComponent == null)
                    {
                        var attr = (UIComponentAttribute)field.GetCustomAttribute(typeof(UIComponentAttribute), false);
                        uiComponent = TryLoadUIComponentByAttribute(attr, out var go, GetParent(attr));
                        if (uiComponent != null)
                        {
                            // 回写字段，便于后续直接使用
                            field.SetValue(this, uiComponent);
                        }
                    }

                    if (uiComponent != null)
                    {
                        value.Item1 = uiComponent;
                        if (uiComponent is MonoBehaviour monoBehaviour)
                        {
                            value.Item2= monoBehaviour.gameObject;
                            value.Item2.SetActive(false);
                            value.Item3 = false; 
                            _uiComponentsDic.Add(field.FieldType, value);
                        }
                        else
                        {
                            LogUtil.LogError($"UI组件 {uiComponent} 不是 MonoBehaviour，无法获取 GameObject", true);
                        }
                    }else
                    {
                        LogUtil.LogError($"UI组件 {field.Name} 未设置或为 null", true);
                    }
                }
            }
        }
        protected void InitHUDComponentsDic()
        {
            var hudFields = GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in hudFields)
            {
                if (field.GetCustomAttributes(typeof(HUDAttribute), false).Length > 0)
                {
                    var hudComponent = field.GetValue(this) as IHUDComponent;

                    // 新增：若未赋值，尝试按特性加载并实例化
                    if (hudComponent == null)
                    {
                        var attr = (HUDAttribute)field.GetCustomAttribute(typeof(HUDAttribute), false);
                        hudComponent = TryLoadHUDComponentByAttribute(attr, out var go, GetParent(attr));
                        if (hudComponent != null)
                        {
                            field.SetValue(this, hudComponent);
                        }
                    }

                    if (hudComponent != null)
                    {
                        if (hudComponent is MonoBehaviour monoBehaviour)
                        {
                            if (hudComponent.IsDefaultHide)
                            {
                                hudComponent.HideHUD();
                            }
                            _hudComponentsDic.Add(field.FieldType, hudComponent);
                        }
                        else
                        {
                            LogUtil.LogError($"HUD组件 {hudComponent} 不是 MonoBehaviour，无法管理", true);
                        }
                    }
                    else
                    {
                        LogUtil.LogError($"HUD组件 {field.Name} 未设置或为 null", true);
                    }
                }
            }
        }

        // ===== 新增：基于特性的加载与实例化辅助 =====

        private Transform GetParent(BasePrefabAttribute attr)
        {
            var root = uiRoot != null ? uiRoot : transform;
            if (attr == null || string.IsNullOrEmpty(attr.ParentPath)) return root;
            var found = root.Find(attr.ParentPath);
            return found != null ? found : root;
        }

        private static GameObject LoadPrefab(string prefabPath, string prefabName)
        { //todo 替换为yooasset加载
            GameObject prefab = null;
            if (!string.IsNullOrWhiteSpace(prefabPath))
            {
                prefab = Resources.Load<GameObject>(prefabPath);
                if (prefab == null)
                    LogUtil.LogError($"Resources 未找到预制体路径: {prefabPath}", true);
            }
            else if (!string.IsNullOrWhiteSpace(prefabName))
            {
                // 名称加载：约定在 Resources/UI 或 Resources/HUD 下
                prefab = Resources.Load<GameObject>($"UI/{prefabName}") ??
                         Resources.Load<GameObject>($"HUD/{prefabName}") ??
                         Resources.Load<GameObject>(prefabName);
#if UNITY_EDITOR
                if (prefab == null)
                {
                    // 编辑器回退：按名称检索工程内任意 prefab（运行库不生效）
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

        private static T InstantiateAndBind<T>(GameObject prefab, Transform parent) where T : Component
        {
            if (prefab == null) return null;
            var go = Instantiate(prefab, parent, false);
            go.name = prefab.name;
            go.SetActive(false); // 与现有流程一致，初始化时先隐藏
            var comp = go.GetComponent<T>();
            if (comp == null)
                LogUtil.LogError($"实例 {go.name} 上未找到组件 {typeof(T).Name}", true);
            return comp;
        }

        private IUIComponent TryLoadUIComponentByAttribute(UIComponentAttribute attr, out GameObject go, Transform parent)
        {
            go = null;
            if (attr == null) return null;
            var prefab = LoadPrefab(attr.PrefabPath, attr.PrefabName);
            var comp = InstantiateAndBind<MonoBehaviour>(prefab, parent);
            go = comp != null ? comp.gameObject : null;
            return comp as IUIComponent;
        }

        private IHUDComponent TryLoadHUDComponentByAttribute(HUDAttribute attr, out GameObject go, Transform parent)
        {
            go = null;
            if (attr == null) return null;
            var prefab = LoadPrefab(attr.PrefabPath, attr.PrefabName);
            var comp = InstantiateAndBind<MonoBehaviour>(prefab, parent);
            go = comp != null ? comp.gameObject : null;
            return comp as IHUDComponent;
        }
        protected IUIComponent ShowUIComponent(Type componentType)
        {
            if (_uiComponentsDic.TryGetValue(componentType, out var component))
            {
                if (component.Item3)
                {
                    // 已经显示则直接返回，避免重复入栈
                    return component.Item1;
                }

                // 移除旧位置，推入栈顶
                RemoveFromStack(componentType);
                component.Item2.SetActive(true);
                component.Item3 = true;
                _uiComponentStack.Push(componentType);
                return component.Item1;
            }
            
            LogUtil.LogError($"UI组件 {componentType.FullName} 未找到。", true);
            return null;
        }
        protected void HideUIComponent(Type componentType)
        {
            if (_uiComponentsDic.TryGetValue(componentType, out var component))
            {
                component.Item1.CloseUIPanel();
                component.Item2.SetActive(false);
                component.Item3 = false;
                // 无论是否在栈顶，都从栈中移除该类型的所有记录
                RemoveFromStack(componentType);
            }
            else
            {
                LogUtil.LogError($"UI组件 {componentType.FullName} 未找到。", true);
            }
        }
        protected void PopOneUIComponent()
        {
            if (_uiComponentStack.Count > 0)
            {
                var componentType = _uiComponentStack.Pop();
                HideUIComponent(componentType);
            }
            else
            {
                LogUtil.LogWarning("UI组件栈为空，无法弹出组件。");
            }
        }
        protected IHUDComponent ShowHUDComponent(Type componentType) 
        {
            if(_hudComponentsDic.TryGetValue(componentType, out var hudComponent))
            {
                hudComponent.IsHUDEnabled = true;
                hudComponent.ShowHUD();
                return hudComponent;
            }

            return null;
        }
        protected void HideHUDComponent(Type componentType) 
        {
            if(_hudComponentsDic.TryGetValue(componentType, out var hudComponent))
            {
                hudComponent.IsHUDEnabled = false;
                hudComponent.HideHUD();
            }
        }
        
        // 运行时注册/反注册 HUD 组件，便于动态加载的 HUD 管理
        public void RegisterHUDComponent<T>(T hudComponent) where T : class, IHUDComponent
        {
            var type = typeof(T);
            if (hudComponent == null)
            {
                LogUtil.LogError($"注册 HUD 失败：{type.FullName} 实例为 null", true);
                return;
            }
            if (!_hudComponentsDic.ContainsKey(type))
            {
                _hudComponentsDic.Add(type, hudComponent);
                if (hudComponent.IsDefaultHide)
                {
                    hudComponent.HideHUD();
                }
            }
        }

        public void UnregisterHUDComponent<T>() where T : class, IHUDComponent
        {
            var type = typeof(T);
            if (_hudComponentsDic.TryGetValue(type, out var hud))
            {
                hud.HideHUD();
                _hudComponentsDic.Remove(type);
            }
        }

        public bool HasActiveUI()
        {
            return _uiComponentStack.Count > 0;
        }

        public void CloseAllUI()
        {
            // 逐一关闭，确保 CloseUIPanel 被调用
            while (_uiComponentStack.Count > 0)
            {
                var type = _uiComponentStack.Pop();
                HideUIComponent(type);
            }
        }

        private void SetAllHUDEnabled(bool enabled)
        {
            foreach (var kv in _hudComponentsDic)
            {
                kv.Value.IsHUDEnabled = enabled;
                if (!enabled)
                {
                    kv.Value.HideHUD();
                }
                else
                {
                    kv.Value.ShowHUD();
                }
            }
        }

        private void OnGameStateChanged(GameState from, GameState to)
        {
            // 这里仅做基础联动，避免侵入具体UI：
            switch (to)
            {
                case GameState.InGame:
                    // 返回游戏时，关闭残留的顶层 UI
                    CloseAllUI();
                    break;
                case GameState.Paused:
                    // 可在此处显示暂停菜单（若接入），此处暂不强制处理
                    break;
                case GameState.GameOver:
                    // 由 DeathScreenHUD 自行展示，不在此全局处理
                    break;
            }
        }

        // 从栈中移除所有匹配类型（保持其余顺序）
        private void RemoveFromStack(Type t)
        {
            if (_uiComponentStack.Count == 0) return;
            var temp = new Stack<Type>();
            while (_uiComponentStack.Count > 0)
            {
                var x = _uiComponentStack.Pop();
                if (x != t) temp.Push(x);
            }
            while (temp.Count > 0)
            {
                _uiComponentStack.Push(temp.Pop());
            }
        }
        
    }


    public abstract class BasePrefabAttribute : Attribute
    {
        public string PrefabPath { get; }
        public string PrefabName { get; }
        public string ParentPath { get; }
        protected BasePrefabAttribute(string prefabPath = null, string prefabName = null, string parentPath = null)
        {
            PrefabPath = prefabPath;
            PrefabName = prefabName;
            ParentPath = parentPath;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class UIComponentAttribute: BasePrefabAttribute
    {
        public UIComponentAttribute(string prefabName = null, string prefabPath = null, string parentPath = null)
            : base(prefabPath, prefabName, parentPath) {}
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class HUDAttribute: BasePrefabAttribute
    {
        public HUDAttribute(string prefabName = null, string prefabPath = null, string parentPath = null)
            : base(prefabPath, prefabName, parentPath) {}
    }
}